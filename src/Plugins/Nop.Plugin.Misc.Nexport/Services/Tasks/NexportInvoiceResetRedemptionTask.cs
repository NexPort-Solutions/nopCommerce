using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
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
        private readonly IRepository<NexportOrderInvoiceRedemptionQueueItem> _nexportOrderInvoiceRedemptionQueueRepository;
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
            IRepository<NexportOrderInvoiceRedemptionQueueItem> nexportOrderInvoiceRedemptionQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _settingService = settingService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _nexportOrderInvoiceRedemptionQueueRepository = nexportOrderInvoiceRedemptionQueueRepository;
            _nexportService = nexportService;
        }

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                _batchSize = await _settingService.GetSettingByKeyAsync(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey,
                    NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSize);
                 
                //get queue items from reset invoice repository
                var queueItems = new List<int>();
                //var queueItems = _nexportOrderInvoiceRedemptionQueueRepository
                //    .Table
                //    .OrderBy(q => q.UtcLastFailedDate)
                //    .ThenBy(q => q.UtcDateCreated)
                //    .Select(q => q.Id)
                //    .Take(_batchSize)
                //    .ToList();

                await ResetNexportOrderInvoiceRedemptionsAsync(queueItems);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot reset Nexport order invoice redemption", ex);
            }
        }

        public async Task ResetNexportOrderInvoiceRedemptionsAsync(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                    try
                    {
                       // var queueItem = await _nexportOrderInvoiceRedemptionQueueRepository.GetByIdAsync(queueItemId);

                       // if (queueItem == null)
                         //   return;

                        //await _logger.DebugAsync($"Begin resetting order invoice redemption for user {queueItem.RedeemingUserId} with invoice item {queueItem.OrderInvoiceItemId}");


                        //var invoiceItem =
                        //    await _nexportService.FindNexportOrderInvoiceItemById(queueItem.OrderInvoiceItemId);
                        //if (invoiceItem != null)
                        //{
                      
                        //}
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot process the NexportOrderInvoiceResetRedemptionQueue item with Id {queueItemId}", ex);
                    }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot process the NexportOrderInvoiceResetRedemptionQueue", ex);
            }
        }

        //delete the queue item after the work is done
        //private async Task DeleteResetRedemptionQueueItemAndAddFinalOrderNote(Order order,
        //    NexportOrderInvoiceRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
        //{
        //    await _nexportService.AddOrderNoteAsync(order,
        //        $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be automatically redeemed for user {queueItem.RedeemingUserId}. " +
        //        "However, this invoice item can still be manually redeem by the user in the order history page.");

        //    await _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);
        //}
    }
}
