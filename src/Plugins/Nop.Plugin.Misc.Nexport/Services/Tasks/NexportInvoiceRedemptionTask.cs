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

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                _batchSize = await _settingService.GetSettingByKeyAsync(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey,
                    NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSize);

                var queueItems = _nexportOrderInvoiceRedemptionQueueRepository
                    .Table
                    .OrderBy(q => q.UtcLastFailedDate)
                    .ThenBy(q => q.UtcDateCreated)
                    .Select(q => q.Id)
                    .Take(_batchSize)
                    .ToList();

                await ProcessNexportOrderInvoiceRedemptionsAsync(queueItems);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process Nexport order invoice redemption", ex);
            }
        }

        public async Task ProcessNexportOrderInvoiceRedemptionsAsync(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                    try
                    {
                        var queueItem = await _nexportOrderInvoiceRedemptionQueueRepository.GetByIdAsync(queueItemId);

                        if (queueItem == null)
                            return;

                        await _logger.DebugAsync($"Begin processing order invoice redemption for user {queueItem.RedeemingUserId} with invoice item {queueItem.OrderInvoiceItemId}");

                        var invoiceItem =
                            await _nexportService.FindNexportOrderInvoiceItemById(queueItem.OrderInvoiceItemId);
                        if (invoiceItem != null)
                        {
                            var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);

                            if (order != null)
                                if (queueItem.RetryCount > MAX_RETRY_COUNT)
                                {
                                    await DeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                    await CleanUpStoredMappingInfoAsync(queueItem.OrderItemId);

                                    // Complete the order
                                    await _orderProcessingService.CheckOrderStatusAsync(order);
                                }
                                else
                                    try
                                    {
                                        var orderItem = await _orderService.GetOrderItemByIdAsync(queueItem.OrderItemId);
                                        if (orderItem != null)
                                        {
                                            var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                                            var storeModelInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
                                                $"StoreModel-{order.Id}-{orderItem.Id}", order.StoreId);
                                            var storeModel = storeModelInfo != null ?
                                                JsonConvert.DeserializeObject<NexportStoreSaleModel>(storeModelInfo) :
                                                await _genericAttributeService.GetAttributeAsync<NexportStoreSaleModel>(store, "NexportStoreSaleModel", store.Id);

                                            // Only doing redemption logic if the store is under Retail mode
                                            if (storeModel == NexportStoreSaleModel.Retail)
                                            {
                                                var mappingInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
                                                $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                                                var productMapping = mappingInfo != null ?
                                                    JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo) :
                                                    await _nexportService.GetProductMappingById(queueItem.ProductMappingId);

                                                var nexportUserMapping =
                                                    await _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);

                                                if (productMapping != null)
                                                {
                                                    var userMapping = await _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);
                                                    if (userMapping != null)
                                                    {
                                                        // Get the invoice details from Nexport (if existed)
                                                        var invoiceDetails = await _nexportService.GetNexportInvoiceAsync(invoiceItem.InvoiceId);

                                                        // Continue to process only if the invoice is opening
                                                        if (invoiceDetails?.State == GetInvoiceResponse.StateEnum.Committed)
                                                        {
                                                            // Redeem the invoice based on the enrollment condition
                                                            await RedeemNexportInvoiceAsync(productMapping, userMapping,
                                                                invoiceItem, queueItem.RedeemingUserId,
                                                                queueItem.ManualApprovalAction);

                                                            await _nexportService.AddOrderNoteAsync(order,
                                                                $"Nexport invoice item {invoiceItem.InvoiceItemId} has been redeemed for user {queueItem.RedeemingUserId}");

                                                            // Find the list of supplemental question Ids that match the current product mapping
                                                            var questionIds = (await _nexportService
                                                                .GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id))
                                                                .Select(x => x.QuestionId)
                                                                .ToList();

                                                            // Get the supplemental question Ids that the current customer does not have answers yet
                                                            var questionWithoutAnswerIds = await _nexportService
                                                                .GetUnansweredQuestions(nexportUserMapping.NopUserId, store.Id, questionIds);

                                                            // Generate new requirement entities for each missing supplemental question
                                                            foreach (var questionId in questionWithoutAnswerIds)
                                                                await _nexportService.InsertNexportRequiredSupplementalInfo(
                                                                    new NexportRequiredSupplementalInfo
                                                                    {
                                                                        CustomerId = nexportUserMapping.NopUserId,
                                                                        StoreId = store.Id,
                                                                        QuestionId = questionId,
                                                                        UtcDateCreated = DateTime.UtcNow
                                                                    });

                                                            // Schedule a registration field synchronization for the customer
                                                            await _nexportService.InsertNexportRegistrationFieldSynchronizationQueueItem(
                                                                new NexportRegistrationFieldSynchronizationQueueItem
                                                                {
                                                                    CustomerId = nexportUserMapping.NopUserId,
                                                                    UtcDateCreated = DateTime.UtcNow
                                                                });

                                                            // Finish the order process and set the status to Complete
                                                            await _orderProcessingService.CheckOrderStatusAsync(order);

                                                            // Remove the queue item
                                                            await _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);

                                                            await _logger.InformationAsync(
                                                                $"Order invoice redemption queue item {queueItemId} for order {order.Id} has been processed and removed!");

                                                            await CleanUpStoredMappingInfoAsync(orderItem.Id);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        await _logger.ErrorAsync($"Failed to redeem Nexport invoice item {invoiceItem.InvoiceItemId} for user {queueItem.RedeemingUserId}", ex);

                                        queueItem.RetryCount++;

                                        if (queueItem.RetryCount <= MAX_RETRY_COUNT)
                                        {
                                            await _nexportService.AddOrderNoteAsync(order,
                                                $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be redeemed for user {queueItem.RedeemingUserId} and will be retry again for {MAX_RETRY_COUNT - queueItem.RetryCount} time(s)");

                                            await _nexportService.UpdateNexportOrderInvoiceRedemptionQueueItem(queueItem);
                                        }
                                        else
                                        {
                                            await DeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                            await CleanUpStoredMappingInfoAsync(queueItem.OrderItemId);

                                            // Complete the order
                                            await _orderProcessingService.CheckOrderStatusAsync(order);
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

        /// <summary>
        /// Redeem the Nexport invoice based on the enrollment condition (if existed)
        /// </summary>
        /// <param name="productMapping">The Nexport product mapping</param>
        /// <param name="userMapping">The Nexport user mapping</param>
        /// <param name="invoiceItem">The Nexport order invoice item</param>
        /// <param name="redeemingUserId">The Nexport user Id</param>
        /// <param name="extensionAction">The extension action: 1 - Renew and extend enrollment; 2 - Restart enrollment</param>
        private async Task RedeemNexportInvoiceAsync(NexportProductMapping productMapping, NexportUserMapping userMapping,
            NexportOrderInvoiceItem invoiceItem, Guid redeemingUserId, int? extensionAction = null)
        {
            if (productMapping == null)
                throw new ArgumentNullException(nameof(productMapping));

            if (userMapping == null)
                throw new ArgumentNullException(nameof(userMapping));

            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            if (redeemingUserId == Guid.Empty)
                throw new ArgumentException("Redeeming user Id cannot be an empty identifier", nameof(redeemingUserId));

            var existingEnrollmentStatus = await _nexportService.VerifyNexportEnrollmentStatusAsync(productMapping, userMapping);

            if (existingEnrollmentStatus != null)
                switch (existingEnrollmentStatus)
                {
                    case var status
                        when status.Value.Phase == Enums.PhaseEnum.Finished && status.Value.Result == Enums.ResultEnum.Failing:
                        {
                            await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment);
                            break;
                        }

                    case var status
                        when status.Value.Phase == Enums.PhaseEnum.Finished && status.Value.Result == Enums.ResultEnum.Passing:
                        {
                            await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment);
                            break;
                        }

                    case var status
                        when status.Value.Phase == Enums.PhaseEnum.InProgress || status.Value.Phase == Enums.PhaseEnum.NotStarted:
                        {
                            var currentEnrollmentExpirationDate = status.Value.EnrollmentExpirationDate;
                            if (currentEnrollmentExpirationDate.HasValue && currentEnrollmentExpirationDate >= DateTime.UtcNow)
                                // Renew the enrollment since the current enrollment has not expired yet
                                await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                    RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption);
                            else
                                if (status.Value.Phase == Enums.PhaseEnum.InProgress ||
                                    status.Value.Phase == Enums.PhaseEnum.NotStarted && productMapping.AllowExtension)
                                if (productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.Auto)
                                    // Renew or delete depends on the threshold setting if the product type is section
                                    if (productMapping.Type == NexportProductTypeEnum.Section)
                                    {
                                        var completionThreshold = productMapping.RenewalCompletionThreshold;
                                        if (completionThreshold.HasValue)
                                            await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                                completionThreshold > status.Value.CompletionPercentage
                                                    ? RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption
                                                    : RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RestartEnrollment);
                                    }
                                    else
                                        // Only renew the enrollment for training plan and catalog
                                        await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                            RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption);
                                else
                                    if (extensionAction != null)
                                    await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                        extensionAction == 1
                                            ? RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption
                                            : RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RestartEnrollment);
                            else
                                // Delete current enrollment and create new enrollment when the enrollment has been started
                                // and the product does not allow extension.
                                await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                    RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment);

                            break;
                        }
                }
            else
                await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId);
        }

        private async Task DeleteRedemptionQueueItemAndAddFinalOrderNote(Order order,
            NexportOrderInvoiceRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
        {
            await _nexportService.AddOrderNoteAsync(order,
                $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be automatically redeemed for user {queueItem.RedeemingUserId}. " +
                "However, this invoice item can still be manually redeem by the user in the order history page.");

            await _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);
        }

        private async Task CleanUpStoredMappingInfoAsync(int orderItemId)
        {
            var cleanUpAttributes = await _genericAttributeService.GetAttributesForEntityAsync(orderItemId, "OrderItem");
            await _genericAttributeService.DeleteAttributesAsync(cleanUpAttributes);
        }
    }
}
