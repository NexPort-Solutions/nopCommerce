using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Model;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tasks;

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
        private readonly ISettingService _settingService;
        private readonly NexportSettings _nexportSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize;

        public NexportOrderProcessingTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IStoreService storeService,
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
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
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
                    try
                    {
                        _logger.Information($"Begin processing order processing queue item {queueItemId}");

                        var queueItem = _nexportOrderProcessingQueueRepository.GetById(queueItemId);

                        if (queueItem == null)
                            continue;

                        var order = _orderService.GetOrderById(queueItem.OrderId);

                        // Only process order that has Processing status
                        if (order != null && order.OrderStatus == OrderStatus.Processing)
                        {
                            _logger.Information($"Begin processing order {order.Id}");

                            var userMapping = _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                            if (userMapping != null)
                            {
                                var store = _storeService.GetStoreById(order.StoreId);

                                if (store != null)
                                {
                                    var orgId = _genericAttributeService.GetAttribute<Guid>(store, "NexportSubscriptionOrganizationId", store.Id);

                                    if (orgId == Guid.Empty)
                                    {
                                        orgId = _nexportSettings.RootOrganizationId.Value;
                                    }

                                    // Check if there is an existing invoice. If not, begin a new invoice transaction.
                                    var orderInvoiceId = _nexportService.FindExistingInvoiceForOrder(order.Id) ??
                                                         Guid.Parse(_nexportService.BeginNexportOrderInvoiceTransaction(orgId,
                                                             userMapping.NexportUserId));

                                    // Get the invoice details from Nexport (if existing)
                                    var invoiceDetails = _nexportService.GetNexportInvoice(orderInvoiceId);

                                    // Continue to process only if the invoice is opening
                                    if (invoiceDetails == null ||
                                        (invoiceDetails.State != GetInvoiceResponse.StateEnum.Committed &&
                                         invoiceDetails.State != GetInvoiceResponse.StateEnum.Failed))
                                    {
                                        decimal invoiceTotalCost = 0;

                                        var autoRedeemingInvoiceItemIds = new List<int>();

                                        foreach (var orderItem in order.OrderItems)
                                        {
                                            var mapping = _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                                            if (mapping != null)
                                            {
                                                var productCost = orderItem.Product.ProductCost;
                                                var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;
                                                var groupMembershipIds = _nexportService.GetProductGroupMembershipIds(mapping.Id);

                                                // Find existing invoice item for the order item
                                                var existingInvoiceItemId =
                                                    _nexportService.FindExistingInvoiceItemForOrderItem(order.Id,
                                                        orderItem.Id);

                                                // If the invoice item does not existed, then add the order item into the invoice
                                                if (existingInvoiceItemId == null ||
                                                    !invoiceDetails.InvoiceItems.Any(i => i.Id == existingInvoiceItemId.ToString()))
                                                {
                                                    string invoiceItemId;
                                                    if (mapping.Type == NexportProductTypeEnum.Catalog)
                                                    {
                                                        invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                                            orderInvoiceId,
                                                            mapping.NexportCatalogId,
                                                            Enums.ProductTypeEnum.Catalog, productCost,
                                                            subscriptionOrgId, groupMembershipIds,
                                                            mapping.UtcAccessExpirationDate,
                                                            mapping.AccessTimeLimit.ToString());
                                                    }
                                                    else
                                                    {
                                                        invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(
                                                            orderInvoiceId,
                                                            mapping.NexportCatalogSyllabusLinkId.Value,
                                                            Enums.ProductTypeEnum.Syllabus, productCost,
                                                            subscriptionOrgId, groupMembershipIds,
                                                            mapping.UtcAccessExpirationDate,
                                                            mapping.AccessTimeLimit.ToString());
                                                    }

                                                    var nexportOrderInvoiceItem = new NexportOrderInvoiceItem()
                                                    {
                                                        OrderId = queueItem.OrderId,
                                                        OrderItemId = orderItem.Id,
                                                        InvoiceItemId = Guid.Parse(invoiceItemId),
                                                        InvoiceId = orderInvoiceId,
                                                        UtcDateProcessed = DateTime.UtcNow
                                                    };

                                                    _nexportService.InsertOrUpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);

                                                    // Add the invoice item for auto redeeming after committing the invoice if AutoRedeem is set on the mapping
                                                    if (mapping.AutoRedeem)
                                                    {
                                                        autoRedeemingInvoiceItemIds.Add(nexportOrderInvoiceItem.Id);
                                                    }

                                                    invoiceTotalCost += productCost;
                                                }
                                            }
                                        }

                                        // Add payment
                                        _nexportService.AddPaymentToNexportOrderInvoice(orderInvoiceId,
                                            invoiceTotalCost, userMapping.NexportUserId, queueItemId, DateTime.UtcNow);

                                        // Commit the invoice
                                        _nexportService.CommitNexportOrderInvoiceTransaction(orderInvoiceId);

                                        // Redeem the invoice item that has the AutoRedeem option set on the mapping
                                        foreach (var invoiceItem in autoRedeemingInvoiceItemIds
                                            .Select(autoRedeemingInvoiceItemId => _nexportService.FindNexportOrderInvoiceItemById(autoRedeemingInvoiceItemId))
                                            .Where(invoiceItem => invoiceItem != null))
                                        {
                                            _nexportService.RedeemNexportOrder(invoiceItem, userMapping.NexportUserId);

                                            _nexportService.AddOrderNote(order,
                                                $"Nexport invoice item {invoiceItem.InvoiceItemId} has been redeemed for user {userMapping.NexportUserId}");
                                        }

                                        _logger.Information(
                                            $"Order {queueItem.OrderId} has been successfully processed!");

                                        _nexportService.AddOrderNote(order,
                                            "Nexport invoice has been successfully processed");
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

                        // Finish the order process and set the status to Complete
                        _orderProcessingService.CheckOrderStatus(order);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportRedemptionProcessingQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot process the NexportRedemptionProcessingQueue item", ex);
            }
        }

        private void LogAndAddOrderNoteForError(Order order, string errMsg)
        {
            var ex = new Exception(errMsg);
            _logger.Error(errMsg, ex);

            _nexportService.AddOrderNote(order, errMsg);
        }
    }
}
