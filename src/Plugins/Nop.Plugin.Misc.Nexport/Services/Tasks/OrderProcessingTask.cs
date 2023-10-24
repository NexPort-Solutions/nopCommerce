using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using ILogger = Nop.Services.Logging.ILogger;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using State = NexportApi.Model.GetInvoiceResponse.StateEnum;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class OrderProcessingTask : IScheduleTask
{
    private readonly LocalizationSettings _localizationSettings;
    private readonly Settings _settings;
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IRepository<OrderProcessingQueueItem> _orderProcessingQueues;
    private readonly INexportService _nexport;
    private readonly IInvoiceService _invoice;
    private readonly IProductMappingService _productMapping;
    private readonly IAddressService _address;
    private readonly IProductService _product;
    private readonly IProductGroupMembershipService _productGroupMembership;
    private readonly IOrderProcessingQueueItemService _orderProcessingQueueItem;
    private readonly IOrderService _order;
    private readonly IOrderProcessingService _orderProcessing;
    private readonly IStoreService _store;
    private readonly ICustomerService _customer;
    private readonly IStateProvinceService _stateProvince;
    private readonly ICountryService _country;
    private readonly ISettingService _setting;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IWorkflowMessagingService _workflowMessaging;
    private readonly IUserService _user;
    private readonly IUserMappingService _userMapping;
    private int _batchSize;

    private record struct AutoRedeemingInvoiceItem(int Id, int ProductMappingId, int OrderItemId, int? ExtensionAction);

    public OrderProcessingTask(
        LocalizationSettings localizationSettings,
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        IAddressService address,
        IProductService product,
        IOrderService order,
        IOrderProcessingService orderProcessing,
        IStoreService store,
        ICustomerService customer,
        IStateProvinceService stateProvince,
        ICountryService country,
        ISettingService setting,
        IGenericAttributeService genericAttribute,
        IRepository<OrderProcessingQueueItem> orderProcessingQueueRepository,
        INexportService nexport,
        Settings settings,
        IInvoiceService invoice,
        IOrderProcessingQueueItemService orderProcessingQueueItem,
        IProductMappingService productMapping,
        IWorkflowMessagingService workflowMessaging,
        IUserService user,
        IProductGroupMembershipService productGroupMembership,
        IUserMappingService userMapping)
    {
        _localizationSettings = localizationSettings;

        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
        _address = address;
        _product = product;
        _order = order;
        _orderProcessing = orderProcessing;
        _store = store;
        _customer = customer;
        _stateProvince = stateProvince;
        _country = country;
        _setting = setting;
        _genericAttribute = genericAttribute;
        _orderProcessingQueues = orderProcessingQueueRepository;
        _nexport = nexport;
        _settings = settings;
        _invoice = invoice;
        _orderProcessingQueueItem = orderProcessingQueueItem;
        _productMapping = productMapping;
        _workflowMessaging = workflowMessaging;
        _user = user;
        _productGroupMembership = productGroupMembership;
        _userMapping = userMapping;
    }

    public async Task ExecuteAsync()
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync(SystemNames.SYSTEM_NAME))
        {
            return;
        }
        try
        {
            _batchSize = await _setting.GetSettingByKeyAsync(Defaults.OrderProcessingTaskBatchSizeSettingKey, Defaults.OrderProcessingTaskBatchSize);
            var queueItems = _orderProcessingQueues.Table
                .OrderBy(queueItem => queueItem.UtcDateCreated)
                .Select(queueItem => queueItem.Id)
                .Take(_batchSize)
                .ToList();
            await ProcessQueueItems(queueItems);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot process NexPort redemption.", exception);
        }
    }

    private async Task ProcessQueueItems(IList<int> queueItemIds)
    {
        var queueItems = queueItemIds
            .SelectAwait(async queueItemId => await _orderProcessingQueues.GetByIdAsync(queueItemId))
            .WhereNotNull();
        await foreach (var queueItem in queueItems)
        {
            await _logger.InformationAsync($"Begin processing order processing queue item {queueItem.Id}");
            if (await _order.GetOrderByIdAsync(queueItem.OrderId) is { Deleted: false, OrderStatus: OrderStatus.Processing } order)
            {
                var complete = await ProcessOrderAsync(order, queueItem.Id);
                await _logger.InformationAsync($"Order {order.Id} has been successfully processed!");
                await _order.UpdateOrderAsync(order); // This is done for the order notes.
                if (complete)
                {
                    await _orderProcessing.CheckOrderStatusAsync(order);
                }
            }
            else
            {
                await _logger.WarningAsync($"Cannot find the order {queueItem.OrderId} or it is deleted or its order status is not set to processing.");
            }
            await _orderProcessingQueueItem.Delete(queueItem);
            await _logger.InformationAsync($"Order processing queue item {queueItem.Id} has been processed and removed!");
        }
    }

    private async Task<bool> ProcessOrderAsync(Order order, int queueItemId)
    {
        await _logger.InformationAsync($"Begin processing order {order.Id}");
        if (await TryGetUserMapping(order) is not { } userMapping
            || await TryGetStore(order) is not { } store
            || await TryGetOrganizationId(store, order) is not { } organizationId
            || await FindExistingOrCreateNewInvoice(order, userMapping, organizationId) is not { } orderInvoiceId
            || await _invoice.GetInvoice(orderInvoiceId) is not { State: not State.Failed } invoiceDetails)
        {
            return false;
        }
        // TODO if exists, use for wholesale order
        // var group = await _genericAttribute.GetAttributeAsync<string>(order, Defaults.GROUP_FOR_ORDER, store.Id);
        if (invoiceDetails.State is State.Committed)
        {
            await _logger.InformationAsync($"Order number {order.Id} invoice details state is committed. Complete the order.");
            return true;
        }
        return await HandleUncommittedInvoice(order, organizationId, invoiceDetails, userMapping, orderInvoiceId, queueItemId, store);
    }

    private async Task<bool> HandleUncommittedInvoice(Order order, Guid organizationId, GetInvoiceResponse invoiceDetails, UserMapping userMapping, Guid orderInvoiceId, int queueItemId, Store store)
    {
        var orderRequiresManualApproval = false;
        var invoiceTotalCost = 0.0M;
        var autoRedeemingInvoiceItems = new List<AutoRedeemingInvoiceItem>();
        var orderItems = await _order.GetOrderItemsAsync(order.Id);
        foreach (var orderItem in orderItems)
        {
            if (await _genericAttribute.GetAttributeAsync<string>(orderItem, $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId) is not { } mappingInfo
                || await GetMostRelevantMappingInfo(order, orderItem, mappingInfo) is not { } mapping
                || await _product.GetProductByIdAsync(orderItem.ProductId) is not { ProductCost: var productCost }
                || (await _invoice.FindExistingInvoiceItemForOrderItem(order.Id, orderItem.Id) is { } existingInvoiceItemId
                    && invoiceDetails.InvoiceItems.Any(invoiceItem => invoiceItem.Id == existingInvoiceItemId))) // Skip if the invoice item already exists
            {
                continue;
            }
            invoiceTotalCost += orderItem.Quantity * productCost;
            var groupMembershipIds = await GetGroupMembershipIds(order, orderItem, mapping);
            var result = await AddAllItemsToInvoice(groupMembershipIds, orderItem, mapping, userMapping, organizationId, orderInvoiceId, productCost);
            orderRequiresManualApproval |= result.OrderRequiresManualApproval;
            autoRedeemingInvoiceItems.AddRange(result.AutoRedeemingInvoiceItems);
        }
        await _invoice.AddPaymentToOrderInvoice(orderInvoiceId, invoiceTotalCost, userMapping.UserId, queueItemId, DateTime.UtcNow);
        if (await _invoice.CommitOrderInvoiceTransaction(orderInvoiceId) is { } commitResult)
        {
            await UpdateOrderInvoiceItemsByCode(commitResult);
        }
        if (autoRedeemingInvoiceItems.Count is not 0)
        {
            await ScheduleRedemptionTaskForItemsThatDoNotNeedAdminApproval(userMapping, autoRedeemingInvoiceItems);
        }
        if (!orderRequiresManualApproval)
        {
            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = $"NexPort invoice has been successfully processed. Order number: {order.Id}, Store: {store.Name}",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _order.InsertOrderNoteAsync(orderNote);
            return true;
        }
        await _workflowMessaging.SendNewOrderApprovalStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
        return false;
    }

    private async Task<(bool OrderRequiresManualApproval, List<AutoRedeemingInvoiceItem> AutoRedeemingInvoiceItems)> AddAllItemsToInvoice(
        IList<Guid> groupMembershipIds,
        OrderItem orderItem,
        ProductMapping mapping,
        UserMapping userMapping,
        Guid organizationId,
        Guid orderInvoiceId,
        decimal productCost)
    {
        var lazyData = Enumerable.Range(0, orderItem.Quantity)
            .SelectAwait(async _ => await AddItemToInvoiceAsync(mapping, userMapping, orderInvoiceId, productCost, mapping.SubscriptionOrgId ?? organizationId, groupMembershipIds))
            .Where(result => result.Id is not null)
            .SelectAwait(async result => await HandleItemQuantum(orderInvoiceId, orderItem, mapping, result.Id!.Value, result.ExtensionAction, result.RequireManualApproval));
        var orderRequiresManualApproval = await lazyData.AnyAsync(result => result.OrderRequiresManualApproval);
        var autoRedeemItems = await lazyData.Select(result => result.AutoRedeemingInvoiceItem).WhereNotNull().ToListAsync();
        return (orderRequiresManualApproval, autoRedeemItems);
    }

    private async Task<Guid?> TryGetOrganizationId(Store store, Order order)
    {
        if (await _genericAttribute.GetAttributeAsync<Guid?>(store, "SubscriptionOrganizationId", store.Id) is not { } orgId)
        {
            var message = $"While processing order {order.Id} - orgid set to root organization id value";
            await _logger.InformationAsync(message);
            return _settings.RootOrganizationId ?? throw new("Root org not set.");
        }
        return orgId;
    }

    private async Task<UserMapping?> TryGetUserMapping(Order order)
    {
        if (await _userMapping.FindByCustomerId(order.CustomerId) is not { } userMapping)
        {
            var message = $"User mapping for the customer {order.CustomerId} could not be found during the processing of NexPort invoice";
            await LogAndAddOrderNoteForErrorAsync(order, message);
            return null;
        }
        await SynchronizeCustomerContactInformationAsync(userMapping);
        return userMapping;
    }

    private async Task<Store?> TryGetStore(Order order)
    {
        if (await _store.GetStoreByIdAsync(order.StoreId) is not Store store)
        {
            var message = $"Store {order.StoreId} could not be found for this order during the processing of NexPort invoice";
            await LogAndAddOrderNoteForErrorAsync(order, message);
            return null;
        }
        var orderNote = new OrderNote
        {
            OrderId = order.Id,
            Note = $"Nexport invoice has started processing for order Number: {order.Id}, Store: {store.Name}",
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _order.InsertOrderNoteAsync(orderNote);
        return store;
    }

    /// <summary>
    /// Check if there is an existing invoice. If not, begin a new invoice transaction.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="userMapping"></param>
    /// <param name="orgId"></param>
    /// <returns>The id of the relevant invoice, or null if there was a misconfiguration.</returns>
    private async Task<Guid?> FindExistingOrCreateNewInvoice(Order order, UserMapping userMapping, Guid orgId)
        => await _invoice.FindExistingInvoiceForOrder(order.Id) // TODO If there is a group id then treat as wholesale order, otherwise do retail
            ?? await _invoice.BeginOrderInvoiceTransaction(orgId, userMapping.UserId);

    private async Task<(bool OrderRequiresManualApproval, AutoRedeemingInvoiceItem? AutoRedeemingInvoiceItem)> HandleItemQuantum(
        Guid orderInvoiceId,
        OrderItem orderItem,
        ProductMapping mapping,
        Guid detailsId,
        int? extensionAction,
        bool? itemRequiresManualApproval)
    {
        var orderInvoiceItem = new OrderInvoiceItem
        {
            OrderId = orderItem.OrderId,
            OrderItemId = orderItem.Id,
            InvoiceItemId = detailsId,
            InvoiceId = orderInvoiceId,
            UtcDateProcessed = DateTime.UtcNow,
            RequireManualApproval = itemRequiresManualApproval,
        };
        await _invoice.InsertOrUpdateOrderInvoiceItem(orderInvoiceItem);
        if (!mapping.AutoRedeem || itemRequiresManualApproval is true)
        {
            return (itemRequiresManualApproval is true, null);
        }
        return (itemRequiresManualApproval is true, new AutoRedeemingInvoiceItem
        {
            Id = orderInvoiceItem.Id,
            ProductMappingId = mapping.Id,
            OrderItemId = orderItem.Id,
            ExtensionAction = extensionAction,
        });
    }

    private async Task UpdateOrderInvoiceItemsByCode(CommitInvoiceResponse commitResult)
    {
        foreach (var (key, value) in commitResult.InvoiceItemRedemptionCodes)
        {
            if (await _invoice.FindOrderInvoiceItemByGuid(Guid.Parse(key)) is not { } invoiceItem)
            {
                var message = $"While updating invoice items, unable to find invoice item '{key}'.";
                await _logger.WarningAsync(message);
                continue;
            }
            invoiceItem.InvoiceRedemptionCode = commitResult.InvoiceRedemptionCode;
            invoiceItem.InvoiceItemRedemptionCode = value;
            await _invoice.UpdateOrderInvoiceItem(invoiceItem);
        }
    }

    private async Task ScheduleRedemptionTaskForItemsThatDoNotNeedAdminApproval(UserMapping userMapping, List<AutoRedeemingInvoiceItem> autoRedeemingInvoiceItems)
    {
        foreach (var redeemingInvoiceItem in autoRedeemingInvoiceItems)
        {
            var orderInvoiceRedemptionQueueItem = new OrderInvoiceRedemptionQueueItem
            {
                OrderInvoiceItemId = redeemingInvoiceItem.Id,
                RedeemingUserId = userMapping.UserId,
                ProductMappingId = redeemingInvoiceItem.ProductMappingId,
                OrderItemId = redeemingInvoiceItem.OrderItemId,
                UtcDateCreated = DateTime.UtcNow,
                ManualApprovalAction = redeemingInvoiceItem.ExtensionAction,
                RetryCount = 0,
            };
            await _invoice.InsertOrderInvoiceRedemptionQueueItem(orderInvoiceRedemptionQueueItem);
        }
    }

    /// <summary>
    /// Retrieve the stored mapping info if existed; otherwise, get the current mapping info
    /// </summary>
    /// <param name="order"></param>
    /// <param name="orderItem"></param>
    /// <param name="mappingInfo"></param>
    /// <returns></returns>
    private async Task<ProductMapping?> GetMostRelevantMappingInfo(Order order, OrderItem orderItem, string? mappingInfo)
    {
        if (mappingInfo is not null)
        {
            return JsonConvert.DeserializeObject<ProductMapping>(mappingInfo);
        }
        return await _productMapping.GetByNopProductId(orderItem.ProductId, order.StoreId)
            ?? await _productMapping.GetByNopProductId(orderItem.ProductId);
    }

    private async Task SynchronizeCustomerContactInformationAsync(UserMapping userMapping)
    {
        if (await _customer.GetCustomerByIdAsync(userMapping.NopUserId) is not Customer customer
            || await GetRecord.OrDefault(_address.GetAddressByIdAsync, customer.BillingAddressId) is not Address address)
        {
            return;
        }
        var customerStateProvince = await GetRecord.OrDefault(_stateProvince.GetStateProvinceByIdAsync, address.StateProvinceId);
        var customerCountry = await GetRecord.OrDefault(_country.GetCountryByIdAsync, address.CountryId);
        var updatedInfo = new UserContactInfoRequest(
            address.Address1,
            address.Address2,
            address.City,
            customerStateProvince?.Name,
            customerCountry?.Name,
            address.ZipPostalCode,
            address.PhoneNumber,
            null,
            address.FaxNumber,
            null,
            new ApiErrorEntity());
        if (await _user.UpdateUserContactInfo(userMapping.UserId, updatedInfo) is { UserId: var userId })
        {
            await _logger.InformationAsync($"Successfully update contact information in NexPort for customer {userId}");
        }
    }

    /// <summary>
    /// Generate the list of group membership identifiers
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="orderItem">The order item</param>
    /// <param name="productMapping">The NexPort product mapping</param>
    /// <returns>The list of group membership identifiers from the NexPort product mapping of the particular product.</returns>
    private async Task<IList<Guid>> GetGroupMembershipIds(Order order, OrderItem orderItem, ProductMapping productMapping)
    {
        var keyGroup = $"ProductGroupMembershipMapping-{order.Id}-{orderItem.Id}-{productMapping.Id}";
        var groupMembershipMappingInfo = (await _genericAttribute
            .GetAttributesForEntityAsync(orderItem.Id, keyGroup))
            .Where<GenericAttribute>(attribute => attribute.StoreId == order.StoreId);
        // TODO Why is this info only used if the generic attributes are empty?
        if (!groupMembershipMappingInfo.Any())
        {
            return await _productGroupMembership.GetProductGroupMembershipIds(productMapping.Id);
        }
        return await groupMembershipMappingInfo
            .Select(attribute => attribute.Value)
            .Select(JsonConvert.DeserializeObject<ProductGroupMembershipMapping>)
            .WhereNotNull()
            .Select(mappingInfo => mappingInfo.GroupId)
            .ToListAsync();
    }

    /// <summary>
    /// Add the product to the invoice
    /// </summary>
    /// <param name="productMapping">The NexPort product mapping entity</param>
    /// <param name="userMapping">The NexPort user mapping entity</param>
    /// <param name="orderInvoiceId">The NexPort invoice Id</param>
    /// <param name="productCost">The actual cost of the product</param>
    /// <param name="subscriptionOrgId">The NexPort subscription organization Id</param>
    /// <param name="groupMembershipIds">The list of group membership Ids</param>
    /// <returns>The NexPort invoice item Id</returns>
    private async Task<AddResultDetails> AddItemToInvoiceAsync(
        ProductMapping productMapping,
        UserMapping userMapping,
        Guid orderInvoiceId,
        decimal productCost,
        Guid subscriptionOrgId,
        IList<Guid> groupMembershipIds)
    {
        if (productMapping.CatalogSyllabusLinkId is not Guid syllabusId)
        {
            return (null, null, null, null);
        }
        var status = await _nexport.VerifyEnrollmentStatusAsync(productMapping, userMapping);
        var completionPercentage = status?.CompletionPercentage;
        return status switch
        {
            // Applicable for enrollment that is needed to be renewed or restarted
            { Phase: Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress }
                when productMapping.RenewalCompletionThreshold is not null
                    && completionPercentage < productMapping.RenewalCompletionThreshold
                => await renewIncomplete(completionPercentage),
            { Phase: Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress }
                when productMapping.RenewalCompletionThreshold is not null
                => await renewComplete(completionPercentage),
            { Phase: Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress }
                when productMapping.RenewalCompletionThreshold is not null
                => await restart(productMapping, syllabusId, completionPercentage, orderInvoiceId, productCost, subscriptionOrgId, groupMembershipIds),
            // Applicable for new enrollment or enrollment that has been completed (passed or failed)
            { Phase: Enums.PhaseEnum.Finished, Result: Enums.ResultEnum.Failing or Enums.ResultEnum.Passing }
                when productMapping.Type is ProductType.Catalog
                => await catalog(productMapping, orderInvoiceId, productCost, subscriptionOrgId, groupMembershipIds),
            { Phase: Enums.PhaseEnum.Finished, Result: Enums.ResultEnum.Failing or Enums.ResultEnum.Passing }
                => await other(productMapping, syllabusId, orderInvoiceId, productCost, subscriptionOrgId, groupMembershipIds),
            _ => (null, completionPercentage, null, null)
        };

        async ValueTask<AddResultDetails> renewIncomplete(int? completionPercentage)
        {
            // Use either access expiration date or access time limit when restarting enrollments that below the completion threshold
            var invoiceItemId = await _invoice.AddItemToOrderInvoice(
                orderInvoiceId,
                productMapping.CatalogSyllabusLinkId.Value,
                Enums.ProductTypeEnum.Syllabus,
                productCost,
                subscriptionOrgId,
                groupMembershipIds,
                productMapping.UtcAccessExpirationDate,
                productMapping.AccessTimeLimit);
            return (invoiceItemId, completionPercentage, null, 2);
        }

        async ValueTask<AddResultDetails> renewComplete(int? completionPercentage)
        {
            var invoiceItemId = await _invoice.AddItemToOrderInvoice(
                orderInvoiceId,
                productMapping.CatalogSyllabusLinkId.Value,
                Enums.ProductTypeEnum.Syllabus,
                productCost,
                subscriptionOrgId,
                groupMembershipIds,
                productMapping.UtcAccessExpirationDate,
                productMapping.RenewalDuration);
            var requireManualApproval = productMapping.RenewalApprovalMethod is EnrollmentRenewalApprovalMethod.Manual;
            return (invoiceItemId, completionPercentage, requireManualApproval, null);
        }

        async ValueTask<AddResultDetails> restart(
            ProductMapping productMapping,
            Guid syllabusId,
            int? completionPercentage,
            Guid orderInvoiceId,
            decimal productCost,
            Guid subscriptionOrgId,
            IList<Guid> groupMembershipIds)
        {
            var newAccessTimeLimit = !string.IsNullOrEmpty(productMapping.AccessTimeLimit)
                ? productMapping.AccessTimeLimit
                : productMapping.RenewalDuration;
            var invoiceItemId = await _invoice.AddItemToOrderInvoice(
                orderInvoiceId,
                syllabusId,
                Enums.ProductTypeEnum.Syllabus,
                productCost,
                subscriptionOrgId,
                groupMembershipIds,
                productMapping.UtcAccessExpirationDate,
                newAccessTimeLimit);
            return (invoiceItemId, completionPercentage, null, null);
        }

        async ValueTask<AddResultDetails> catalog(ProductMapping productMapping, Guid orderInvoiceId, decimal productCost, Guid subscriptionOrgId, IList<Guid> groupMembershipIds)
        {
            var invoiceItemId = await _invoice.AddItemToOrderInvoice(
                orderInvoiceId,
                productMapping.CatalogId ?? throw new($"{nameof(productMapping.CatalogId)} missing"),
                Enums.ProductTypeEnum.Catalog,
                productCost,
                subscriptionOrgId,
                groupMembershipIds,
                productMapping.UtcAccessExpirationDate,
                productMapping.AccessTimeLimit);
            return (invoiceItemId, CompletionPercentage: completionPercentage, null, null);
        }

        async ValueTask<AddResultDetails> other(
            ProductMapping productMapping,
            Guid syllabusId,
            Guid orderInvoiceId,
            decimal productCost,
            Guid subscriptionOrgId,
            IList<Guid> groupMembershipIds)
        {
            var id = await _invoice.AddItemToOrderInvoice(
                orderInvoiceId,
                syllabusId,
                Enums.ProductTypeEnum.Syllabus,
                productCost,
                subscriptionOrgId,
                groupMembershipIds,
                productMapping.UtcAccessExpirationDate,
                productMapping.AccessTimeLimit);
            return (id, CompletionPercentage: completionPercentage, null, null);
        }
    }

    private async Task LogAndAddOrderNoteForErrorAsync(Order order, string message, Exception? exception = null)
    {
        exception ??= new(message);
        await _logger.ErrorAsync(message, exception);
        var orderNote = new OrderNote
        {
            OrderId = order.Id,
            Note = message,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _order.InsertOrderNoteAsync(orderNote);
    }
}

internal record struct AddResultDetails(Guid? Id, int? CompletionPercentage, bool? RequireManualApproval, int? ExtensionAction)
{
    public static implicit operator (Guid? Id, int? CompletionPercentage, bool? RequireManualApproval, int? extensionAction)(AddResultDetails value)
        => (value.Id, value.CompletionPercentage, value.RequireManualApproval, value.ExtensionAction);

    public static implicit operator AddResultDetails((Guid? Id, int? CompletionPercentage, bool? RequireManualApproval, int? ExtensionAction) value)
        => new(value.Id, value.CompletionPercentage, value.RequireManualApproval, value.ExtensionAction);
}
