using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportInvoiceResetRedemptionTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IRepository<NexportOrderInvoiceResetRedemptionQueueItem> _nexportOrderInvoiceResetRedemptionQueueRepository;
        private readonly NexportService _nexportService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize;
        private const int MAX_RETRY_COUNT = 5;

        public NexportInvoiceResetRedemptionTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ISettingService settingService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            IRepository<NexportOrderInvoiceResetRedemptionQueueItem> nexportOrderInvoiceResetRedemptionQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _settingService = settingService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _nexportOrderInvoiceResetRedemptionQueueRepository = nexportOrderInvoiceResetRedemptionQueueRepository;
            _nexportService = nexportService;
        }

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                _batchSize = await _settingService.GetSettingByKeyAsync(NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSizeSettingKey,
                    NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSize);

                var queueItems = _nexportOrderInvoiceResetRedemptionQueueRepository
                    .Table
                    .OrderBy(q => q.UtcLastFailedDate)
                    .ThenBy(q => q.UtcDateCreated)
                    .Select(q => q.Id)
                    .Take(_batchSize)
                    .ToList();

                await ProcessNexportOrderInvoiceResetRedemptionsAsync(queueItems);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process Nexport order invoice reset redemptions", ex);
            }
        }

        public async Task ProcessNexportOrderInvoiceResetRedemptionsAsync(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                    try
                    {
                        var queueItem = await _nexportOrderInvoiceResetRedemptionQueueRepository.GetByIdAsync(queueItemId);

                        if (queueItem == null)
                            return;

                        await _logger.DebugAsync($"Begin resetting order invoice redemption with invoice item id: {queueItem.OrderInvoiceItemId}");


                        var invoiceItem =
                            await _nexportService.FindNexportOrderInvoiceItemById(queueItem.OrderInvoiceItemId);
                        if (invoiceItem != null)
                        {
                            var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);

                            await _nexportService.AddOrderNoteAsync(order,
                                $"Nexport invoice item {invoiceItem.InvoiceItemId}, order number {order.Id} has started the redemption resetting process");

                            if (order != null)
                            {

                                if (queueItem.RetryCount > MAX_RETRY_COUNT)
                                {
                                    await DeleteResetRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                }
                                else
                                {
                                    try
                                    {
                                        var orderItem =
                                            await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);
                                        if (orderItem != null)
                                        {
                                            // Get the invoice details from Nexport (if existed)
                                            var invoiceDetails =
                                                await _nexportService.GetNexportInvoiceAsync(invoiceItem
                                                    .InvoiceId);

                                            // Continue to process only if the invoice is committed
                                            if (invoiceDetails?.State ==
                                                GetInvoiceResponse.StateEnum.Committed)
                                            {

                                                var oldUserId = invoiceItem.RedeemingUserId;

                                                invoiceItem.RedeemingUserId = null;
                                                var redemptionCode =
                                                    (await _genericAttributeService
                                                        .GetAttributesForEntityAsync(
                                                            1, "testapi")).FirstOrDefault(x =>
                                                        x.Key == "redemptioncode");
                                                invoiceItem.InvoiceItemRedemptionCode =
                                                    redemptionCode.Value;
                                                invoiceItem.UtcDateRedemption = null;
                                                invoiceItem.RedemptionEnrollmentId = null;

                                                // if something fails and redemption code comes back null
                                                // then try to get the invoice redemption from api one more time
                                                if (invoiceItem.InvoiceItemRedemptionCode == null)
                                                {
                                                    var invoiceRedemptionResponse = await _nexportService.GetNexportInvoiceRedemptionAsync(
                                                        invoiceItem.InvoiceItemId);
                                                    if (invoiceRedemptionResponse != null)
                                                    {
                                                        invoiceItem.InvoiceItemRedemptionCode =
                                                            invoiceRedemptionResponse.RedemptionCode;
                                                    }
                                                }

                                                // if redemption code is still null at this point then we retry the reset task
                                                // until the max retry is reached for the queue item 
                                                if (invoiceItem.InvoiceItemRedemptionCode == null)
                                                {
                                                    await RetryResetRedemptionTask(queueItem, order, invoiceItem);
                                                }
                                                else
                                                {
                                                    invoiceItem.RedemptionStatus =
                                                        NexportOrderInvoiceItemRedemptionStatus.Available;

                                                    //update invoice item with new redemption code so it can be reassigned later
                                                    await _nexportService.UpdateNexportOrderInvoiceItem(
                                                        invoiceItem);

                                                    var wholesaleOrderInfo = await _nexportService.GetWholesaleOrderInfoForOrderItemAsync(order.Id, orderItem.Id);
                                                    if (wholesaleOrderInfo != null)
                                                    {
                                                        if (wholesaleOrderInfo.Redeemed > 0)
                                                        {
                                                            wholesaleOrderInfo.Redeemed--;
                                                            wholesaleOrderInfo.Available++;

                                                            await _nexportService.UpdateWholesaleOrderInfoAsync(
                                                                wholesaleOrderInfo);
                                                        }
                                                    }

                                                    await _nexportService.DeleteNexportOrderInvoiceResetRedemptionQueueItem(queueItem);

                                                    await _nexportService.AddOrderNoteAsync(order,
                                                        $"Nexport invoice item {invoiceItem.InvoiceItemId} that was assigned to user {oldUserId} has been reset");

                                                    await _logger.InformationAsync($"Order invoice reset redemption queue item {queueItemId} for order {order.Id} has been processed and removed!");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        await _logger.ErrorAsync(
                                            $"Failed to reset redemption of Nexport invoice item {invoiceItem.InvoiceItemId}",
                                            ex);

                                        await RetryResetRedemptionTask(queueItem, order, invoiceItem);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot process the NexportOrderInvoiceRedemptionQueue item with Id {queueItemId}", ex);
                    }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot process the NexportOrderInvoiceRedemptionQueue", ex);
            }
        }

        private async Task RetryResetRedemptionTask(NexportOrderInvoiceResetRedemptionQueueItem queueItem, Order order, NexportOrderInvoiceItem invoiceItem)
        {
            queueItem.RetryCount++;

            if (queueItem.RetryCount <= MAX_RETRY_COUNT)
            {
                await _nexportService.AddOrderNoteAsync(order,
                    $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be reset and will be retry again for {MAX_RETRY_COUNT - queueItem.RetryCount} time(s)");

                await _nexportService.UpdateNexportOrderInvoiceResetRedemptionQueueItem(
                    queueItem);
            }
            else
            {
                await DeleteResetRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
            }
        }

        private async Task DeleteResetRedemptionQueueItemAndAddFinalOrderNote(Order order,
            NexportOrderInvoiceResetRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
        {
            await _nexportService.AddOrderNoteAsync(order,
                $"Nexport invoice item with id:{invoiceItem.InvoiceItemId} redemption cannot be reset");

            await _nexportService.DeleteNexportOrderInvoiceResetRedemptionQueueItem(queueItem);
        }
    }
}
