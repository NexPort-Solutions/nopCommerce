using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleTasks
{
    public class NexportOrderProcessingTask : IScheduleTask
    {
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly NexportSettings _nexportSettings;

        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IRepository<NexportOrderProcessingQueueItem> _nexportOrderProcessingQueueRepository;
        private readonly NexportService _nexportService;
        private readonly IAddressService _addressService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly ISettingService _settingService;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize;

        private struct AutoRedeemingInvoiceItem
        {
            public int Id;

            public int ProductMappingId;

            public int OrderItemId;

            public int? ExtensionAction;
        }

        public NexportOrderProcessingTask(
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
            NexportSettings nexportSettings)
        {
            _emailAccountSettings = emailAccountSettings;
            _localizationSettings = localizationSettings;

            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _addressService = addressService;
            _productService = productService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _storeService = storeService;
            _customerService = customerService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _settingService = settingService;
            _genericAttributeService = genericAttributeService;
            _nexportOrderProcessingQueueRepository = nexportOrderProcessingQueueRepository;
            _nexportService = nexportService;
            _nexportSettings = nexportSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                _batchSize = await _settingService.GetSettingByKeyAsync(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey,
                    NexportDefaults.NexportOrderProcessingTaskBatchSize);

                var orders = (from q in _nexportOrderProcessingQueueRepository.Table
                              orderby q.UtcDateCreated
                              select q.Id).Take(_batchSize).ToList();

                await ProcessNexportOrdersAsync(orders);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process Nexport redemption", ex);
            }
        }

        public async Task ProcessNexportOrdersAsync(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    var completeOrder = false;
                    var requireManualApproval = false;

                    try
                    {
                        await _logger.InformationAsync($"Begin processing order processing queue item {queueItemId}");

                        var queueItem = await _nexportOrderProcessingQueueRepository.GetByIdAsync(queueItemId);

                        if (queueItem == null)
                            continue;

                        var order = await _orderService.GetOrderByIdAsync(queueItem.OrderId);

                        // Only process order that has Processing status
                        if (order is { Deleted: false, OrderStatus: OrderStatus.Processing })
                        {
                            await _logger.InformationAsync($"Begin processing order {order.Id}");

                            var userMapping = await _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                            if (userMapping != null)
                            {
                                await SynchronizeCustomerContactInformationAsync(userMapping);

                                var store = await _storeService.GetStoreByIdAsync(order.StoreId);

                                if (store != null)
                                {
                                    var orgId = await _genericAttributeService.GetAttributeAsync<Guid>(store, "NexportSubscriptionOrganizationId", store.Id);

                                    if (orgId == Guid.Empty)
                                    {
                                        orgId = _nexportSettings.RootOrganizationId.Value;
                                    }

                                    // Check if there is an existing invoice. If not, begin a new invoice transaction.
                                    var orderInvoiceId =
                                        await _nexportService.FindExistingInvoiceForOrder(order.Id) ??
                                        await _nexportService.BeginNexportOrderInvoiceTransactionAsync(orgId, userMapping.NexportUserId);

                                    // Get the invoice details from Nexport (if existing)
                                    var invoiceDetails = await _nexportService.GetNexportInvoiceAsync(orderInvoiceId);

                                    // Continue to process only if the invoice is opening
                                    if (invoiceDetails == null ||
                                        (invoiceDetails.State != GetInvoiceResponse.StateEnum.Committed &&
                                         invoiceDetails.State != GetInvoiceResponse.StateEnum.Failed))
                                    {
                                        decimal invoiceTotalCost = 0;

                                        var autoRedeemingInvoiceItems = new List<AutoRedeemingInvoiceItem>();

                                        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                                        foreach (var orderItem in orderItems)
                                        {
                                            var mappingInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
                                                $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                                            // Retrieve the stored mapping info if existed; otherwise, get the current mapping info
                                            var mapping = mappingInfo != null ?
                                                JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo) :
                                                await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
                                                await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                                            if (mapping != null)
                                            {
                                                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                                                if (product != null)
                                                {
                                                    var productCost = product.ProductCost;
                                                    var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;

                                                    // Generate the listing of group membership identifiers
                                                    var groupMembershipIds = await GenerateGroupMembershipIds(order, orderItem, mapping);

                                                    // Find existing invoice item for the order item
                                                    var existingInvoiceItemId =
                                                        await _nexportService.FindExistingInvoiceItemForOrderItem(order.Id, orderItem.Id);

                                                    // If the invoice item does not existed, then add the order item into the invoice
                                                    if (existingInvoiceItemId == null ||
                                                        !invoiceDetails.InvoiceItems.Any(i => i.Id == existingInvoiceItemId))
                                                    {
                                                        var addItemResult = await AddItemToNexportInvoiceAsync(mapping, userMapping,
                                                            orderInvoiceId, productCost, subscriptionOrgId,
                                                            groupMembershipIds);

                                                        var invoiceItemId = addItemResult.InvoiceItemId;

                                                        if (invoiceItemId.HasValue)
                                                        {
                                                            int? extensionAction;
                                                            var nexportOrderInvoiceItem = new NexportOrderInvoiceItem
                                                            {
                                                                OrderId = queueItem.OrderId,
                                                                OrderItemId = orderItem.Id,
                                                                InvoiceItemId = invoiceItemId.Value,
                                                                InvoiceId = orderInvoiceId,
                                                                UtcDateProcessed = DateTime.UtcNow,
                                                                RequireManualApproval = addItemResult.RequireManualApproval
                                                            };

                                                            // This allows the task to automatically process redemption as restarting the enrollment
                                                            // in which the customer currently does not meet the enrollment completion threshold
                                                            // no matter what the approval method is.
                                                            extensionAction = addItemResult.ExtensionAction;

                                                            await _nexportService.InsertOrUpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);

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
                                        await _nexportService.AddPaymentToNexportOrderInvoiceAsync(orderInvoiceId,
                                            invoiceTotalCost, userMapping.NexportUserId, queueItemId, DateTime.UtcNow);

                                        // Commit the invoice
                                        await _nexportService.CommitNexportOrderInvoiceTransactionAsync(orderInvoiceId);

                                        if (autoRedeemingInvoiceItems.Count > 0)
                                        {
                                            try
                                            {
                                                // Schedule the redemption task for invoice items that do not need approval from administrators
                                                foreach (var redeemingInvoiceItem in autoRedeemingInvoiceItems)
                                                {
                                                    await _nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(
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
                                                    $"Cannot schedule redemption processing for order items within order {order.Id}", ex);
                                            }
                                        }

                                        // Only complete the order if it does not require approval from administrators
                                        if (!requireManualApproval)
                                        {
                                            completeOrder = true;
                                            await _nexportService.AddOrderNoteAsync(order, "Nexport invoice has been successfully processed");
                                        }
                                        else
                                        {
                                            // Send email notification to store owners to take approval action for this order
                                            await _nexportService.SendNewNexportOrderApprovalStoreOwnerNotificationAsync(order,
                                                _localizationSettings.DefaultAdminLanguageId);
                                        }

                                        await _logger.InformationAsync($"Order {queueItem.OrderId} has been successfully processed!");
                                    }
                                    else if (invoiceDetails.State == GetInvoiceResponse.StateEnum.Committed)
                                    {
                                        // Complete the order when the invoice has been committed
                                        completeOrder = true;
                                    }
                                }
                                else
                                {
                                    await LogAndAddOrderNoteForErrorAsync(order,
                                        $"Store {order.StoreId} could not be found for this order during the processing of Nexport invoice");
                                }
                            }
                            else
                            {
                                await LogAndAddOrderNoteForErrorAsync(order,
                                    $"User mapping for the customer {order.CustomerId} could not be found during the processing of Nexport invoice");
                            }
                        }
                        else
                        {
                            await _logger.WarningAsync($"Cannot find the order {queueItem.OrderId}");
                        }

                        // Update the order with order notes. This does not complete the order yet.
                        await _orderService.UpdateOrderAsync(order);

                        // Delete queue item
                        await _nexportService.DeleteNexportOrderProcessingQueueItem(queueItem);

                        await _logger.InformationAsync($"Order processing queue item {queueItemId} has been processed and removed!");

                        if (completeOrder)
                        {
                            // Finish the order process and set the status to Complete
                            await _orderProcessingService.CheckOrderStatusAsync(order);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot process NexportRedemptionProcessingQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process the NexportRedemptionProcessingQueue", ex);
            }
        }

        private async Task SynchronizeCustomerContactInformationAsync(NexportUserMapping userMapping)
        {
            if (userMapping == null)
                throw new ArgumentNullException(nameof(userMapping));

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(userMapping.NopUserId);
                if (customer?.BillingAddressId != null)
                {
                    var currentBillingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                    if (currentBillingAddress != null)
                    {
                        var customerStateProvince =
                            await _stateProvinceService.GetStateProvinceByIdAsync(currentBillingAddress
                                .StateProvinceId.GetValueOrDefault(0));

                        var customerAddressState = customerStateProvince != null ? customerStateProvince.Name : "";

                        var customerCountry =
                            await _countryService.GetCountryByIdAsync(currentBillingAddress.CountryId.GetValueOrDefault(0));

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

                        await _nexportService.UpdateNexportUserContactInfoAsync(userMapping.NexportUserId, updatedInfo)!;

                        await _logger.InformationAsync($"Successfully update contact information in Nexport for customer {userMapping.NopUserId}");
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot update contact information for customer {userMapping.NopUserId} in Nexport", ex);
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

            var groupMembershipMappingInfo = (await _genericAttributeService
                .GetAttributesForEntityAsync(orderItem.Id,
                    $"ProductGroupMembershipMapping-{order.Id}-{orderItem.Id}-{productMapping.Id}"))
                .Where(a => a.StoreId == order.StoreId).ToList();

            if (groupMembershipMappingInfo.Any())
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
                groupMembershipIds = await _nexportService.GetProductGroupMembershipIds(productMapping.Id);
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
        /// <returns>The Nexport invoice item Id</returns>
        private async Task<(Guid? InvoiceItemId, int? CompletionPercentage, bool? RequireManualApproval, int? ExtensionAction)>
            AddItemToNexportInvoiceAsync(NexportProductMapping productMapping, NexportUserMapping userMapping,
            Guid orderInvoiceId, decimal productCost, Guid subscriptionOrgId, IList<Guid> groupMembershipIds)
        {
            Guid? invoiceItemId = null;

            int? completionPercentage = null;
            int? extensionAction = null;
            bool? requireManualApproval = null;

            if (productMapping.NexportCatalogSyllabusLinkId != null)
            {
                // Verify the enrollment for the current product mapping if existed
                var existingEnrollmentStatus =
                    await _nexportService.VerifyNexportEnrollmentStatusAsync(productMapping, userMapping);

                switch (existingEnrollmentStatus)
                {
                    // Applicable for enrollment that is needed to be renewed or restarted
                    case { Phase: Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress }:
                        {
                            completionPercentage = existingEnrollmentStatus.Value.CompletionPercentage;

                            if (productMapping.RenewalCompletionThreshold.HasValue)
                            {
                                if (completionPercentage < productMapping.RenewalCompletionThreshold)
                                {
                                    // Use either access expiration date or access time limit when restarting enrollments that below the completion threshold
                                    invoiceItemId = await _nexportService.AddItemToNexportOrderInvoiceAsync(
                                        orderInvoiceId,
                                        productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus, productCost,
                                        subscriptionOrgId, groupMembershipIds,
                                        productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit)!;

                                    extensionAction = 2;
                                }
                                else
                                {
                                    invoiceItemId = await _nexportService.AddItemToNexportOrderInvoiceAsync(
                                        orderInvoiceId,
                                        productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                        productCost,
                                        subscriptionOrgId, groupMembershipIds,
                                        productMapping.UtcAccessExpirationDate, productMapping.RenewalDuration)!;

                                    requireManualApproval = productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.Manual;
                                }
                            }
                            else
                            {
                                var newAccessTimeLimit = !string.IsNullOrEmpty(productMapping.AccessTimeLimit)
                                    ? productMapping.AccessTimeLimit
                                    : productMapping.RenewalDuration;

                                invoiceItemId = await _nexportService.AddItemToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                    productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, newAccessTimeLimit)!;
                            }

                            break;
                        }

                    // Applicable for new enrollment or enrollment that has been completed (passed or failed)
                    case var status
                        when status == null ||
                             status.Value.Phase == Enums.PhaseEnum.Finished &&
                             status.Value.Result is Enums.ResultEnum.Failing or Enums.ResultEnum.Passing:
                        {
                            if (productMapping.Type == NexportProductTypeEnum.Catalog)
                            {
                                invoiceItemId = await _nexportService.AddItemToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogId, Enums.ProductTypeEnum.Catalog, productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit)!;
                            }
                            else
                            {
                                invoiceItemId = await _nexportService.AddItemToNexportOrderInvoiceAsync(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus, productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit)!;
                            }

                            break;
                        }
                }
            }

            return (invoiceItemId, completionPercentage, requireManualApproval, extensionAction);
        }

        private async Task LogAndAddOrderNoteForErrorAsync(Order order, string errMsg, Exception ex = null)
        {
            ex ??= new Exception(errMsg);
            await _logger.ErrorAsync(errMsg, ex);

            await _nexportService.AddOrderNoteAsync(order, errMsg);
        }
    }
}
