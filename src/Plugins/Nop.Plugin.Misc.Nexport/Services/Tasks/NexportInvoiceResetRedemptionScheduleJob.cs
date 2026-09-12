using Hangfire;
using NexportApi.Model;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Filters;
using Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class NexportInvoiceResetRedemptionScheduleJob(
    IWidgetPluginManager widgetPluginManager,
    ILogger logger,
    IOrderService orderService,
    IOrderProcessingService orderProcessingService,
    ISettingService settingService,
    IStoreService storeService,
    IGenericAttributeService genericAttributeService,
    IRepository<NexportOrderInvoiceResetRedemptionQueueItem> nexportOrderInvoiceResetRedemptionQueueRepository,
    NexportService nexportService)
    : INexportScheduleJob
{
    private int _batchSize;
    private const int MAX_RETRY_COUNT = 5;

    public string JobName { get; set; } = "NexportInvoiceResetRedemption";

    public long Interval { get; set; } = 5; // Default to 5 seconds

    [SkipConcurrentExecution]
    public async Task ExecuteAsync()
    {
        if (!await widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
        {
            await logger.WarningAsync("InvoiceResetRedemption job cannot be executed due to Nexport plugin is not currently active!");
            return;
        }

        try
        {
            _batchSize = await settingService.GetSettingByKeyAsync(NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSizeSettingKey,
                NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSize);

            var queueItems = await nexportOrderInvoiceResetRedemptionQueueRepository
                .Table
                .OrderBy(q => q.UtcLastFailedDate)
                .ThenBy(q => q.UtcDateCreated)
                .Take(_batchSize)
                .ToListAsync();

            await ProcessNexportOrderInvoiceResetRedemptionsAsync(queueItems);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Cannot process Nexport order invoice reset redemptions", ex);
        }
    }

    public async Task ProcessNexportOrderInvoiceResetRedemptionsAsync(IList<NexportOrderInvoiceResetRedemptionQueueItem> queueItems)
    {
        try
        {
            foreach (var queueItem in queueItems)
                try
                {
                    await logger.DebugAsync($"Begin resetting order invoice redemption with invoice item id: {queueItem.OrderInvoiceItemId}");

                    var invoiceItem = await nexportService.FindNexportOrderInvoiceItemById(queueItem.OrderInvoiceItemId);
                    if (invoiceItem != null)
                    {
                        var order = await orderService.GetOrderByIdAsync(invoiceItem.OrderId);

                        await nexportService.AddOrderNoteAsync(order,
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
                                    var orderItem = await orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);
                                    if (orderItem != null)
                                    {
                                        // Get the invoice details from Nexport (if existed)
                                        var invoiceDetails = await nexportService.GetNexportInvoiceAsync(invoiceItem.InvoiceId);

                                        // Continue to process only if the invoice is committed
                                        if (invoiceDetails?.State == GetInvoiceResponse.StateEnum.Committed)
                                        {
                                            var oldUserId = invoiceItem.RedeemingUserId;

                                            invoiceItem = await nexportService.ResetInvoiceRedemptionAsync(invoiceItem);

                                            // If something fails and redemption code comes back null
                                            // then try to get the invoice redemption from api one more time
                                            if (invoiceItem.InvoiceItemRedemptionCode == null)
                                            {
                                                var invoiceRedemptionResponse = await nexportService.GetNexportInvoiceRedemptionAsync(invoiceItem.InvoiceItemId);
                                                if (invoiceRedemptionResponse != null)
                                                {
                                                    invoiceItem.InvoiceItemRedemptionCode = invoiceRedemptionResponse.RedemptionCode;
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
                                                var wholesaleOrderInfo = await nexportService.GetWholesaleOrderInfoForOrderItemAsync(order.Id, orderItem.Id);
                                                if (wholesaleOrderInfo != null)
                                                {
                                                    if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable)
                                                    {
                                                        invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Available;
                                                        wholesaleOrderInfo.ProcessingAvailable--;
                                                    }
                                                    else if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting)
                                                    {
                                                        invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Awaiting;
                                                        wholesaleOrderInfo.ProcessingAwaiting--;
                                                    }

                                                    wholesaleOrderInfo.Available++;

                                                    // Update invoice item with new redemption code so it can be reassigned later
                                                    await nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);
                                                    await nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);
                                                }

                                                await nexportService.DeleteNexportOrderInvoiceResetRedemptionQueueItem(queueItem);

                                                await nexportService.AddOrderNoteAsync(order,
                                                    $"Nexport invoice item {invoiceItem.InvoiceItemId} that was assigned to user {oldUserId} has been reset");

                                                await logger.InformationAsync($"Order invoice reset redemption queue item {queueItem.Id} for order {order.Id} has been processed and removed!");

                                                var previousUser = await nexportService.FindUserMappingByNexportUserId(oldUserId.Value);
                                                if (previousUser != null)
                                                {
                                                    await nexportService.InsertNexportRedemptionAuditLogAsync(
                                                        new NexportRedemptionAuditLog
                                                        {
                                                            InvoiceItemId = invoiceItem.InvoiceItemId,
                                                            CustomerId = previousUser.NopUserId,
                                                            Description = "Invoice item had been unassigned",
                                                            Type = NexportRedemptionAuditLogTypeEnum.UnassignRedemption,
                                                            UtcDateCreated = DateTime.UtcNow
                                                        });
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await logger.ErrorAsync($"Failed to reset redemption of Nexport invoice item {invoiceItem.InvoiceItemId}", ex);

                                    await RetryResetRedemptionTask(queueItem, order, invoiceItem);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await logger.ErrorAsync($"Cannot process the NexportOrderInvoiceRedemptionQueue item with Id {queueItem.Id}", ex);
                }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"Cannot process the NexportOrderInvoiceRedemptionQueue", ex);
        }
    }

    private async Task RetryResetRedemptionTask(NexportOrderInvoiceResetRedemptionQueueItem queueItem, Order order, NexportOrderInvoiceItem invoiceItem)
    {
        queueItem.RetryCount++;

        if (queueItem.RetryCount <= MAX_RETRY_COUNT)
        {
            await nexportService.AddOrderNoteAsync(order,
                $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be reset and will be retry again for {MAX_RETRY_COUNT - queueItem.RetryCount} time(s)");

            await nexportService.UpdateNexportOrderInvoiceResetRedemptionQueueItem(queueItem);
        }
        else
        {
            await DeleteResetRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
        }
    }

    private async Task DeleteResetRedemptionQueueItemAndAddFinalOrderNote(Order order,
        NexportOrderInvoiceResetRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
    {
        await nexportService.AddOrderNoteAsync(order, $"Nexport invoice item with Id: {invoiceItem.InvoiceItemId} redemption cannot be reset");

        await nexportService.DeleteNexportOrderInvoiceResetRedemptionQueueItem(queueItem);
    }
}
