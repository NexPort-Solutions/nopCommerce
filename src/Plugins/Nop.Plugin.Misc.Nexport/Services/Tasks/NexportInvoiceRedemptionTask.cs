using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class NexportInvoiceRedemptionTask : IScheduleTask
{
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IRepository<NexportOrderInvoiceRedemptionQueueItem> _nexportOrderInvoiceRedemptionQueueRepository;
    private readonly NexportService _nexportService;
    private readonly INexportWholesaleService _nexportWholesaleService;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly ISettingService _settingService;
    private readonly IStoreService _storeService;
    private readonly IGenericAttributeService _genericAttributeService;

    private int _batchSize;
#if DEBUG
    private const int MAX_RETRY_COUNT = 1;
#else
    private const int MAX_RETRY_COUNT = 5;
#endif

    public NexportInvoiceRedemptionTask(
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        ISettingService settingService,
        IStoreService storeService,
        IGenericAttributeService genericAttributeService,
        IRepository<NexportOrderInvoiceRedemptionQueueItem> nexportOrderInvoiceRedemptionQueueRepository,
        NexportService nexportService,
        INexportWholesaleService nexportWholesaleService)
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
        _nexportWholesaleService = nexportWholesaleService;
    }

    public async Task ExecuteAsync()
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
        {
            return;
        }

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
            {
                try
                {
                    var queueItem = await _nexportOrderInvoiceRedemptionQueueRepository.GetByIdAsync(queueItemId);

                    if (queueItem == null)
                        return;

                    // Processing the redemption when the processing time has reached
                    if (queueItem.UtcProcessingDate != null && DateTime.UtcNow < queueItem.UtcProcessingDate)
                        return;

                    // When user Id is empty, this is a bad queue item. Therefore, we should increase the retry count and delete it
                    if (queueItem.RedeemingUserId == Guid.Empty)
                    {
                        queueItem.RetryCount = MAX_RETRY_COUNT + 1;
                        await _logger.WarningAsync($"Redeeming user id for redemption queue item: {queueItem.Id} is empty. Queue item will be deleted!");
                    }

                    await _logger.DebugAsync($"Begin processing order invoice redemption for user {queueItem.RedeemingUserId} with invoice item {queueItem.OrderInvoiceItemId}");

                    var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(queueItem.OrderInvoiceItemId);
                    if (invoiceItem != null)
                    {
                        var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                        if (order != null)
                        {
                            var isWholesale = await _genericAttributeService.GetAttributeAsync<bool>(order, "IsWholesaleOrder", order.StoreId);

                            if (queueItem.RetryCount > MAX_RETRY_COUNT)
                            {
                                if (isWholesale)
                                {
                                    await WholesaleDeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                }
                                else
                                {
                                    await RetailDeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                                    await CleanUpOrderItemAttributesAsync(queueItem.OrderItemId);
                                }

                                // Complete the order
                                await _orderProcessingService.CheckOrderStatusAsync(order);
                            }
                            else
                            {
                                var orderNoteMsg = $"Redemption process has been started for Nexport invoice item {invoiceItem.InvoiceItemId} (Order #{order.Id})";
                                if (queueItem.RetryCount is > 0 and <= MAX_RETRY_COUNT)
                                {
                                    orderNoteMsg += $"[{queueItem.RetryCount}]";
                                }

                                await _nexportService.AddOrderNoteAsync(order, orderNoteMsg);

                                if (isWholesale)
                                {
                                    await ProcessWholesaleRedemptionAsync(queueItem, order, invoiceItem);
                                }
                                else
                                {
                                    await ProcessRetailRedemptionAsync(queueItem, order, invoiceItem);
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
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Cannot process the NexportOrderInvoiceRedemptionQueue", ex);
        }
    }

    private async Task ProcessRetailRedemptionAsync(NexportOrderInvoiceRedemptionQueueItem queueItem, Order order, NexportOrderInvoiceItem invoiceItem)
    {
        try
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(queueItem.OrderItemId);
            if (orderItem != null)
            {
                var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                if (store != null)
                {
                    var mappingInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
                        $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                    var productMapping = mappingInfo != null
                        ? JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo)
                        : await _nexportService.GetProductMappingById(
                            queueItem.ProductMappingId);

                    var nexportUserMapping = await _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);

                    if (productMapping != null)
                    {
                        var userMapping = await _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);
                        if (userMapping != null)
                        {
                            // Get the invoice details from Nexport (if existed)
                            var invoiceDetails = await _nexportService.GetNexportInvoiceAsync(invoiceItem.InvoiceId)!;

                            // Continue to process only if the invoice is opening
                            if (invoiceDetails?.State == GetInvoiceResponse.StateEnum.Committed)
                            {
                                // Redeem the invoice based on the enrollment condition
                                await RedeemNexportInvoiceAsync(productMapping, userMapping,
                                    invoiceItem, queueItem.RedeemingUserId,
                                    queueItem.ManualApprovalAction);

                                var nexportUser = await _nexportService.GetNexportUserAsync(queueItem.RedeemingUserId)!;

                                var userText = $"user Id: [{queueItem.RedeemingUserId}]";

                                if (nexportUser != null)
                                {
                                    userText += $" ({nexportUser.FirstName} {nexportUser.LastName})";
                                }

                                var orderNoteMsg = $"Nexport invoice item {invoiceItem.InvoiceItemId} (Product: {productMapping.NexportProductName}) has been redeemed for {userText} in store {store.Name}";
                                await _nexportService.AddOrderNoteAsync(order, orderNoteMsg);

                                // Find the list of supplemental question Ids that match the current product mapping
                                var questionIds = (await _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id))
                                    .Select(x => x.QuestionId)
                                    .ToList();

                                // Get the supplemental question Ids that the current customer does not have answers yet
                                var questionWithoutAnswerIds = await _nexportService
                                    .GetUnansweredQuestions(nexportUserMapping.NopUserId,
                                        store.Id, questionIds);

                                // Generate new requirement entities for each missing supplemental question
                                foreach (var questionId in questionWithoutAnswerIds)
                                {
                                    await _nexportService.InsertNexportRequiredSupplementalInfo(
                                        new NexportRequiredSupplementalInfo
                                        {
                                            CustomerId = nexportUserMapping.NopUserId,
                                            StoreId = store.Id,
                                            QuestionId = questionId,
                                            UtcDateCreated = DateTime.UtcNow
                                        });
                                }

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

                                await _logger.InformationAsync($"Order invoice redemption queue item {queueItem.Id} for order {order.Id} has been processed and removed!");

                                await CleanUpOrderItemAttributesAsync(orderItem.Id);
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
                await RetailDeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                await CleanUpOrderItemAttributesAsync(queueItem.OrderItemId);

                // Complete the order
                await _orderProcessingService.CheckOrderStatusAsync(order);
            }
        }
    }

    private async Task ProcessWholesaleRedemptionAsync(NexportOrderInvoiceRedemptionQueueItem queueItem, Order order, NexportOrderInvoiceItem invoiceItem)
    {
        try
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(queueItem.OrderItemId);
            if (orderItem != null)
            {
                var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                if (store != null)
                {
                    var mappingInfo = await _genericAttributeService.GetAttributeAsync<string>(
                        orderItem, $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                    var productMapping = mappingInfo != null
                        ? JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo)
                        : await _nexportService.GetProductMappingById(queueItem.ProductMappingId);

                    var nexportUserMapping = await _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);

                    if (productMapping != null)
                    {
                        var userMapping = await _nexportService.FindUserMappingByNexportUserId(queueItem.RedeemingUserId);
                        if (userMapping != null)
                        {
                            // Get the invoice details from Nexport (if existed)
                            var invoiceDetails = await _nexportService.GetNexportInvoiceAsync(invoiceItem.InvoiceId)!;

                            // Continue to process only if the invoice is opening
                            if (invoiceDetails?.State == GetInvoiceResponse.StateEnum.Committed)
                            {
                                UpdatedInvoiceFields updatedInvoiceFields = null;
                                var redeemingProductMapping = productMapping;

                                if (productMapping.Type == NexportProductTypeEnum.OpenEnded &&
                                    productMapping.AssignWhenRedeemed.HasValue && productMapping.AssignWhenRedeemed.Value)
                                {
                                    NexportProductMapping selectedMappingForOpenEndedProduct = null;
                                    var selectedMappingForOpenEndedProductInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
                                        $"ProductMapping-OpenEnded-Selected-{order.Id}-{orderItem.Id}", order.StoreId);

                                    if (selectedMappingForOpenEndedProductInfo != null)
                                    {
                                        selectedMappingForOpenEndedProduct = JsonConvert.DeserializeObject<NexportProductMapping>(selectedMappingForOpenEndedProductInfo);
                                        if (selectedMappingForOpenEndedProduct == null && queueItem.RedeemingProductMappingId != null)
                                            selectedMappingForOpenEndedProduct = await _nexportService.GetProductMappingById(queueItem.RedeemingProductMappingId.Value);
                                    }
                                    else
                                    {
                                        if (queueItem.RedeemingProductMappingId != null)
                                            selectedMappingForOpenEndedProduct = await _nexportService.GetProductMappingById(queueItem.RedeemingProductMappingId.Value);
                                    }

                                    if (selectedMappingForOpenEndedProduct != null)
                                        redeemingProductMapping = selectedMappingForOpenEndedProduct;

                                    // Update new group memberships when redeeming the open-ended product
                                    IList<Guid> groupMembershipIds = new List<Guid>();

                                    var groupMembershipMappingInfo = (await _genericAttributeService.GetAttributesForEntityAsync(orderItem.Id, nameof(OrderItem)))
                                        .Where(a =>
                                            a.StoreId == order.StoreId &&
                                            a.Key.Contains($"ProductGroupMembershipMapping-OpenEnded-Selected-{order.Id}-{orderItem.Id}-{redeemingProductMapping.Id}"))
                                        .ToList();

                                    if (groupMembershipMappingInfo.Count > 0)
                                    {
                                        foreach (var groupMembershipMapping in groupMembershipMappingInfo
                                                     .Select(attribute => JsonConvert.DeserializeObject<NexportProductGroupMembershipMapping>(attribute.Value))
                                                     .Where(groupMembershipMapping => groupMembershipMapping != null))
                                        {
                                            if (!groupMembershipIds.Contains(groupMembershipMapping.NexportGroupId))
                                                groupMembershipIds.Add(groupMembershipMapping.NexportGroupId);
                                        }
                                    }
                                    else
                                    {
                                        groupMembershipIds = await _nexportService.GetProductGroupMembershipIds(productMapping.Id);
                                    }

                                    var redeemingPurchasingGroupId = await _genericAttributeService.GetAttributeAsync<Guid?>(invoiceItem,
                                        $"RedeemingPurchasingGroup-{invoiceItem.Id}", order.StoreId);

                                    string fundingPoolCode = null;
                                    var redeemingFundingPoolId = await _genericAttributeService.GetAttributeAsync<int?>(invoiceItem,
                                        $"RedeemingFundingPool-{invoiceItem.Id}", order.StoreId);
                                    if (redeemingFundingPoolId != null)
                                    {
                                        var redeemingFundingPool = await _nexportWholesaleService.GetFundingPoolById(redeemingFundingPoolId.Value);
                                        fundingPoolCode = redeemingFundingPool?.Code;
                                    }

                                    updatedInvoiceFields = new UpdatedInvoiceFields
                                    {
                                        GroupMemberships = groupMembershipIds.ToList(),
                                        PurchasingGroupId = redeemingPurchasingGroupId,
                                        FundingPool = fundingPoolCode
                                    };
                                }

                                var redeemingStore = store;
                                if (redeemingProductMapping.StoreId != null)
                                {
                                    redeemingStore = await _storeService.GetStoreByIdAsync(redeemingProductMapping.StoreId.Value) ?? store;
                                }

                                var ignoreEnrollmentStatus = await _genericAttributeService.GetAttributeAsync<bool>(
                                    invoiceItem, $"IgnoreEnrollmentStatus-{invoiceItem.Id}", order.StoreId);

                                // Redeem the invoice based on the enrollment condition
                                var redeemed = await RedeemNexportInvoiceAsync(redeemingProductMapping, userMapping,
                                    invoiceItem, queueItem.RedeemingUserId,
                                    queueItem.ManualApprovalAction, updatedInvoiceFields, ignoreEnrollmentStatus);

                                var nexportUser = await _nexportService.GetNexportUserAsync(queueItem.RedeemingUserId)!;
                                var userText = $"user Id: [{queueItem.RedeemingUserId}]";

                                if (nexportUser != null)
                                {
                                    userText += $" ({nexportUser.FirstName} {nexportUser.LastName})";
                                }

                                var orderNoteMsg = $"Nexport invoice item {invoiceItem.InvoiceItemId} (Product: {redeemingProductMapping.NexportProductName}) has been assigned & redeemed for {userText} in store {redeemingStore.Name}";
                                await _nexportService.AddOrderNoteAsync(order, orderNoteMsg);

                                // Find the list of supplemental question Ids that match the current product mapping
                                var questionIds = (await _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(redeemingProductMapping.Id))
                                    .Select(x => x.QuestionId)
                                    .ToList();

                                // Get the supplemental question Ids that the current customer does not have answers yet
                                var questionWithoutAnswerIds = await _nexportService.GetUnansweredQuestions(nexportUserMapping.NopUserId, redeemingStore.Id, questionIds);

                                // Generate new requirement entities for each missing supplemental question
                                foreach (var questionId in questionWithoutAnswerIds)
                                {
                                    await _nexportService.InsertNexportRequiredSupplementalInfo(
                                        new NexportRequiredSupplementalInfo
                                        {
                                            CustomerId = nexportUserMapping.NopUserId,
                                            StoreId = redeemingStore.Id,
                                            QuestionId = questionId,
                                            UtcDateCreated = DateTime.UtcNow
                                        });
                                }

                                // Schedule a registration field synchronization for the customer
                                await _nexportService.InsertNexportRegistrationFieldSynchronizationQueueItem(
                                    new NexportRegistrationFieldSynchronizationQueueItem
                                    {
                                        CustomerId = nexportUserMapping.NopUserId,
                                        UtcDateCreated = DateTime.UtcNow
                                    });

                                // Finish the order process and set the status to Complete
                                await _orderProcessingService.CheckOrderStatusAsync(order);

                                if (redeemed)
                                {
                                    var wholesaleOrderInfo = await _nexportService.GetWholesaleOrderInfoForOrderItemAsync(order.Id, orderItem.Id);
                                    if (wholesaleOrderInfo != null)
                                    {
                                        wholesaleOrderInfo.ProcessingAvailable--;
                                        wholesaleOrderInfo.Redeemed++;

                                        await _nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);
                                    }

                                    // Remove the queue item
                                    await _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);

                                    await _logger.InformationAsync($"Order invoice redemption queue item {queueItem.Id} for order {order.Id} has been processed and removed!");

                                    // Audit log for redemption
                                    await _nexportService.InsertNexportRedemptionAuditLogAsync(
                                        new NexportRedemptionAuditLog
                                        {
                                            InvoiceItemId = invoiceItem.InvoiceItemId,
                                            Description = $"Invoice item had been redeemed for {userText}",
                                            CustomerId = nexportUserMapping.NopUserId,
                                            Type = NexportRedemptionAuditLogTypeEnum.RedemptionRedeemed,
                                            UtcDateCreated = DateTime.UtcNow
                                        });
                                }
                                else
                                {
                                    queueItem.RetryCount++;

                                    if (queueItem.RetryCount <= MAX_RETRY_COUNT)
                                    {
                                        await _logger.WarningAsync($"Unable to redeem Nexport invoice item {invoiceItem.InvoiceItemId} for user {queueItem.RedeemingUserId} in the wholesale redemption process!");

                                        await _nexportService.AddOrderNoteAsync(order,
                                            $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be redeemed for user {queueItem.RedeemingUserId} and will be retry again!");

                                        await _nexportService.UpdateNexportOrderInvoiceRedemptionQueueItem(queueItem);
                                    }
                                    else
                                    {
                                        await _logger.ErrorAsync($"Failed to redeem Nexport invoice item {invoiceItem.InvoiceItemId} for user {queueItem.RedeemingUserId} in the wholesale redemption process!");

                                        await WholesaleDeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);

                                        // Complete the order
                                        await _orderProcessingService.CheckOrderStatusAsync(order);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to process wholesale redemption for Nexport invoice item {invoiceItem.InvoiceItemId} for user {queueItem.RedeemingUserId}", ex);

            queueItem.RetryCount++;

            if (queueItem.RetryCount <= MAX_RETRY_COUNT)
            {
                await _nexportService.AddOrderNoteAsync(order,
                    $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be redeemed for user {queueItem.RedeemingUserId} and will be retry again!");

                await _nexportService.UpdateNexportOrderInvoiceRedemptionQueueItem(queueItem);
            }
            else
            {
                await WholesaleDeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);

                // Complete the order
                await _orderProcessingService.CheckOrderStatusAsync(order);
            }
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
    /// <param name="updatedInvoiceFields">The invoice updated fields</param>
    /// <param name="ignoreEnrollmentStatus"></param>
    private async Task<bool> RedeemNexportInvoiceAsync(NexportProductMapping productMapping,
        NexportUserMapping userMapping,
        NexportOrderInvoiceItem invoiceItem, Guid redeemingUserId, int? extensionAction = null,
        UpdatedInvoiceFields updatedInvoiceFields = null, bool ignoreEnrollmentStatus = false)
    {
        ArgumentNullException.ThrowIfNull(productMapping);
        ArgumentNullException.ThrowIfNull(userMapping);
        ArgumentNullException.ThrowIfNull(invoiceItem);

        if (redeemingUserId == Guid.Empty)
            throw new ArgumentException("Redeeming user Id cannot be an empty identifier", nameof(redeemingUserId));

        var redeemed = false;

        if (ignoreEnrollmentStatus && extensionAction != null)
        {
            redeemed = await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId, productMapping,
                (RedeemInvoiceItemRequest.RedemptionActionTypeEnum)extensionAction, updatedInvoiceFields);
        }
        else
        {
            var existingEnrollmentStatus = await _nexportService.VerifyNexportEnrollmentStatusAsync(productMapping, userMapping);
            if (existingEnrollmentStatus != null)
            {
                switch (existingEnrollmentStatus)
                {
                    case { Phase: Enums.PhaseEnum.Finished, Result: Enums.ResultEnum.Failing }:
                    case { Phase: Enums.PhaseEnum.Finished, Result: Enums.ResultEnum.Passing }:
                        {
                            redeemed = await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId, productMapping,
                                RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment, updatedInvoiceFields);

                            break;
                        }

                    case var _
                        when existingEnrollmentStatus.Value.Phase is Enums.PhaseEnum.InProgress or Enums.PhaseEnum.NotStarted:
                        {
                            var currentEnrollmentExpirationDate = existingEnrollmentStatus.Value.EnrollmentExpirationDate;
                            if (currentEnrollmentExpirationDate.HasValue && currentEnrollmentExpirationDate >= DateTime.UtcNow)
                            {
                                // Renew the enrollment since the current enrollment has not expired yet
                                redeemed = await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId,
                                    productMapping, RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption, updatedInvoiceFields);
                            }
                            else
                            {
                                if (existingEnrollmentStatus.Value.Phase == Enums.PhaseEnum.InProgress ||
                                    existingEnrollmentStatus.Value.Phase == Enums.PhaseEnum.NotStarted &&
                                    productMapping.AllowExtension)
                                {
                                    if (productMapping.RenewalApprovalMethod == NexportEnrollmentRenewalApprovalMethodEnum.Auto)
                                    {
                                        // Renew or delete depends on the threshold setting if the product type is section
                                        if (productMapping.Type == NexportProductTypeEnum.Section)
                                        {
                                            var completionThreshold = productMapping.RenewalCompletionThreshold;
                                            if (completionThreshold.HasValue)
                                            {
                                                redeemed = await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem,
                                                    redeemingUserId, productMapping,
                                                    completionThreshold >
                                                    existingEnrollmentStatus.Value.CompletionPercentage
                                                        ? RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption
                                                        : RedeemInvoiceItemRequest.RedemptionActionTypeEnum
                                                            .RestartEnrollment,
                                                    updatedInvoiceFields);
                                            }
                                        }
                                        else
                                        {
                                            // Only renew the enrollment for training plan and catalog
                                            redeemed = await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem,
                                                redeemingUserId, productMapping,
                                                RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption,
                                                updatedInvoiceFields);
                                        }
                                    }
                                    else
                                    {
                                        redeemed = extensionAction != null
                                            ? await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem,
                                                redeemingUserId, productMapping,
                                                extensionAction == 1
                                                    ? RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RenewRedemption
                                                    : RedeemInvoiceItemRequest.RedemptionActionTypeEnum.RestartEnrollment,
                                                updatedInvoiceFields)
                                            :
                                            // Delete current enrollment and create new enrollment when the enrollment has been started
                                            // and the product does not allow extension.
                                            await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem,
                                                redeemingUserId, productMapping,
                                                RedeemInvoiceItemRequest.RedemptionActionTypeEnum.DeleteFinishedEnrollment,
                                                updatedInvoiceFields);
                                    }
                                }
                            }

                            break;
                        }
                }
            }
            else
            {
                redeemed = await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, redeemingUserId, productMapping, updatedInvoiceFields: updatedInvoiceFields);
            }
        }

        return redeemed;
    }

    private async Task WholesaleDeleteRedemptionQueueItemAndAddFinalOrderNote(Order order,
        NexportOrderInvoiceRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
    {
        var wholesaleOrderInfo = await _nexportService.GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);

        if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable)
        {
            invoiceItem.RedeemingUserId = null;
            invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Available;

            wholesaleOrderInfo.Available++;
            wholesaleOrderInfo.ProcessingAvailable--;
        }
        else if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting)
        {
            invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Awaiting;

            wholesaleOrderInfo.Awaiting++;
            wholesaleOrderInfo.ProcessingAwaiting--;
        }

        await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);
        await _nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

        var selectedMappingForOpenEndedProductStr = await _genericAttributeService.GetAttributeAsync<string>(
            invoiceItem, $"SelectedMappingForOpenEndedProduct-{invoiceItem}", order.StoreId);

        if (selectedMappingForOpenEndedProductStr != null)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem, $"SelectedMappingForOpenEndedProduct-{invoiceItem.Id}",
                null, order.StoreId);
        }

        // Reset stored attributes
        await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
            $"redeeming-user-email-for-invoice-{invoiceItem.Id}", null, order.StoreId);
        await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
            $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", null, order.StoreId);
        await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
            $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", null, order.StoreId);

        //await _nexportService.AddOrderNoteAsync(order,
        //    $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be automatically redeemed for user {queueItem.RedeemingUserId}. " +
        //    "However, this invoice item can still be manually redeem by the user in the order history page.");

        await _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);
    }

    private async Task RetailDeleteRedemptionQueueItemAndAddFinalOrderNote(Order order,
        NexportOrderInvoiceRedemptionQueueItem queueItem, NexportOrderInvoiceItem invoiceItem)
    {
        await _nexportService.AddOrderNoteAsync(order,
            $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be automatically redeemed for user {queueItem.RedeemingUserId}. " +
            "However, this invoice item can still be manually redeem by the user in the order history page.");

        await _nexportService.DeleteNexportOrderInvoiceRedemptionQueueItem(queueItem);
    }

    private async Task CleanUpOrderItemAttributesAsync(int orderItemId)
    {
        var cleanUpAttributes = await _genericAttributeService.GetAttributesForEntityAsync(orderItemId, "OrderItem");
        await _genericAttributeService.DeleteAttributesAsync(cleanUpAttributes);
    }
}