using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using ActionType = NexportApi.Model.RedeemInvoiceItemRequest.RedemptionActionTypeEnum;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class InvoiceRedemptionTask : IScheduleTask
{
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IRepository<OrderInvoiceRedemptionQueueItem> _orderInvoiceRedemptionQueues;
    private readonly IRegistrationFieldService _registrationField;
    private readonly INexportService _nexport;
    private readonly IInvoiceService _invoice;
    private readonly IProductMappingService _productMapping;
    private readonly IUserMappingService _userMapping;
    private readonly IOrderService _order;
    private readonly IOrderProcessingService _orderProcessing;
    private readonly ISettingService _setting;
    private readonly IStoreService _store;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly ISupplementalInfoService _supplementalInfo;
    private readonly IUserService _user;
    private int _batchSize;

    private const int MAX_RETRY_COUNT = 5;

    public InvoiceRedemptionTask(
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        IOrderService order,
        IOrderProcessingService orderProcessing,
        ISettingService setting,
        IStoreService store,
        IGenericAttributeService genericAttribute,
        IRepository<OrderInvoiceRedemptionQueueItem> orderInvoiceRedemptionQueues,
        INexportService nexport,
        IProductMappingService productMapping,
        IUserMappingService userMapping,
        IInvoiceService invoice,
        IRegistrationFieldService registrationField,
        ISupplementalInfoService supplementalInfo,
        IUserService user)
    {
        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
        _order = order;
        _orderProcessing = orderProcessing;
        _setting = setting;
        _store = store;
        _genericAttribute = genericAttribute;
        _orderInvoiceRedemptionQueues = orderInvoiceRedemptionQueues;
        _nexport = nexport;
        _productMapping = productMapping;
        _userMapping = userMapping;
        _invoice = invoice;
        _registrationField = registrationField;
        _supplementalInfo = supplementalInfo;
        _user = user;
    }

    public async Task ExecuteAsync()
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync(SystemNames.SYSTEM_NAME))
        {
            return;
        }
        try
        {
            _batchSize = await _setting.GetSettingByKeyAsync(
                Defaults.OrderInvoiceRedemptionTaskBatchSizeSettingKey,
                Defaults.OrderInvoiceRedemptionTaskBatchSize);
            var queueItems = _orderInvoiceRedemptionQueues
                .Table
                .OrderBy(queueItem => queueItem.UtcLastFailedDate)
                .ThenBy(queueItem => queueItem.UtcDateCreated)
                .Select(queueItem => queueItem.Id)
                .Take(_batchSize)
                .ToList();
            await ProcessOrderInvoiceRedemptionsAsync(queueItems);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot process NexPort order invoice redemption", exception);
        }
    }

    public async Task ProcessOrderInvoiceRedemptionsAsync(IList<int> queueItemIds)
    {
        foreach (var queueItemId in queueItemIds)
        {
            if (await _orderInvoiceRedemptionQueues.GetByIdAsync(queueItemId) is not OrderInvoiceRedemptionQueueItem queueItem
                || await _invoice.FindOrderInvoiceItemById(queueItem.OrderInvoiceItemId) is not OrderInvoiceItem invoiceItem
                || await _order.GetOrderByIdAsync(invoiceItem.OrderId) is not Order order
                || await _order.GetOrderItemByIdAsync(queueItem.OrderItemId) is not OrderItem orderItem)
            {
                continue;
            }
            await _logger.DebugAsync($"Begin processing order invoice redemption for user {queueItem.RedeemingUserId} with invoice item {queueItem.OrderInvoiceItemId}");
            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = $"Nexport invoice item {invoiceItem.InvoiceItemId}, order number {order.Id} has started the redemption process",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _order.InsertOrderNoteAsync(orderNote);
            if (queueItem.RetryCount > MAX_RETRY_COUNT)
            {
                await DeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
                await CleanUpStoredMappingInfoAsync(queueItem.OrderItemId);
                await _orderProcessing.CheckOrderStatusAsync(order);
            }
            var store = await _store.GetStoreByIdAsync(order.StoreId);
            var storeModelInfo = await _genericAttribute.GetAttributeAsync<string>(orderItem, $"StoreModel-{order.Id}-{orderItem.Id}", order.StoreId);
            if (await IsRetailStore(store, storeModelInfo))
            {
                if (await GetMappingInfo(queueItem, order, orderItem) is not (var product, var user))
                {
                    continue;
                }
                if (await _invoice.GetInvoice(invoiceItem.InvoiceId) is { State: GetInvoiceResponse.StateEnum.Committed })
                {
                    await ProcessOpenInvoice(queueItemId, queueItem, invoiceItem, order, orderItem, store, product, user);
                }
            }
            await HandleRedeemInvoiceItemFailure(queueItem, invoiceItem, order);
        }
    }

    private async Task<bool> IsRetailStore(Store store, string storeModelInfo)
        => (Enum.TryParse<StoreSaleModel>(storeModelInfo, out var result) && result is StoreSaleModel.Retail)
            || await _genericAttribute.GetAttributeAsync<StoreSaleModel>(store, nameof(StoreSaleModel), store.Id) is StoreSaleModel.Retail;

    private async Task<(ProductMapping Product, UserMapping User)?> GetMappingInfo(OrderInvoiceRedemptionQueueItem queueItem, Order order, OrderItem orderItem)
    {
        var mappingInfo = await _genericAttribute.GetAttributeAsync<string>(orderItem, $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);
        var productMapping = mappingInfo is not null
            ? JsonConvert.DeserializeObject<ProductMapping>(mappingInfo)
            : await _productMapping.GetById(queueItem.ProductMappingId);
        var userMapping = await _userMapping.FindUserMappingByUserId(queueItem.RedeemingUserId);
        return productMapping is null || userMapping is null ? null : (productMapping, userMapping);
    }

    private async Task RedeemInvoiceAsync(
        ProductMapping productMapping,
        UserMapping userMapping,
        OrderInvoiceItem invoiceItem,
        Guid redeemingUserId,
        ActionType extensionAction = ActionType.DeleteFinishedEnrollment)
    {
        var existingEnrollmentStatus = await _nexport.VerifyEnrollmentStatusAsync(productMapping, userMapping);
        var redeemTask = (existingEnrollmentStatus.Value.Phase, existingEnrollmentStatus.Value.Result, existingEnrollmentStatus.Value.EnrollmentExpirationDate) switch
        {
            (Enums.PhaseEnum.Finished, Enums.ResultEnum.Failing, _) or
            (Enums.PhaseEnum.Finished, Enums.ResultEnum.Passing, _) =>
                    _invoice.RedeemInvoiceItemAsync(invoiceItem, redeemingUserId, ActionType.DeleteFinishedEnrollment),
            (Enums.PhaseEnum.InProgress or Enums.PhaseEnum.NotStarted, _, DateTime expiration) when expiration >= DateTime.UtcNow =>
                    _invoice.RedeemInvoiceItemAsync(invoiceItem, redeemingUserId, ActionType.RenewRedemption),
            (Enums.PhaseEnum.InProgress or Enums.PhaseEnum.NotStarted, _, _) when productMapping.AllowExtension =>
                    InProgressOrNotStartedAndAllowExtension(productMapping, invoiceItem, redeemingUserId, extensionAction, existingEnrollmentStatus.Value.CompletionPercentage),
            _ => _invoice.RedeemInvoiceItemAsync(invoiceItem, redeemingUserId),
        };
        await redeemTask;
    }

    private async Task DeleteRedemptionQueueItemAndAddFinalOrderNote(
        Order order,
        OrderInvoiceRedemptionQueueItem queueItem,
        OrderInvoiceItem invoiceItem)
    {
        var note = $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be automatically redeemed for user {queueItem.RedeemingUserId}. "
            + "However, this invoice item can still be manually redeem by the user in the order history page.";
        var orderNote = new OrderNote
        {
            OrderId = order.Id,
            Note = note,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _order.InsertOrderNoteAsync(orderNote);
        await _invoice.DeleteOrderInvoiceRedemptionQueueItem(queueItem);
    }

    private async Task CleanUpStoredMappingInfoAsync(int orderItemId)
    {
        var cleanUpAttributes = await _genericAttribute.GetAttributesForEntityAsync(orderItemId, "OrderItem");
        await _genericAttribute.DeleteAttributesAsync(cleanUpAttributes);
    }

    private async Task ProcessOpenInvoice(
        int queueItemId,
        OrderInvoiceRedemptionQueueItem queueItem,
        OrderInvoiceItem invoiceItem,
        Order order,
        OrderItem orderItem,
        Store store,
        ProductMapping productMapping,
        UserMapping userMapping)
    {
        var extensionAction = queueItem.ManualApprovalAction switch
        {
            1 => ActionType.RenewRedemption,
            2 => ActionType.RestartEnrollment,
            _ => ActionType.DeleteFinishedEnrollment
        };
        await RedeemInvoiceAsync(productMapping, userMapping, invoiceItem, queueItem.RedeemingUserId, extensionAction);
        var note = $"Redeemed NexPort invoice item {invoiceItem.InvoiceItemId}, "
            + $"Store: {store.Name}, "
            + $"Product: {productMapping.ProductName}, "
            + $"ID: {queueItem.RedeemingUserId}, ";
        if (await _user.GetUser(queueItem.RedeemingUserId) is { } user)
        {
            note += $"First name: {user.FirstName}, "
                + $"Last name: {user.LastName}";
        }
        var orderNote = new OrderNote
        {
            OrderId = order.Id,
            Note = note,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _order.InsertOrderNoteAsync(orderNote);
        var questionIds = (await _supplementalInfo.GetSupplementalInfoQuestionMappings(productMapping.Id)).ConvertAll(questionMapping => questionMapping.QuestionId);
        var requirements = (await _supplementalInfo.GetUnansweredQuestions(userMapping.NopUserId, store.Id, questionIds))
            .Select(questionId => new RequiredSupplementalInfo
            {
                CustomerId = userMapping.NopUserId,
                StoreId = store.Id,
                QuestionId = questionId,
                UtcDateCreated = DateTime.UtcNow,
            });
        foreach (var requirement in requirements)
        {
            await _supplementalInfo.InsertRequiredSupplementalInfo(requirement);
        }
        var registrationFieldSynchronizationQueueItem = new SynchronizationQueueItem
        {
            CustomerId = userMapping.NopUserId,
            UtcDateCreated = DateTime.UtcNow,
        };
        await _registrationField.InsertSynchronizationQueueItem(registrationFieldSynchronizationQueueItem);
        await _orderProcessing.CheckOrderStatusAsync(order);
        await _invoice.DeleteOrderInvoiceRedemptionQueueItem(queueItem);
        await _logger.InformationAsync($"Order invoice redemption queue item {queueItemId} for order {order.Id} has been processed and removed!");
        await CleanUpStoredMappingInfoAsync(orderItem.Id);
    }

    private async Task HandleRedeemInvoiceItemFailure(OrderInvoiceRedemptionQueueItem queueItem, OrderInvoiceItem invoiceItem, Order order)
    {
        await _logger.ErrorAsync($"Failed to redeem NexPort invoice item {invoiceItem.InvoiceItemId} for user {queueItem.RedeemingUserId}");
        queueItem.RetryCount++;
        if (queueItem.RetryCount <= MAX_RETRY_COUNT)
        {
            var note = $"Nexport invoice item {invoiceItem.InvoiceItemId} cannot be redeemed for user {queueItem.RedeemingUserId} "
                + $"and will be retry again for {MAX_RETRY_COUNT - queueItem.RetryCount} time(s)";
            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _order.InsertOrderNoteAsync(orderNote);
            await _invoice.UpdateOrderInvoiceRedemptionQueueItem(queueItem);
        }
        else
        {
            await DeleteRedemptionQueueItemAndAddFinalOrderNote(order, queueItem, invoiceItem);
            await CleanUpStoredMappingInfoAsync(queueItem.OrderItemId);
            await _orderProcessing.CheckOrderStatusAsync(order);
        }
    }

    private async Task InProgressOrNotStartedAndAllowExtension(
        ProductMapping productMapping,
        OrderInvoiceItem invoiceItem,
        Guid redeemingUserId,
        ActionType extensionAction,
        int completionPercentage)
    {
        if (productMapping.RenewalApprovalMethod is not EnrollmentRenewalApprovalMethod.Auto)
        {
            await _invoice.RedeemInvoiceItemAsync(invoiceItem, redeemingUserId, extensionAction);
        }

        // Renew or delete depends on the threshold setting if the product type is section
        if (productMapping.Type is ProductType.Section && productMapping.RenewalCompletionThreshold is not null)
        {
            var redemptionAction = completionPercentage >= productMapping.RenewalCompletionThreshold
                ? ActionType.RestartEnrollment : ActionType.RenewRedemption;
            await _invoice.RedeemInvoiceItemAsync(invoiceItem, redeemingUserId, redemptionAction);
        }

        // Only renew the enrollment for training plan and catalog
        await _invoice.RedeemInvoiceItemAsync(invoiceItem, redeemingUserId, ActionType.RenewRedemption);
    }
}
