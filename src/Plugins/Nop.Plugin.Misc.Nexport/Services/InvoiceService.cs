using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Logging;
using static Nop.Plugin.Misc.Nexport.Defaults;
using RedemptionAction = NexportApi.Model.RedeemInvoiceItemRequest.RedemptionActionTypeEnum;
using ProductType = NexportApi.Model.Enums.ProductTypeEnum;
using PaymentProcessor = NexportApi.Model.InvoicePaymentRequest.PaymentProcessorEnum;
using System.Globalization;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IInvoiceService
{
    Task<Guid?> AddItemToOrderInvoice
        (Guid invoiceId, Guid productId, ProductType type, decimal cost, Guid subscription, IList<Guid>? memberships = null, DateTime? expiration = null, string? timeLimit = null, string? note = null);

    Task<AddInvoicePaymentResponse?> AddPaymentToOrderInvoice(Guid invoiceId, decimal totalCost, Guid payeeId, int nopOrderId, DateTime dueDate);
    Task<string?> SignInAsync(OrderInvoiceItem invoiceItem);
    Task RedeemInvoiceItemAsync(OrderInvoiceItem invoiceItem, Guid redeemingUserId, RedemptionAction redemptionAction = RedemptionAction.NormalRedemption);
    Task<CommitInvoiceResponse?> CommitOrderInvoiceTransaction(Guid invoiceId);
    Task<Guid?> BeginOrderInvoiceTransaction(Guid orgId, Guid purchasingAgentId);
    Task DeleteOrderInvoiceItem(OrderInvoiceItem item);
    Task DeleteOrderInvoiceRedemptionQueueItem(OrderInvoiceRedemptionQueueItem queueItem);
    Task<Guid?> FindExistingInvoiceForOrder(int orderId);
    Task<Guid?> FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId);
    Task<OrderInvoiceItem?> FindOrderInvoiceItem(int orderId, int orderItemId);
    Task<OrderInvoiceItem?> FindOrderInvoiceItemByGuid(Guid orderInvoiceItemId);
    Task<OrderInvoiceItem?> FindOrderInvoiceItemById(int orderInvoiceItemId);
    Task<OrderInvoiceItem?> FindOrderInvoiceItemByInvoiceItemGuid(Guid invoiceItemId);
    Task<List<OrderInvoiceItem>?> FindOrderInvoiceItems(int orderId, int orderItemId);
    Task<OrderInvoiceItem?> GetFirstAvailableInvoiceItemForGroupIdAndProductId(Guid groupId, int productId);
    Task<int> GetInvoiceItemCountForGroupByGuid(Guid groupId);
    Task<List<OrderInvoiceItem>> GetInvoiceItemsForGroupIdAndProductIdAndRedeemingUserIdHasValue(Guid groupId, int productId);
    Task<List<OrderInvoiceItem>> GetOrderInvoiceItems(Guid userId);
    Task<IPagedList<OrderInvoiceItem>> GetOrderInvoiceItems(int orderId, bool excludeNonApproval = false, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    Task<GetInvoiceResponse?> GetInvoice(Guid invoiceId);
    Task<InvoiceRedemptionResponse?> GetInvoiceRedemption(Guid invoiceItemId);
    Task UpdateOrderInvoiceItem(OrderInvoiceItem item);
    Task UpdateOrderInvoiceRedemptionQueueItem(OrderInvoiceRedemptionQueueItem queueItem);
    Task InsertOrderInvoiceRedemptionQueueItem(OrderInvoiceRedemptionQueueItem queueItem);
    Task InsertOrUpdateOrderInvoiceItem(OrderInvoiceItem item);
}

public class InvoiceService : IInvoiceService
{
    #region Fields

    private readonly NexportApiService _nexport;
    private readonly Settings _settings;
    private readonly IRepository<Product> _products;
    private readonly IRepository<OrderInvoiceItem> _orderInvoiceItems;
    private readonly IRepository<OrderInvoiceRedemptionQueueItem> _orderInvoiceRedemptionQueues;
    private readonly IRepository<GenericAttribute> _genericAttributes;
    private readonly IRepository<Order> _orders;
    private readonly IRepository<OrderItem> _orderItems;
    private readonly IStoreContext _storeContext;
    private readonly ILogger _logger;
    private readonly HelperService _helper;

    #endregion Fields

    #region Constructors

    public InvoiceService(
        NexportApiService apiService,
        Settings settings,
        IRepository<Product> productRepository,
        IRepository<OrderInvoiceItem> orderInvoiceItemRepository,
        IRepository<OrderInvoiceRedemptionQueueItem> orderInvoiceRedemptionQueueRepository,
        IRepository<GenericAttribute> genericAttributeRepository,
        IRepository<Order> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        IStoreContext storeContext,
        ILogger logger,
        HelperService helper)
    {
        _nexport = apiService;
        _settings = settings;
        _products = productRepository;
        _orderInvoiceItems = orderInvoiceItemRepository;
        _orderInvoiceRedemptionQueues = orderInvoiceRedemptionQueueRepository;
        _genericAttributes = genericAttributeRepository;
        _orders = orderRepository;
        _orderItems = orderItemRepository;
        _storeContext = storeContext;
        _logger = logger;
        _helper = helper;
    }

    #endregion Constructors

    public async Task<Guid?> AddItemToOrderInvoice
        (Guid invoiceId, Guid productId, ProductType type, decimal cost, Guid subscription, IList<Guid>? memberships = null, DateTime? expiration = null, string? timeLimit = null, string? note = null)
    {
        if (!_settings.IsValid())
        {
            return null;
        }
        var response = await _nexport.AddInvoiceItem(
            (_settings.Url,
            _settings.AuthenticationToken),
            invoiceId,
            productId,
            type,
            subscription,
            memberships ?? new List<Guid>(),
            cost,
            note,
            expiration,
            timeLimit);
        return response?.InvoiceItemId;
    }

    public async Task<Guid?> FindExistingInvoiceForOrder(int orderId)
        => (await _orderInvoiceItems.Table.FirstOrDefaultAsync(invoiceItem => invoiceItem.OrderId == orderId))?.InvoiceId;

    public async Task<Guid?> FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId)
        => (await _orderInvoiceItems.Table.FirstOrDefaultAsync(orderItem => orderItem.OrderId == orderId && orderItem.OrderItemId == orderItemId))?.InvoiceItemId;

    public Task<List<OrderInvoiceItem>> GetInvoiceItemsForGroupIdAndProductIdAndRedeemingUserIdHasValue(Guid groupId, int productId)
    {
        var query = _genericAttributes.Table
            .Where(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains("{\"GroupGuid\":\"" + groupId + "\""))
            .Join(_orderItems.Table, attribute => attribute.EntityId, orderItem => orderItem.OrderId, (_, orderItem) => orderItem)
            .Join(_products.Table, orderItem => orderItem.ProductId, product => product.Id, (orderItem, product) => new { orderItem, product })
            .Where(orderItemAndProduct => orderItemAndProduct.product.Id == productId)
            .Join(
                _orderInvoiceItems.Table,
                orderItemAndProduct => new { Order = orderItemAndProduct.orderItem.OrderId, OrderItem = orderItemAndProduct.orderItem.Id },
                invoiceItem => new { Order = invoiceItem.OrderId, OrderItem = invoiceItem.OrderItemId },
                (_, invoiceItem) => invoiceItem)
            .Where(invoiceItem => invoiceItem.RedeemingUserId != null);
        return query.ToListAsync();
    }

    public Task<OrderInvoiceItem?> GetFirstAvailableInvoiceItemForGroupIdAndProductId(Guid groupId, int productId)
    {
        var query = _genericAttributes.Table
            .Where(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains("{\"GroupGuid\":\"" + groupId + "\""))
            .Join(_orderItems.Table, attribute => attribute.EntityId, orderItem => orderItem.OrderId, (_, orderItem) => orderItem)
            .Join(_products.Table, orderItem => orderItem.ProductId, product => product.Id, (orderItem, product) => new { orderItem, product })
            .Where(orderItemAndProduct => orderItemAndProduct.product.Id == productId)
            .Join(
                _orderInvoiceItems.Table,
                orderItemAndProduct => new { Order = orderItemAndProduct.orderItem.OrderId, OrderItem = orderItemAndProduct.orderItem.Id },
                invoiceItem => new { Order = invoiceItem.OrderId, OrderItem = invoiceItem.OrderItemId },
                (_, noii) => noii)
            .Where(invoiceItem => invoiceItem.RedeemingUserId != null || invoiceItem.UtcDateRedemption != null);
        return query.FirstOrDefaultAsync();
    }

    public async Task InsertOrUpdateOrderInvoiceItem(OrderInvoiceItem item)
    {
        if (await _orderInvoiceItems.Table.AnyAsync(invoiceItem => invoiceItem.OrderId == item.OrderId
            && invoiceItem.OrderItemId == item.OrderItemId
            && invoiceItem.InvoiceItemId == item.InvoiceItemId)
            && await GetInvoiceToUpdate(item) is { } invoiceToUpdate)
        {
            // TODO @JS this should change when we have the reset redemption api call
            invoiceToUpdate.InvoiceItemId = item.InvoiceItemId;
            await _orderInvoiceItems.UpdateAsync(invoiceToUpdate);
        }
        await _orderInvoiceItems.InsertAsync(item);
    }

    private async Task<OrderInvoiceItem?> GetInvoiceToUpdate(OrderInvoiceItem item)
    {
        if (await FindOrderInvoiceItems(item.OrderId, item.OrderItemId) is not { } currentInvoiceItems)
        {
            return null;
        }
        foreach (var invoiceItem in currentInvoiceItems)
        {
            if (invoiceItem.UtcDateRedemption is not null)
            {
                return invoiceItem;
            }
        }
        return null;
    }

    public async Task InsertOrderInvoiceRedemptionQueueItem(OrderInvoiceRedemptionQueueItem queueItem)
    {
        if (_orderInvoiceRedemptionQueues.Table.Any(queueItem => queueItem.OrderInvoiceItemId == queueItem.OrderInvoiceItemId))
        {
            return;
        }
        await _orderInvoiceRedemptionQueues.InsertAsync(queueItem);
        await _logger.InformationAsync($"Invoice redemption {queueItem.OrderInvoiceItemId} for user {queueItem.RedeemingUserId} has been scheduled.");
    }

    public async Task DeleteOrderInvoiceItem(OrderInvoiceItem item) => await _orderInvoiceItems.DeleteAsync(item);
    public async Task UpdateOrderInvoiceItem(OrderInvoiceItem item) => await _orderInvoiceItems.UpdateAsync(item);
    public async Task DeleteOrderInvoiceRedemptionQueueItem(OrderInvoiceRedemptionQueueItem queueItem) => await _orderInvoiceRedemptionQueues.DeleteAsync(queueItem);
    public async Task UpdateOrderInvoiceRedemptionQueueItem(OrderInvoiceRedemptionQueueItem queueItem) => await _orderInvoiceRedemptionQueues.UpdateAsync(queueItem);

    public Task<OrderInvoiceItem?> FindOrderInvoiceItem(int orderId, int orderItemId)
        => _orderInvoiceItems.Table.SingleOrDefaultAsync(orderInvoiceItem => orderInvoiceItem.OrderId == orderId && orderInvoiceItem.OrderItemId == orderItemId)!;

    public Task<OrderInvoiceItem?> FindOrderInvoiceItemByInvoiceItemGuid(Guid invoiceItemId)
       => _orderInvoiceItems.Table.SingleOrDefaultAsync(invoiceItem => invoiceItem.InvoiceItemId == invoiceItemId)!;

    public Task<List<OrderInvoiceItem>?> FindOrderInvoiceItems(int orderId, int orderItemId)
       => _orderInvoiceItems.Table.Where(invoiceItem => invoiceItem.OrderId == orderId && invoiceItem.OrderItemId == orderItemId).ToListAsync();

    public Task<OrderInvoiceItem?> FindOrderInvoiceItemById(int orderInvoiceItemId)
        => _orderInvoiceItems.GetByIdAsync(orderInvoiceItemId)!; // GetByIdAsync can return null.

    public Task<List<OrderInvoiceItem>> GetOrderInvoiceItems(Guid userId)
       => _orderInvoiceItems.Table.Where(invoiceItem => invoiceItem.RedeemingUserId == userId).ToListAsync();

    public Task<IPagedList<OrderInvoiceItem>> GetOrderInvoiceItems(int orderId, bool excludeNonApproval = false, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var query = _orderInvoiceItems.Table.Where(invoiceItem => invoiceItem.OrderId == orderId);
        if (excludeNonApproval)
        {
            query = query.Where(invoiceItem =>
                invoiceItem.RequireManualApproval == true
                    && !_orderInvoiceRedemptionQueues.Table.Select(queueItem => queueItem.OrderItemId).Contains(invoiceItem.OrderItemId));
        }
        return query.ToPagedListAsync(pageIndex, pageSize);
    }

    public Task<int> GetInvoiceItemCountForGroupByGuid(Guid groupId)
        => _genericAttributes.Table.Where(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains($@"{{""GroupGuid"":""{groupId}"""))
            .Join(_orders.Table, attribute => attribute.EntityId, order => order.Id, (attribute, order) => new { attribute, order })
            .Join(_orderItems.Table, attributeAndOrder => attributeAndOrder.order.Id, orderItem => orderItem.OrderId, (_, orderItem) => orderItem)
            .Join(_orderInvoiceItems.Table, orderItem => orderItem.Id, invoiceItem => invoiceItem.OrderItemId, (_, invoiceItem) => invoiceItem)
            .CountAsync();

    public Task<OrderInvoiceItem?> FindOrderInvoiceItemByGuid(Guid orderInvoiceItemId)
       => _orderInvoiceItems.Table.SingleOrDefaultAsync(invoiceItem => invoiceItem.InvoiceItemId == orderInvoiceItemId)!;

    public Task<AddInvoicePaymentResponse?> AddPaymentToOrderInvoice(Guid invoiceId, decimal totalCost, Guid payeeId, int nopOrderId, DateTime dueDate) => _helper.Do(s
        => _nexport.AddInvoicePayment((s.Url, s.Token),
            invoiceId, totalCost, s.Merchant, payeeId, PaymentProcessor.NopCommercePlugin, nopOrderId.ToString(CultureInfo.InvariantCulture), dueDate, default, "Payment for NopCommerce order"));

    public async Task<string?> SignInAsync(OrderInvoiceItem invoiceItem)
    {
        if (!_settings.IsValid())
        {
            return null;
        }
        var redemption = await GetInvoiceRedemption(invoiceItem.InvoiceItemId);
        if (redemption?.ApiErrorEntity.ErrorCode is not ApiErrorEntity.ErrorCodeEnum.NoError)
        {
            return null;
        }
        var (url, token) = (_settings.Url, _settings.AuthenticationToken);
        if (invoiceItem.RedemptionEnrollmentId is null && redemption.RedemptionUserId is not null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var signInResult = await _nexport.SingleSignOn((url, token), redemption.OrganizationId, redemption.RedemptionUserId.Value, store.Url);
            if (signInResult?.ApiErrorEntity.ErrorCode is ApiErrorEntity.ErrorCodeEnum.NoError)
            {
                return signInResult.Url;
            }
        }
        else if (redemption.RedemptionEnrollmentId is not null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var signInResult = await _nexport.ClassroomSingleSignOn((url, token), redemption.RedemptionEnrollmentId.Value, store.Url);
            if (signInResult?.ApiErrorEntity.ErrorCode is ApiErrorEntity.ErrorCodeEnum.NoError)
            {
                return signInResult.Url;
            }
        }
        return null;
    }

    public async Task RedeemInvoiceItemAsync(OrderInvoiceItem invoiceItem, Guid redeemingUserId, RedemptionAction redemptionAction = RedemptionAction.NormalRedemption)
    {
        if (!_settings.IsValid())
        {
            return;
        }
        var (url, token) = (_settings.Url, _settings.AuthenticationToken);
        if (await _nexport.RedeemInvoice((url, token), redeemingUserId, redemptionAction, invoiceItem.InvoiceItemRedemptionCode) is not { } redeemInvoiceResult)
        {
            return;
        }
        invoiceItem.RedeemingUserId = redeemingUserId;
        invoiceItem.UtcDateRedemption = redeemInvoiceResult.UtcRedemptionDate;
        invoiceItem.RequireManualApproval = null;
        if (redeemInvoiceResult.RedemptionEnrollmentId is not null)
        {
            invoiceItem.RedemptionEnrollmentId = redeemInvoiceResult.RedemptionEnrollmentId;
        }
        await UpdateOrderInvoiceItem(invoiceItem);
    }

    public Task<CommitInvoiceResponse?> CommitOrderInvoiceTransaction(Guid invoiceId) => _helper.Do(s => _nexport.CommitInvoiceTransaction((s.Url, s.Token), invoiceId));
    public Task<GetInvoiceResponse?> GetInvoice(Guid invoiceId) => _helper.Do(s => _nexport.GetInvoice((s.Url, s.Token), invoiceId));

    public async Task<Guid?> BeginOrderInvoiceTransaction(Guid orgId, Guid purchasingAgentId)
        => (await _helper.Do(s => _nexport.BeginInvoiceTransaction((s.Url, s.Token), orgId, purchasingAgentId)))?.InvoiceId;

    public Task<InvoiceRedemptionResponse?> GetInvoiceRedemption(Guid invoiceItemId) => _helper.Do(s => _nexport.GetInvoiceRedemption((s.Url, s.Token), invoiceItemId));
}
