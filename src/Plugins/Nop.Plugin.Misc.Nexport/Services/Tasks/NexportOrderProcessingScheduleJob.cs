using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class NexportOrderProcessingScheduleJob(
    EmailAccountSettings emailAccountSettings,
    LocalizationSettings localizationSettings,
    IWidgetPluginManager widgetPluginManager,
    ILogger logger,
    IAddressService addressService,
    IProductService productService,
    IOrderService orderService,
    IOrderProcessingService orderProcessingService,
    IStoreService storeService,
    ICustomerService customerService,
    IStateProvinceService stateProvinceService,
    ICountryService countryService,
    ISettingService settingService,
    IGenericAttributeService genericAttributeService,
    IRepository<NexportOrderProcessingQueueItem> nexportOrderProcessingQueueRepository,
    NexportService nexportService,
    INexportWholesaleService nexportWholesaleService,
    NexportSettings nexportSettings)
    : INexportScheduleJob
{
    private int _batchSize;

    public string JobName { get; set; } = "NexportOrderProcessing";

    public long Interval { get; set; } = 5; // Default to 5 seconds

    public async Task ExecuteAsync()
    {
        if (!await widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
        {
            await logger.WarningAsync("OrderProcessing job cannot be executed due to Nexport plugin is not currently active!");
            return;
        }

        try
        {
            _batchSize = await settingService.GetSettingByKeyAsync(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey,
                NexportDefaults.NexportOrderProcessingTaskBatchSize);

            var orders = (from q in nexportOrderProcessingQueueRepository.Table
                orderby q.UtcDateCreated
                where q.UtcProcessingDate == null || (q.UtcProcessingDate != null && q.UtcProcessingDate <= DateTime.UtcNow)
                select q.Id).Take(_batchSize).ToList();

            await ProcessNexportOrdersAsync(orders);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Cannot process Nexport redemption", ex);
        }
    }

    private struct AutoRedeemingInvoiceItem
    {
        public int Id;
        public int ProductMappingId;
        public int OrderItemId;
        public int? ExtensionAction;
    }

    public async Task ProcessNexportOrdersAsync(IList<int> queueItemIds)
    {
        try
        {
            foreach (var queueItemId in queueItemIds)
            {
                var completeOrder = false;

                await logger.InformationAsync($"Begin processing order processing queue item {queueItemId}");

                var queueItem = await nexportOrderProcessingQueueRepository.GetByIdAsync(queueItemId);

                if (queueItem == null)
                    continue;

                var order = await orderService.GetOrderByIdAsync(queueItem.OrderId);

                try
                {
                    if (order is { Deleted: false, OrderStatus: OrderStatus.Processing })
                    {
                        var store = await storeService.GetStoreByIdAsync(order.StoreId);

                        var isWholesale = await genericAttributeService.GetAttributeAsync<bool>(order, "IsWholesaleOrder", store.Id);
                        if (isWholesale)
                        {
                            completeOrder = await ProcessNexportWholesaleOrderAsync(queueItem, order, store);
                        }
                        else
                        {
                            completeOrder = await ProcessNexportRetailOrderAsync(queueItem, order, store);
                        }
                    }
                    else
                    {
                        await logger.WarningAsync($"Cannot find the order {queueItem.OrderId} or it is deleted or its order status is not set to processing.");
                    }

                    // Update the order with order notes. This does not complete the order yet.
                    await orderService.UpdateOrderAsync(order);

                    // Delete queue item
                    await nexportService.DeleteNexportOrderProcessingQueueItem(queueItem);

                    await logger.InformationAsync($"Order processing queue item {queueItem.Id} has been processed and removed!");

                    if (completeOrder)
                    {
                        // Finish the order process and set the status to Complete
                        await orderProcessingService.CheckOrderStatusAsync(order);
                    }
                }
                catch (Exception ex)
                {
                    await logger.ErrorAsync($"Cannot process NexportRedemptionProcessingQueue item with Id {queueItem.Id}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Cannot process the NexportRedemptionProcessingQueue", ex);
        }
    }

    public async Task<bool> ProcessNexportRetailOrderAsync(NexportOrderProcessingQueueItem queueItem, Order order, Store store)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem), "Queue item cannot be null!");

        if (order == null)
            throw new ArgumentNullException(nameof(order), "Order entity cannot be null!");

        if (store == null)
            throw new ArgumentNullException(nameof(store), "Store entity cannot be null!");

        var completeOrder = false;
        var requireManualApproval = false;

        await logger.InformationAsync($"Begin processing order #{order.Id}");

        var userMapping = await nexportService.FindUserMappingByCustomerId(order.CustomerId);

        if (userMapping != null)
        {
            await SynchronizeCustomerContactInformationAsync(userMapping);

            await nexportService.AddOrderNoteAsync(order, $"Nexport invoice has started processing for order #{order.Id} (Store: {store.Name})");

            var orgId = await genericAttributeService.GetAttributeAsync<Guid>(store, "NexportSubscriptionOrganizationId", store.Id);

            if (orgId == Guid.Empty)
            {
                // ReSharper disable once PossibleInvalidOperationException
                orgId = nexportSettings.RootOrganizationId.Value;
            }

            // Check if there is an existing invoice. If not, begin a new invoice transaction.
            var orderInvoiceId =
                await nexportService.FindExistingInvoiceForOrder(order.Id) ??
                await nexportService.BeginNexportOrderInvoiceTransactionAsync(orgId, userMapping.NexportUserId);

            // Get the invoice details from Nexport (if existing)
            var invoiceDetails = await nexportService.GetNexportInvoiceAsync(orderInvoiceId)!;

            // Continue to process only if the invoice is opening
            if (invoiceDetails == null ||
                (invoiceDetails.State != GetInvoiceResponse.StateEnum.Committed &&
                 invoiceDetails.State != GetInvoiceResponse.StateEnum.Failed))
            {
                decimal invoiceTotalCost = 0;

                var autoRedeemingInvoiceItems = new List<AutoRedeemingInvoiceItem>();

                var orderItems = await orderService.GetOrderItemsAsync(order.Id);
                foreach (var orderItem in orderItems)
                {
                    var mappingInfo = await genericAttributeService.GetAttributeAsync<string>(orderItem,
                        $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                    // Retrieve the stored mapping info if existed; otherwise, get the current mapping info
                    var mapping = mappingInfo != null ?
                        JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo) :
                        await nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
                        await nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                    if (mapping != null)
                    {
                        var product = await productService.GetProductByIdAsync(orderItem.ProductId);
                        if (product != null)
                        {
                            var productCost = product.ProductCost;
                            var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;

                            // Generate the listing of group membership identifiers
                            var groupMembershipIds = await GenerateGroupMembershipIds(order, orderItem, mapping);

                            // Find existing invoice item for the order item
                            var existingInvoiceItemId =
                                await nexportService.FindExistingInvoiceItemForOrderItem(order.Id,
                                    orderItem.Id);

                            // If the invoice item does not exist, then add the order item into the invoice
                            if (existingInvoiceItemId == null || !invoiceDetails.InvoiceItems.Any(i => i.Id == existingInvoiceItemId))
                            {
                                var redemptionAvailableDate = await genericAttributeService.GetAttributeAsync<DateTime?>(order, "NexportEnrollmentStartDate", order.StoreId);

                                var (invoiceItemIds, _, requiringManualApproval, extensionAction) =
                                    await AddItemToNexportInvoiceWithQuantityAsync(
                                        mapping, userMapping,
                                        orderInvoiceId, productCost, subscriptionOrgId,
                                        groupMembershipIds, redemptionAvailableDate: redemptionAvailableDate);

                                if (invoiceItemIds is { Count: > 0 })
                                {
                                    // Get the first invoice item ID since this should be the only one for retail orders
                                    var invoiceItemId = invoiceItemIds.First();

                                    var nexportOrderInvoiceItem = new NexportOrderInvoiceItem
                                    {
                                        OrderId = queueItem.OrderId,
                                        OrderItemId = orderItem.Id,
                                        InvoiceItemId = invoiceItemId,
                                        InvoiceId = orderInvoiceId,
                                        UtcDateProcessed = DateTime.UtcNow,
                                        RequireManualApproval = requiringManualApproval,
                                        RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Available
                                    };

                                    // This allows the task to automatically process redemption as restarting the enrollment
                                    // in which the customer currently does not meet the enrollment completion threshold
                                    // no matter what the approval method is.

                                    await nexportService.InsertOrUpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);

                                    if (mapping.AutoRedeem)
                                    {
                                        // Add the invoice item for auto redeeming after committing the invoice
                                        // if AutoRedeem is set on the mapping and the renewal approval method is not defined
                                        // or the approval method is set to be Auto
                                        if (!nexportOrderInvoiceItem.RequireManualApproval.HasValue ||
                                            !nexportOrderInvoiceItem.RequireManualApproval.Value)
                                        {
                                            autoRedeemingInvoiceItems.Add(new AutoRedeemingInvoiceItem
                                            {
                                                Id = nexportOrderInvoiceItem.Id,
                                                ProductMappingId = mapping.Id,
                                                OrderItemId = orderItem.Id,
                                                ExtensionAction = extensionAction
                                            });
                                        }
                                        else
                                        {
                                            // Set this to true in order to prevent completing the order
                                            requireManualApproval = true;
                                        }
                                    }
                                    else
                                    {
                                        // Order invoice item that does not do automatically redemption
                                        // still require manual approval if set
                                        if (nexportOrderInvoiceItem.RequireManualApproval.HasValue &&
                                            nexportOrderInvoiceItem.RequireManualApproval.Value)
                                        {
                                            // Set this to true in order to prevent completing the order
                                            requireManualApproval = true;
                                        }
                                    }

                                    invoiceTotalCost += productCost;
                                }
                            }
                        }
                    }
                }

                // Add payment
                await nexportService.AddPaymentToNexportOrderInvoiceAsync(orderInvoiceId,
                    invoiceTotalCost, userMapping.NexportUserId, queueItem.Id, DateTime.UtcNow);

                // Commit the invoice
                var commitResult = await nexportService.CommitNexportOrderInvoiceTransactionAsync(orderInvoiceId);

                if (commitResult != null)
                {
                    try
                    {
                        foreach (var code in commitResult.InvoiceItemRedemptionCodes)
                        {
                            var invoiceItem = await nexportService.FindNexportOrderInvoiceItemByGuidAsync(Guid.Parse(code.Key));
                            if (invoiceItem != null)
                            {
                                invoiceItem.InvoiceRedemptionCode = commitResult.InvoiceRedemptionCode;
                                invoiceItem.InvoiceItemRedemptionCode = code.Value;
                                await nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);
                            }
                            else
                            {
                                await LogAndAddOrderNoteForErrorAsync(order, $"Invoice item not found after commit for code: {code.Key}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await LogAndAddOrderNoteForErrorAsync(order, $"Failed while updating order invoice items after commit", ex);
                    }
                }

                if (autoRedeemingInvoiceItems.Count > 0)
                {
                    try
                    {
                        // Schedule the redemption task for invoice items that do not need approval from administrators
                        foreach (var redeemingInvoiceItem in autoRedeemingInvoiceItems)
                        {
                            await nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(
                                new NexportOrderInvoiceRedemptionQueueItem
                                {
                                    OrderInvoiceItemId = redeemingInvoiceItem.Id,
                                    RedeemingUserId = userMapping.NexportUserId,
                                    ProductMappingId = redeemingInvoiceItem.ProductMappingId,
                                    OrderItemId = redeemingInvoiceItem.OrderItemId,
                                    UtcDateCreated = DateTime.UtcNow,
                                    ManualApprovalAction = redeemingInvoiceItem.ExtensionAction
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        await LogAndAddOrderNoteForErrorAsync(order,
                            $"Cannot schedule redemption processing for order items within order #{order.Id}", ex);
                    }
                }

                // Only complete the order if it does not require approval from administrators
                if (!requireManualApproval)
                {
                    completeOrder = true;
                    await nexportService.AddOrderNoteAsync(order, $"Nexport invoice has been successfully processed order #{order.Id} (Store: {store.Name})");
                }
                else
                {
                    // Send email notification to store owners to take approval action for this order
                    await nexportService.SendNewNexportOrderApprovalStoreOwnerNotificationAsync(order,
                        localizationSettings.DefaultAdminLanguageId);
                }

                await logger.InformationAsync($"Order {queueItem.OrderId} has been successfully processed!");
            }
            else if (invoiceDetails.State == GetInvoiceResponse.StateEnum.Committed)
            {
                // Complete the order when the invoice has been committed
                completeOrder = true;
                await logger.InformationAsync($"Order number {queueItem.OrderId} invoice details state is committed. Complete the order.");
            }
        }
        else
        {
            await LogAndAddOrderNoteForErrorAsync(order,
                $"User mapping for the customer {order.CustomerId} could not be found during the processing of Nexport invoice");
        }
        return completeOrder;
    }

    public async Task<bool> ProcessNexportWholesaleOrderAsync(NexportOrderProcessingQueueItem queueItem, Order order, Store store)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem), "Queue item cannot be null!");

        if (order == null)
            throw new ArgumentNullException(nameof(order), "Order entity cannot be null!");

        if (store == null)
            throw new ArgumentNullException(nameof(store), "Store entity cannot be null!");

        var completeOrder = false;
        var requireManualApproval = false;

        NexportFundingPool fundingPool = null;

        await logger.InformationAsync($"Begin processing order {order.Id}");

        var userMapping = await nexportService.FindUserMappingByCustomerId(order.CustomerId);

        if (userMapping != null)
        {
            await SynchronizeCustomerContactInformationAsync(userMapping);

            await nexportService.AddOrderNoteAsync(order, $"Nexport invoice has started processing for order: #{order.Id} (Store: {store.Name})");

            var orgId = await genericAttributeService.GetAttributeAsync<Guid>(store, "NexportSubscriptionOrganizationId", store.Id);

            if (orgId == Guid.Empty)
            {
                // ReSharper disable once PossibleInvalidOperationException
                orgId = nexportSettings.RootOrganizationId.Value;
            }

            var wholesaleOrderInfo = await nexportService.GetWholesaleOrderInfoForOrderAsync(order.Id);
            if (wholesaleOrderInfo?.FundingPoolId != null)
                fundingPool = await nexportWholesaleService.GetFundingPoolById(wholesaleOrderInfo.FundingPoolId.Value);

            // Check if there is an existing invoice. If not, begin a new invoice transaction.
            var orderInvoiceId = await nexportService.FindExistingInvoiceForOrder(order.Id);

            if (orderInvoiceId == null)
            {
                WholesalePurchasingGroup wholesalePurchasingGroup = null;

                if (wholesaleOrderInfo?.NexportGroupId != null)
                    wholesalePurchasingGroup = await nexportService.GetWholesalePurchaseGroupAsync(wholesaleOrderInfo.NexportGroupId.Value);

                if (wholesalePurchasingGroup != null)
                {
                    // pass group into begin order transaction
                    orderInvoiceId = await nexportService.BeginNexportOrderInvoiceTransactionAsync(
                        orgId, userMapping.NexportUserId, wholesalePurchasingGroup.NexportGroupId);
                }
                else
                {
                    orderInvoiceId = await nexportService.BeginNexportOrderInvoiceTransactionAsync(orgId, userMapping.NexportUserId);
                }
            }

            // Get the invoice details from Nexport (if existing)
            var invoiceDetails = await nexportService.GetNexportInvoiceAsync(orderInvoiceId.Value)!;

            // Continue to process only if the invoice is opening
            if (invoiceDetails == null ||
                (invoiceDetails.State != GetInvoiceResponse.StateEnum.Committed &&
                 invoiceDetails.State != GetInvoiceResponse.StateEnum.Failed))
            {
                var invoiceTotalCost = 0m;

                var orderItems = await orderService.GetOrderItemsAsync(order.Id);
                foreach (var orderItem in orderItems)
                {
                    var mappingInfo = await genericAttributeService.GetAttributeAsync<string>(orderItem,
                        $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                    // Retrieve the stored mapping info if existed; otherwise, get the current mapping info
                    var mapping = mappingInfo != null
                        ? JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo)
                        : await nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
                          await nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                    if (mapping != null)
                    {
                        var product = await productService.GetProductByIdAsync(orderItem.ProductId);
                        if (product != null)
                        {
                            var productCost = product.ProductCost;
                            var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;

                            // Generate the listing of group membership identifiers
                            var groupMembershipIds = await GenerateGroupMembershipIds(order, orderItem, mapping);

                            // Find existing invoice item for the order item
                            var existingInvoiceItemId = await nexportService.FindExistingInvoiceItemForOrderItem(order.Id, orderItem.Id);
                            // If the invoice item does not exist, then add the order item into the invoice
                            if (existingInvoiceItemId == null ||
                                !invoiceDetails.InvoiceItems.Any(i => i.Id == existingInvoiceItemId))
                            {
                                var (invoiceItemIds, _, requiringManualApproval, extensionAction) =
                                    await AddItemToNexportInvoiceWithQuantityAsync(
                                        mapping, userMapping,
                                        orderInvoiceId.Value, productCost, subscriptionOrgId, groupMembershipIds,
                                        quantity: orderItem.Quantity, fundingPool: fundingPool?.Code);

                                if (invoiceItemIds != null)
                                {
                                    // TODO: Rework ManualApproval & ExtensionAction with wholesale logic
                                    requireManualApproval = requiringManualApproval ?? requireManualApproval;

                                    foreach (var invoiceItemId in invoiceItemIds)
                                    {
                                        var nexportOrderInvoiceItem = new NexportOrderInvoiceItem
                                        {
                                            OrderId = queueItem.OrderId,
                                            OrderItemId = orderItem.Id,
                                            InvoiceItemId = invoiceItemId,
                                            InvoiceId = orderInvoiceId.Value,
                                            UtcDateProcessed = DateTime.UtcNow,
                                            RequireManualApproval = requireManualApproval,
                                            RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Available
                                        };

                                        var success = await nexportService.InsertOrUpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);

                                        invoiceTotalCost += productCost;
                                    }
                                }
                            }
                        }
                    }
                }

                // Add payment
                await nexportService.AddPaymentToNexportOrderInvoiceAsync(orderInvoiceId.Value,
                    invoiceTotalCost, userMapping.NexportUserId, queueItem.Id, DateTime.UtcNow);

                // Commit the invoice
                var commitResult = await nexportService.CommitNexportOrderInvoiceTransactionAsync(orderInvoiceId.Value);

                if (commitResult != null)
                {
                    foreach (var code in commitResult.InvoiceItemRedemptionCodes)
                    {
                        var invoiceItem = await nexportService.FindNexportOrderInvoiceItemByGuidAsync(Guid.Parse(code.Key));
                        if (invoiceItem != null)
                        {
                            invoiceItem.InvoiceRedemptionCode = commitResult.InvoiceRedemptionCode;
                            invoiceItem.InvoiceItemRedemptionCode = code.Value;

                            await nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);
                        }
                        else
                        {
                            await LogAndAddOrderNoteForErrorAsync(order, $"Invoice item not found after commit for code: {code.Key}");
                        }
                    }
                }

                // Only complete the order if it does not require approval from administrators
                if (!requireManualApproval)
                {
                    completeOrder = true;
                    await nexportService.AddOrderNoteAsync(order, $"Nexport invoice has started processing for order #{order.Id} (Store: {store.Name})");
                }
                else
                {
                    // Send email notification to store owners to take approval action for this order
                    await nexportService.SendNewNexportOrderApprovalStoreOwnerNotificationAsync(order, localizationSettings.DefaultAdminLanguageId);
                }

                await logger.InformationAsync($"Order #{queueItem.OrderId} has been successfully processed!");
            }
            else if (invoiceDetails.State == GetInvoiceResponse.StateEnum.Committed)
            {
                // Complete the order when the invoice has been committed
                completeOrder = true;
                await logger.InformationAsync($"Order number {queueItem.OrderId} invoice details state is committed. Complete the order.");
            }
        }
        else
        {
            await LogAndAddOrderNoteForErrorAsync(order, $"User mapping for the customer {order.CustomerId} could not be found during the processing of Nexport invoice");
        }

        return completeOrder;
    }

    private async Task SynchronizeCustomerContactInformationAsync(NexportUserMapping userMapping)
    {
        if (userMapping == null)
            throw new ArgumentNullException(nameof(userMapping));

        try
        {
            var customer = await customerService.GetCustomerByIdAsync(userMapping.NopUserId);
            if (customer?.BillingAddressId != null)
            {
                var currentBillingAddress = await addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                if (currentBillingAddress != null)
                {
                    var customerStateProvince =
                        await stateProvinceService.GetStateProvinceByIdAsync(currentBillingAddress
                            .StateProvinceId.GetValueOrDefault(0));

                    var customerAddressState = customerStateProvince != null ? customerStateProvince.Name : "";

                    var customerCountry =
                        await countryService.GetCountryByIdAsync(currentBillingAddress.CountryId.GetValueOrDefault(0));

                    var customerAddressCountry = customerCountry != null ? customerCountry.Name : "";

                    var updatedInfo = new UserContactInfoRequest(apiErrorEntity: new ApiErrorEntity())
                    {
                        AddressLine1 = currentBillingAddress.Address1,
                        AddressLine2 = currentBillingAddress.Address2,
                        City = currentBillingAddress.City,
                        State = customerAddressState,
                        Country = customerAddressCountry,
                        PostalCode = currentBillingAddress.ZipPostalCode,
                        Phone = currentBillingAddress.PhoneNumber,
                        Fax = currentBillingAddress.FaxNumber
                    };

                    await nexportService.UpdateNexportUserContactInfoAsync(userMapping.NexportUserId, updatedInfo)!;

                    await logger.InformationAsync($"Successfully update contact information in Nexport for customer {userMapping.NopUserId}");
                }
            }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"Cannot update contact information for customer {userMapping.NopUserId} in Nexport", ex);
        }
    }

    /// <summary>
    /// Generate the list of group membership identifiers
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="orderItem">The order item</param>
    /// <param name="productMapping">The Nexport product mapping</param>
    /// <returns>The list of group membership identifiers from the Nexport product mapping of the particular product.</returns>
    private async Task<IList<Guid>> GenerateGroupMembershipIds(Order order, OrderItem orderItem, NexportProductMapping productMapping)
    {
        IList<Guid> groupMembershipIds = new List<Guid>();

        var groupMembershipMappingInfo = (await genericAttributeService
            .GetAttributesForEntityAsync(orderItem.Id,
                $"ProductGroupMembershipMapping-{order.Id}-{orderItem.Id}-{productMapping.Id}"))
            .Where(a => a.StoreId == order.StoreId).ToList();

        if (groupMembershipMappingInfo.Count != 0)
        {
            foreach (var attribute in groupMembershipMappingInfo)
            {
                var groupMembershipMapping = JsonConvert.DeserializeObject<NexportProductGroupMembershipMapping>(attribute.Value);
                if (groupMembershipMapping != null)
                {
                    groupMembershipIds.Add(groupMembershipMapping.NexportGroupId);
                }
            }
        }
        else
        {
            groupMembershipIds = await nexportService.GetProductGroupMembershipIds(productMapping.Id);
        }

        return groupMembershipIds;
    }

    /// <summary>
    /// Add the product to the invoice
    /// </summary>
    /// <param name="productMapping">The Nexport product mapping entity</param>
    /// <param name="userMapping">The Nexport user mapping entity</param>
    /// <param name="orderInvoiceId">The Nexport invoice Id</param>
    /// <param name="productCost">The actual cost of the product</param>
    /// <param name="subscriptionOrgId">The Nexport subscription organization Id</param>
    /// <param name="groupMembershipIds">The list of group membership Ids</param>
    /// <param name="purchasingGroupId">The purchasing group Id</param>
    /// <param name="fundingPool">The funding pool</param>
    /// <param name="redemptionAvailableDate">The available redemption date</param>
    /// <returns>The Nexport invoice item Id</returns>
    private async Task<(Guid? InvoiceItemId, int? CompletionPercentage, bool? RequireManualApproval, int? ExtensionAction)>
        AddItemToNexportInvoiceAsync(NexportProductMapping productMapping, NexportUserMapping userMapping,
            Guid orderInvoiceId, decimal productCost, Guid subscriptionOrgId, IList<Guid> groupMembershipIds,
            Guid? purchasingGroupId = null, string fundingPool = null, DateTime? redemptionAvailableDate = null)
    {
        Guid? invoiceItemId = null;

        int? completionPercentage = null;
        int? extensionAction = null;
        bool? requireManualApproval = null;

        // Skip checking enrollment status if the product type is open-ended
        if (productMapping.Type == NexportProductTypeEnum.OpenEnded && productMapping.AssignWhenRedeemed.HasValue && productMapping.AssignWhenRedeemed.Value)
        {
            invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                orderInvoiceId,
                productMapping.NexportCatalogId, Enums.ProductTypeEnum.OpenEnded, productCost,
                subscriptionOrgId, groupMembershipIds,
                productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                purchasingGroupId, fundingPool, redemptionAvailableDate)!;
        }
        else
        {
            // TODO: Move to another method and needs to be called when we do selection of open-ended as well as here
            if (productMapping.NexportCatalogSyllabusLinkId != null)
            {
                // Verify the enrollment for the current product mapping if existed
                var existingEnrollmentStatus = await nexportService.VerifyNexportEnrollmentStatusAsync(productMapping, userMapping);

                switch (existingEnrollmentStatus)
                {
                    // Applicable for enrollment that is needed to be renewed or restarted
                    case { Phase: Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress }:
                        {
                            completionPercentage = existingEnrollmentStatus.CompletionPercentage;

                            if (productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.ForceManual)
                            {
                                invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                    productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.RenewalDuration,
                                    purchasingGroupId, fundingPool, redemptionAvailableDate)!;

                                requireManualApproval = true;
                            }
                            else
                            {
                                if (productMapping.RenewalCompletionThreshold.HasValue)
                                {
                                    if (completionPercentage < productMapping.RenewalCompletionThreshold)
                                    {
                                        // Use either access expiration date or access time limit when restarting enrollments that below the completion threshold
                                        invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                                            orderInvoiceId,
                                            productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                            productCost,
                                            subscriptionOrgId, groupMembershipIds,
                                            productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                                            purchasingGroupId, fundingPool, redemptionAvailableDate)!;

                                        extensionAction = 2;
                                    }
                                    else
                                    {
                                        invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                                            orderInvoiceId,
                                            productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                            productCost,
                                            subscriptionOrgId, groupMembershipIds,
                                            productMapping.UtcAccessExpirationDate, productMapping.RenewalDuration,
                                            purchasingGroupId, fundingPool, redemptionAvailableDate)!;

                                        requireManualApproval = productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.Manual;
                                    }
                                }
                                else
                                {
                                    var newAccessTimeLimit = !string.IsNullOrEmpty(productMapping.AccessTimeLimit)
                                        ? productMapping.AccessTimeLimit
                                        : productMapping.RenewalDuration;

                                    invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                                        orderInvoiceId,
                                        productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                        productCost,
                                        subscriptionOrgId, groupMembershipIds,
                                        productMapping.UtcAccessExpirationDate, newAccessTimeLimit,
                                        purchasingGroupId, fundingPool, redemptionAvailableDate)!;
                                }
                            }

                            break;
                        }

                    // Applicable for new enrollment or enrollment that has been completed (passed or failed)
                    case null or { Phase: Enums.PhaseEnum.Finished, Result: Enums.ResultEnum.Failing or Enums.ResultEnum.Passing }:
                        {
                            if (productMapping.Type == NexportProductTypeEnum.Catalog)
                            {
                                invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogId, Enums.ProductTypeEnum.Catalog, productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                                    purchasingGroupId, fundingPool, redemptionAvailableDate)!;
                            }
                            else
                            {
                                invoiceItemId = await nexportService.AddItemToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                    productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                                    purchasingGroupId, fundingPool, redemptionAvailableDate)!;
                            }

                            break;
                        }
                }
            }
        }

        return (invoiceItemId, completionPercentage, requireManualApproval, extensionAction);
    }

    /// <summary>
    /// Add the product with quantity to the invoice
    /// </summary>
    /// <param name="productMapping">The Nexport product mapping entity</param>
    /// <param name="userMapping">The Nexport user mapping entity</param>
    /// <param name="orderInvoiceId">The Nexport invoice Id</param>
    /// <param name="productCost">The actual cost of the product</param>
    /// <param name="subscriptionOrgId">The Nexport subscription organization Id</param>
    /// <param name="groupMembershipIds">The list of group membership Ids</param>
    /// <param name="quantity">The quantity of the product</param>
    /// <param name="purchasingGroupId">The purchasing group Id</param>
    /// <param name="fundingPool">The funding pool</param>
    /// <param name="redemptionAvailableDate">The available redemption date</param>
    /// <returns>The list of Nexport invoice item Ids</returns>
    private async Task<(List<Guid> InvoiceItemIds, int? CompletionPercentage, bool? RequireManualApproval, int? ExtensionAction)>
        AddItemToNexportInvoiceWithQuantityAsync(NexportProductMapping productMapping, NexportUserMapping userMapping,
            Guid orderInvoiceId, decimal productCost, Guid subscriptionOrgId, IList<Guid> groupMembershipIds,
            int quantity = 1,
            Guid? purchasingGroupId = null, string fundingPool = null, DateTime? redemptionAvailableDate = null)
    {
        List<Guid> invoiceItemIds = null;

        int? completionPercentage = null;
        int? extensionAction = null;
        bool? requireManualApproval = null;

        // Skip checking enrollment status if the product type is open-ended
        if (productMapping.Type == NexportProductTypeEnum.OpenEnded && productMapping.AssignWhenRedeemed.HasValue && productMapping.AssignWhenRedeemed.Value)
        {
            invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                orderInvoiceId,
                productMapping.NexportCatalogId, Enums.ProductTypeEnum.OpenEnded, productCost,
                subscriptionOrgId, groupMembershipIds,
                productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;
        }
        else
        {
            // TODO: Move to another method and needs to be called when we do selection of open-ended as well as here
            if (productMapping.NexportCatalogSyllabusLinkId != null)
            {
                // Verify the enrollment for the current product mapping if existed
                var existingEnrollmentStatus = await nexportService.VerifyNexportEnrollmentStatusAsync(productMapping, userMapping);

                switch (existingEnrollmentStatus)
                {
                    // Applicable for enrollment that is needed to be renewed or restarted
                    case { Phase: Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress }:
                        {
                            completionPercentage = existingEnrollmentStatus.CompletionPercentage;

                            if (productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.ForceManual)
                            {
                                invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                    productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.RenewalDuration,
                                    purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;

                                requireManualApproval = true;
                            }
                            else
                            {
                                if (productMapping.RenewalCompletionThreshold.HasValue)
                                {
                                    if (completionPercentage < productMapping.RenewalCompletionThreshold)
                                    {
                                        // Use either access expiration date or access time limit when restarting enrollments that below the completion threshold
                                        invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                                            orderInvoiceId,
                                            productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                            productCost,
                                            subscriptionOrgId, groupMembershipIds,
                                            productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                                            purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;

                                        extensionAction = 2;
                                    }
                                    else
                                    {
                                        invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                                            orderInvoiceId,
                                            productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                            productCost,
                                            subscriptionOrgId, groupMembershipIds,
                                            productMapping.UtcAccessExpirationDate, productMapping.RenewalDuration,
                                            purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;

                                        requireManualApproval = productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.Manual;
                                    }
                                }
                                else
                                {
                                    var newAccessTimeLimit = !string.IsNullOrEmpty(productMapping.AccessTimeLimit)
                                        ? productMapping.AccessTimeLimit
                                        : productMapping.RenewalDuration;

                                    invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                                        orderInvoiceId,
                                        productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                        productCost,
                                        subscriptionOrgId, groupMembershipIds,
                                        productMapping.UtcAccessExpirationDate, newAccessTimeLimit,
                                        purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;
                                }
                            }

                            break;
                        }

                    // Applicable for new enrollment or enrollment that has been completed (passed or failed)
                    case null or { Phase: Enums.PhaseEnum.Finished, Result: Enums.ResultEnum.Failing or Enums.ResultEnum.Passing }:
                        {
                            if (productMapping.Type == NexportProductTypeEnum.Catalog)
                            {
                                invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogId, Enums.ProductTypeEnum.Catalog, productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                                    purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;
                            }
                            else
                            {
                                invoiceItemIds = await nexportService.AddItemsToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                    productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit,
                                    purchasingGroupId, fundingPool, redemptionAvailableDate, quantity: quantity)!;
                            }

                            break;
                        }
                }
            }
        }

        return (invoiceItemIds, completionPercentage, requireManualApproval, extensionAction);
    }

    private async Task LogAndAddOrderNoteForErrorAsync(Order order, string errMsg, Exception ex = null)
    {
        ex ??= new Exception(errMsg);
        await logger.ErrorAsync(errMsg, ex);

        await nexportService.AddOrderNoteAsync(order, errMsg);
    }
}