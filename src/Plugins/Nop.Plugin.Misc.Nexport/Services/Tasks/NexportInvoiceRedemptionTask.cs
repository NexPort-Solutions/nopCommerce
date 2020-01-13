using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportInvoiceRedemptionTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IRepository<NexportOrderInvoiceRedemptionQueueItem> _nexportOrderInvoiceRedemptionQueueRepository;
        private readonly NexportService _nexportService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ISettingService _settingService;

        private int _batchSize;
        private const int MAX_RETRY_COUNT = 5;

        public NexportInvoiceRedemptionTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ISettingService settingService,
            IRepository<NexportOrderInvoiceRedemptionQueueItem> nexportOrderInvoiceRedemptionQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _settingService = settingService;
            _nexportOrderInvoiceRedemptionQueueRepository = nexportOrderInvoiceRedemptionQueueRepository;
            _nexportService = nexportService;
        }

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                _batchSize = _settingService.GetSettingByKey(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey,
                    NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSize);

                var queueItems = (from q in _nexportOrderInvoiceRedemptionQueueRepository.Table
                                  orderby q.UtcLastFailedDate,
                                          q.UtcDateCreated
                                  select q.Id).Take(_batchSize).ToList();

                ProcessNexportOrderInvoiceRedemptions(queueItems);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process Nexport order invoice redemption", ex);
            }
        }

        public void ProcessNexportOrderInvoiceRedemptions(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var queueItem = _nexportOrderInvoiceRedemptionQueueRepository.GetById(queueItemId);

                        if (queueItem == null)
                            return;

                        _logger.Debug($"Begin processing order invoice redemption for user {queueItem.RedeemingUserId} with invoice item {queueItem.OrderInvoiceItemId}");

                        var invoiceItem =
                            _nexportService.FindNexportOrderInvoiceItemById(queueItem.OrderInvoiceItemId);
                        if (invoiceItem != null)
                        {
                            var order = _orderService.GetOrderById(invoiceItem.OrderId);

                            if (order != null)
                            {
                                if (queueItem.RetryCount > MAX_RETRY_COUNT)
                                {
                                    DeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                }
                                else
                                {
                                    try
                                    {
                                        _nexportService.RedeemNexportInvoiceItem(invoiceItem,
                                            queueItem.RedeemingUserId);

                                        _nexportService.AddOrderNote(order,
                                            $"Nexport invoice item {invoiceItem.InvoiceItemId} has been redeemed for user {queueItem.RedeemingUserId}");

                                        _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);

                                        _logger.Information($"Order invoice redemption queue item {queueItemId} for order {order.Id} has been processed and removed!");

                                        // Finish the order process and set the status to Complete
                                        _orderProcessingService.CheckOrderStatus(order);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error($"Failed to redeem Nexport invoice item {invoiceItem.InvoiceItemId} for user {queueItem.RedeemingUserId}", ex);

                                        queueItem.RetryCount++;

                                        if (queueItem.RetryCount <= MAX_RETRY_COUNT)
                                        {
                                            _nexportService.AddOrderNote(order,
                                                $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be redeemed for user {queueItem.RedeemingUserId} and will be retry again for {MAX_RETRY_COUNT - queueItem.RetryCount} time(s)",
                                                updateOrder: true);

                                            _nexportService.UpdateNexportOrderInvoiceRedemptionQueueItem(queueItem);
                                        }
                                        else
                                        {
                                            DeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportOrderInvoiceRedemptionQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot process the NexportOrderInvoiceRedemptionQueue", ex);
            }
        }

        private void DeleteRedemptionQueueItemAndAddFinalOrderNote(Order order, NexportOrderInvoiceRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
        {
            _nexportService.AddOrderNote(order, $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be automatically redeemed for user {queueItem.RedeemingUserId}. " +
                                                "However, this invoice item can still be manually redeem by the user in the order history page.", updateOrder: true);

            _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);
        }
    }
}
