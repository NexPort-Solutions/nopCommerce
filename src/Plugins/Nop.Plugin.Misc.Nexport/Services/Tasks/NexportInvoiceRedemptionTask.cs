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
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;

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
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize;
        private const int MAX_RETRY_COUNT = 5;

        public NexportInvoiceRedemptionTask(
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
                                        var orderItem = _orderService.GetOrderItemById(queueItem.OrderItemId);
                                        if (orderItem != null)
                                        {
                                            var store = _storeService.GetStoreById(order.StoreId);
                                            var storeModelInfo = _genericAttributeService.GetAttribute<string>(orderItem,
                                                $"StoreModel-{order.Id}-{orderItem.Id}", order.StoreId);
                                            var storeModel = storeModelInfo != null ?
                                                JsonConvert.DeserializeObject<NexportStoreSaleModel>(storeModelInfo) :
                                                _genericAttributeService.GetAttribute<NexportStoreSaleModel>(store, "NexportStoreSaleModel", store.Id);
                                            if (storeModel == NexportStoreSaleModel.Retail)
                                            {
                                                var mappingInfo = _genericAttributeService.GetAttribute<string>(orderItem,
                                                $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                                                var productMapping = mappingInfo != null ?
                                                    JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo) :
                                                    _nexportService.GetProductMappingById(queueItem.ProductMappingId);

                                                var nexportUserMapping =
                                                    _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);

                                                if (productMapping != null)
                                                {
                                                    var userMapping = _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);
                                                    if (userMapping != null)
                                                    {
                                                        //TODO: Verify invoice state before verifying enrollment status

                                                        var existingEnrollmentStatus = _nexportService.VerifyNexportEnrollmentStatus(productMapping, userMapping);

                                                        if (existingEnrollmentStatus != null)
                                                        {
                                                            switch (existingEnrollmentStatus)
                                                            {
                                                                case var status
                                                                    when status.Value.Phase == Enums.PhaseEnum.Finished && status.Value.Result == Enums.ResultEnum.Failing:
                                                                    {
                                                                        _nexportService.RedeemNexportInvoiceItem(invoiceItem, queueItem.RedeemingUserId,
                                                                            RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment);
                                                                        break;
                                                                    }

                                                                case var status
                                                                    when status.Value.Phase == Enums.PhaseEnum.Finished && status.Value.Result == Enums.ResultEnum.Passing:
                                                                    {
                                                                        _nexportService.RedeemNexportInvoiceItem(invoiceItem, queueItem.RedeemingUserId,
                                                                            RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment);
                                                                        break;
                                                                    }

                                                                case var status
                                                                    when status.Value.Phase == Enums.PhaseEnum.InProgress:
                                                                    {
                                                                        _nexportService.RedeemNexportInvoiceItem(invoiceItem, queueItem.RedeemingUserId,
                                                                            RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption);
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _nexportService.RedeemNexportInvoiceItem(invoiceItem, queueItem.RedeemingUserId);
                                                        }

                                                        _nexportService.AddOrderNote(order,
                                                            $"Nexport invoice item {invoiceItem.InvoiceItemId} has been redeemed for user {queueItem.RedeemingUserId}");

                                                        var questionIds = _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id)
                                                                .Select(x => x.QuestionId).ToList();

                                                        var questionWithoutAnswerIds =
                                                            _nexportService.GetUnansweredQuestions(
                                                                nexportUserMapping.NopUserId, store.Id, questionIds);

                                                        foreach (var questionId in questionWithoutAnswerIds)
                                                        {
                                                            _nexportService.InsertNexportRequiredSupplementalInfo(
                                                                new NexportRequiredSupplementalInfo
                                                                {
                                                                    CustomerId = nexportUserMapping.NopUserId,
                                                                    StoreId = store.Id,
                                                                    QuestionId = questionId,
                                                                    UtcDateCreated = DateTime.UtcNow
                                                                });
                                                        }

                                                        _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);

                                                        _logger.Information($"Order invoice redemption queue item {queueItemId} for order {order.Id} has been processed and removed!");

                                                        // Finish the order process and set the status to Complete
                                                        _orderProcessingService.CheckOrderStatus(order);

                                                        CleanUpStoredMappingInfo(orderItem.Id);

                                                        _nexportService.InsertNexportRegistrationFieldSynchronizationQueueItem(
                                                            new NexportRegistrationFieldSynchronizationQueueItem
                                                            {
                                                                CustomerId = nexportUserMapping.NopUserId,
                                                                UtcDateCreated = DateTime.UtcNow
                                                            });
                                                    }
                                                }
                                            }
                                        }
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
                                            CleanUpStoredMappingInfo(queueItem.OrderItemId);

                                            _orderProcessingService.CheckOrderStatus(order);
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

        private void CleanUpStoredMappingInfo(int orderItemId)
        {
            var cleanUpAttributes = _genericAttributeService.GetAttributesForEntity(orderItemId, "OrderItem");
            _genericAttributeService.DeleteAttributes(cleanUpAttributes);
        }
    }
}
