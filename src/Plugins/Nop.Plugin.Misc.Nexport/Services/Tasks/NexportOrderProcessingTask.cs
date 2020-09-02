using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportOrderProcessingTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IRepository<NexportOrderProcessingQueueItem> _nexportOrderProcessingQueueRepository;
        private readonly NexportService _nexportService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly ISettingService _settingService;
        private readonly NexportSettings _nexportSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize;

        private struct AutoRedeemingInvoiceItem
        {
            public int Id;

            public int ProductMappingId;

            public int OrderItemId;
        }

        public NexportOrderProcessingTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
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
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
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

                                        //var autoRedeemingInvoiceItemIds = new List<int, int>();
                                        var autoRedeemingInvoiceItemIds = new List<AutoRedeemingInvoiceItem>();

                                        foreach (var orderItem in order.OrderItems)
                                        {
                                            var mappingInfo = _genericAttributeService.GetAttribute<string>(orderItem,
                                                $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                                            var mapping = mappingInfo != null ?
                                                JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo) :
                                                _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
                                                _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                                            if (mapping != null)
                                            {
                                                var productCost = orderItem.Product.ProductCost;
                                                var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;

                                                IList<Guid> groupMembershipIds = new List<Guid>();

                                                var groupMembershipMappingInfo = _genericAttributeService
                                                    .GetAttributesForEntity(orderItem.Id,
                                                        $"ProductGroupMembershipMapping-{order.Id}-{orderItem.Id}-{mapping.Id}")
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
                                                    groupMembershipIds = _nexportService.GetProductGroupMembershipIds(mapping.Id);
                                                }

                                                // Find existing invoice item for the order item
                                                var existingInvoiceItemId =
                                                    _nexportService.FindExistingInvoiceItemForOrderItem(order.Id,
                                                        orderItem.Id);

                                                // If the invoice item does not existed, then add the order item into the invoice
                                                if (existingInvoiceItemId == null ||
                                                    !invoiceDetails.InvoiceItems.Any(i => i.Id == existingInvoiceItemId))
                                                {
                                                    var accessTimeLimit = mapping.AccessTimeLimit;

                                                    Guid? invoiceItemId;
                                                    if (mapping.Type == NexportProductTypeEnum.Catalog)
                                                    {
                                                        invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                                            orderInvoiceId,
                                                            mapping.NexportCatalogId,
                                                            Enums.ProductTypeEnum.Catalog, productCost,
                                                            subscriptionOrgId, groupMembershipIds,
                                                            mapping.UtcAccessExpirationDate,
                                                            accessTimeLimit);
                                                    }
                                                    else
                                                    {
                                                        invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                                            orderInvoiceId,
                                                            mapping.NexportCatalogSyllabusLinkId.Value,
                                                            Enums.ProductTypeEnum.Syllabus, productCost,
                                                            subscriptionOrgId, groupMembershipIds,
                                                            mapping.UtcAccessExpirationDate,
                                                            accessTimeLimit);
                                                    }

                                                    if (invoiceItemId.HasValue)
                                                    {
                                                        var nexportOrderInvoiceItem = new NexportOrderInvoiceItem
                                                        {
                                                            OrderId = queueItem.OrderId,
                                                            OrderItemId = orderItem.Id,
                                                            InvoiceItemId = invoiceItemId.Value,
                                                            InvoiceId = orderInvoiceId,
                                                            UtcDateProcessed = DateTime.UtcNow
                                                        };

                                                        _nexportService.InsertOrUpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);

                                                        // Add the invoice item for auto redeeming after committing the invoice if AutoRedeem is set on the mapping
                                                        if (mapping.AutoRedeem)
                                                        {
                                                            autoRedeemingInvoiceItemIds.Add(new AutoRedeemingInvoiceItem
                                                            {
                                                                Id = nexportOrderInvoiceItem.Id,
                                                                ProductMappingId = mapping.Id,
                                                                OrderItemId = orderItem.Id
                                                            });
                                                        }

                                                        invoiceTotalCost += productCost;
                                                    }
                                                }
                                            }
                                        }

                                        // Add payment
                                        _nexportService.AddPaymentToNexportOrderInvoice(orderInvoiceId,
                                            invoiceTotalCost, userMapping.NexportUserId, queueItemId, DateTime.UtcNow);

                                        // Commit the invoice
                                        _nexportService.CommitNexportOrderInvoiceTransaction(orderInvoiceId);

                                        if (autoRedeemingInvoiceItemIds.Count > 0)
                                        {
                                            try
                                            {
                                                foreach (var redeemingInvoiceItem in autoRedeemingInvoiceItemIds)
                                                {
                                                    _nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(new NexportOrderInvoiceRedemptionQueueItem
                                                    {
                                                        OrderInvoiceItemId = redeemingInvoiceItem.Id,
                                                        RedeemingUserId = userMapping.NexportUserId,
                                                        ProductMappingId = redeemingInvoiceItem.ProductMappingId,
                                                        OrderItemId = redeemingInvoiceItem.OrderItemId,
                                                        UtcDateCreated = DateTime.UtcNow
                                                    });
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                LogAndAddOrderNoteForError(order,
                                                    $"Cannot schedule redemption processing for order items within order {order.Id}", ex);
                                            }
                                        }
                                        else
                                        {
                                            completeOrder = true;
                                            _nexportService.AddOrderNote(order,
                                                "Nexport invoice has been successfully processed");
                                        }

                                        _logger.Information(
                                            $"Order {queueItem.OrderId} has been successfully processed!");
                                    }
                                    else if (invoiceDetails.State == GetInvoiceResponse.StateEnum.Committed)
                                    {
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
                var currentBillingAddress = customer?.BillingAddress;
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
            catch (Exception ex)
            {
                _logger.Error($"Cannot update contact information for customer {userMapping.NopUserId} in Nexport", ex);
            }
        }

        private void LogAndAddOrderNoteForError(Order order, string errMsg, Exception ex = null)
        {
            ex = ex ?? new Exception(errMsg);
            _logger.Error(errMsg, ex);

            _nexportService.AddOrderNote(order, errMsg);
        }
    }
}
