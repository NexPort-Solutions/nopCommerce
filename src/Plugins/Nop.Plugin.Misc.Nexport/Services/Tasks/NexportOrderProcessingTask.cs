using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Services.Catalog;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
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

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                _batchSize = _settingService.GetSettingByKey(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey,
                    NexportDefaults.NexportOrderProcessingTaskBatchSize);

                var orders = (from q in _nexportOrderProcessingQueueRepository.Table
                              orderby q.UtcDateCreated
                              select q.Id).Take(_batchSize).ToList();

                ProcessNexportOrders(orders);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process Nexport redemption", ex);
            }
        }

        public void ProcessNexportOrders(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    var completeOrder = false;
                    var requireManualApproval = false;

                    try
                    {
                        _logger.Information($"Begin processing order processing queue item {queueItemId}");

                        var queueItem = _nexportOrderProcessingQueueRepository.GetById(queueItemId);

                        if (queueItem == null)
                            continue;

                        var order = _orderService.GetOrderById(queueItem.OrderId);

                        // Only process order that has Processing status
                        if (order != null && !order.Deleted && order.OrderStatus == OrderStatus.Processing)
                        {
                            _logger.Information($"Begin processing order {order.Id}");

                            var userMapping = _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                            if (userMapping != null)
                            {
                                SynchronizeCustomerContactInformation(userMapping);

                                var store = _storeService.GetStoreById(order.StoreId);

                                if (store != null)
                                {
                                    var orgId = _genericAttributeService.GetAttribute<Guid>(store, "NexportSubscriptionOrganizationId", store.Id);

                                    if (orgId == Guid.Empty)
                                    {
                                        orgId = _nexportSettings.RootOrganizationId.Value;
                                    }

                                    // Check if there is an existing invoice. If not, begin a new invoice transaction.
                                    var orderInvoiceId =
                                        _nexportService.FindExistingInvoiceForOrder(order.Id) ??
                                        _nexportService.BeginNexportOrderInvoiceTransaction(orgId, userMapping.NexportUserId);

                                    // Get the invoice details from Nexport (if existing)
                                    var invoiceDetails = _nexportService.GetNexportInvoice(orderInvoiceId);

                                    // Continue to process only if the invoice is opening
                                    if (invoiceDetails == null ||
                                        (invoiceDetails.State != GetInvoiceResponse.StateEnum.Committed &&
                                         invoiceDetails.State != GetInvoiceResponse.StateEnum.Failed))
                                    {
                                        decimal invoiceTotalCost = 0;

                                        var autoRedeemingInvoiceItems = new List<AutoRedeemingInvoiceItem>();

                                        var orderItems = _orderService.GetOrderItems(order.Id);
                                        foreach (var orderItem in orderItems)
                                        {
                                            var mappingInfo = _genericAttributeService.GetAttribute<string>(orderItem,
                                                $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                                            // Retrieve the stored mapping info if existed; otherwise, get the current mapping info
                                            var mapping = mappingInfo != null ?
                                                JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo) :
                                                _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
                                                _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                                            if (mapping != null)
                                            {
                                                var product = _productService.GetProductById(orderItem.ProductId);
                                                if (product != null)
                                                {
                                                    var productCost = product.ProductCost;
                                                    var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;

                                                    // Generate the listing of group membership identifiers
                                                    var groupMembershipIds = GenerateGroupMembershipIds(order, orderItem, mapping);

                                                    // Find existing invoice item for the order item
                                                    var existingInvoiceItemId =
                                                        _nexportService.FindExistingInvoiceItemForOrderItem(order.Id, orderItem.Id);

                                                    // If the invoice item does not existed, then add the order item into the invoice
                                                    if (existingInvoiceItemId == null ||
                                                        !invoiceDetails.InvoiceItems.Any(i => i.Id == existingInvoiceItemId))
                                                    {
                                                        var addItemResult = AddItemToNexportInvoice(mapping, userMapping,
                                                            orderInvoiceId, productCost, subscriptionOrgId,
                                                            groupMembershipIds);

                                                        var invoiceItemId = addItemResult.InvoiceItemId;

                                                        if (invoiceItemId.HasValue)
                                                        {
                                                            int? extensionAction = null;
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

                                                            _nexportService.InsertOrUpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);

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
                                        _nexportService.AddPaymentToNexportOrderInvoice(orderInvoiceId,
                                            invoiceTotalCost, userMapping.NexportUserId, queueItemId, DateTime.UtcNow);

                                        // Commit the invoice
                                        _nexportService.CommitNexportOrderInvoiceTransaction(orderInvoiceId);

                                        if (autoRedeemingInvoiceItems.Count > 0)
                                        {
                                            try
                                            {
                                                // Schedule the redemption task for invoice items that do not need approval from administrators
                                                foreach (var redeemingInvoiceItem in autoRedeemingInvoiceItems)
                                                {
                                                    _nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(
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
                                                LogAndAddOrderNoteForError(order,
                                                    $"Cannot schedule redemption processing for order items within order {order.Id}", ex);
                                            }
                                        }

                                        // Only complete the order if it does not require approval from administrators
                                        if (!requireManualApproval)
                                        {
                                            completeOrder = true;
                                            _nexportService.AddOrderNote(order, "Nexport invoice has been successfully processed");
                                        }
                                        else
                                        {
                                            // Send email notification to store owners to take approval action for this order
                                            _nexportService.SendNewNexportOrderApprovalStoreOwnerNotification(order,
                                                _localizationSettings.DefaultAdminLanguageId);
                                        }

                                        _logger.Information($"Order {queueItem.OrderId} has been successfully processed!");
                                    }
                                    else if (invoiceDetails.State == GetInvoiceResponse.StateEnum.Committed)
                                    {
                                        // Complete the order when the invoice has been committed
                                        completeOrder = true;
                                    }
                                }
                                else
                                {
                                    LogAndAddOrderNoteForError(order,
                                        $"Store {order.StoreId} could not be found for this order during the processing of Nexport invoice");
                                }
                            }
                            else
                            {
                                LogAndAddOrderNoteForError(order,
                                    $"User mapping for the customer {order.CustomerId} could not be found during the processing of Nexport invoice");
                            }
                        }
                        else
                        {
                            _logger.Warning($"Cannot find the order {queueItem.OrderId}");
                        }

                        // Update the order with order notes. This does not complete the order yet.
                        _orderService.UpdateOrder(order);

                        // Delete queue item
                        _nexportService.DeleteNexportOrderProcessingQueueItem(queueItem);

                        _logger.Information($"Order processing queue item {queueItemId} has been processed and removed!");

                        if (completeOrder)
                        {
                            // Finish the order process and set the status to Complete
                            _orderProcessingService.CheckOrderStatus(order);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process NexportRedemptionProcessingQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process the NexportRedemptionProcessingQueue", ex);
            }
        }

        private void SynchronizeCustomerContactInformation(NexportUserMapping userMapping)
        {
            if (userMapping == null)
                throw new ArgumentNullException(nameof(userMapping));

            try
            {
                var customer = _customerService.GetCustomerById(userMapping.NopUserId);
                if (customer?.BillingAddressId != null)
                {
                    var currentBillingAddress = _addressService.GetAddressById(customer.BillingAddressId.Value);
                    if (currentBillingAddress != null)
                    {
                        var customerStateProvince =
                            _stateProvinceService.GetStateProvinceById(currentBillingAddress
                                .StateProvinceId.GetValueOrDefault(0));

                        var customerAddressState = customerStateProvince != null ? customerStateProvince.Name : "";

                        var customerCountry =
                            _countryService.GetCountryById(currentBillingAddress.CountryId.GetValueOrDefault(0));

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

                        _nexportService.UpdateNexportUserContactInfo(userMapping.NexportUserId,
                            updatedInfo);

                        _logger.Information($"Successfully update contact information in Nexport for customer {userMapping.NopUserId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot update contact information for customer {userMapping.NopUserId} in Nexport", ex);
            }
        }

        /// <summary>
        /// Generate the list of group membership identifiers
        /// </summary>
        /// <param name="order">The order</param>
        /// <param name="orderItem">The order item</param>
        /// <param name="productMapping">The Nexport product mapping</param>
        /// <returns>The list of group membership identifiers from the Nexport product mapping of the particular product.</returns>
        private IList<Guid> GenerateGroupMembershipIds(Order order, OrderItem orderItem, NexportProductMapping productMapping)
        {
            IList<Guid> groupMembershipIds = new List<Guid>();

            var groupMembershipMappingInfo = _genericAttributeService
                .GetAttributesForEntity(orderItem.Id,
                    $"ProductGroupMembershipMapping-{order.Id}-{orderItem.Id}-{productMapping.Id}")
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
                groupMembershipIds = _nexportService.GetProductGroupMembershipIds(productMapping.Id);
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
        private (Guid? InvoiceItemId, int? CompletionPercentage, bool? RequireManualApproval, int? ExtensionAction)
            AddItemToNexportInvoice(NexportProductMapping productMapping, NexportUserMapping userMapping,
            Guid orderInvoiceId, decimal productCost, Guid subscriptionOrgId, IList<Guid> groupMembershipIds)
        {
            Guid? invoiceItemId = null;

            int? completionPercentage = null;
            int? extensionAction = null;
            bool? requireManualApproval = null;

            if (productMapping.NexportCatalogSyllabusLinkId != null)
            {
                // Verify the enrollment for the current product mapping if existed
                var existingEnrollmentStatus = _nexportService.VerifyNexportEnrollmentStatus(productMapping, userMapping);

                switch (existingEnrollmentStatus)
                {
                    // Applicable for enrollment that is needed to be renewed or restarted
                    case var status
                        when status != null &&
                             (status.Value.Phase == Enums.PhaseEnum.NotStarted || status.Value.Phase == Enums.PhaseEnum.InProgress):
                        {
                            completionPercentage = existingEnrollmentStatus.Value.CompletionPercentage;

                            if (productMapping.RenewalCompletionThreshold.HasValue)
                            {
                                if (completionPercentage < productMapping.RenewalCompletionThreshold)
                                {
                                    // Use either access expiration date or access time limit when restarting enrollments that below the completion threshold
                                    invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                        orderInvoiceId,
                                        productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus, productCost,
                                        subscriptionOrgId, groupMembershipIds,
                                        productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit);

                                    extensionAction = 2;
                                }
                                else
                                {
                                    invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                        orderInvoiceId,
                                        productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                        productCost,
                                        subscriptionOrgId, groupMembershipIds,
                                        productMapping.UtcAccessExpirationDate, productMapping.RenewalDuration);

                                    requireManualApproval = productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.Manual;
                                }
                            }
                            else
                            {
                                var newAccessTimeLimit = !string.IsNullOrEmpty(productMapping.AccessTimeLimit)
                                    ? productMapping.AccessTimeLimit
                                    : productMapping.RenewalDuration;

                                invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus,
                                    productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, newAccessTimeLimit);
                            }

                            break;
                        }

                    // Applicable for new enrollment or enrollment that has been completed (passed or failed)
                    case var status
                        when status == null ||
                             status.Value.Phase == Enums.PhaseEnum.Finished &&
                             (status.Value.Result == Enums.ResultEnum.Failing || status.Value.Result == Enums.ResultEnum.Passing):
                        {
                            if (productMapping.Type == NexportProductTypeEnum.Catalog)
                            {
                                invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogId, Enums.ProductTypeEnum.Catalog, productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit);
                            }
                            else
                            {
                                invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                    orderInvoiceId,
                                    productMapping.NexportCatalogSyllabusLinkId.Value, Enums.ProductTypeEnum.Syllabus, productCost,
                                    subscriptionOrgId, groupMembershipIds,
                                    productMapping.UtcAccessExpirationDate, productMapping.AccessTimeLimit);
                            }

                            break;
                        }
                }
            }

            return (invoiceItemId, completionPercentage, requireManualApproval, extensionAction);
        }

        private void LogAndAddOrderNoteForError(Order order, string errMsg, Exception ex = null)
        {
            ex = ex ?? new Exception(errMsg);
            _logger.Error(errMsg, ex);

            _nexportService.AddOrderNote(order, errMsg);
        }
    }
}
