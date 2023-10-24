using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using static Nop.Plugin.Misc.Nexport.Defaults;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IGroupService
{
    IEnumerable<GenericAttribute> GetAllGroupForOrders();
    Task<int> GetAvailableGroupProductRedemptionsCount(Guid groupId, int productId);
    Task<GenericAttribute?> GetGroupByGroupId(Guid groupId);
    Task<List<GroupProductModel>> GetGroupProductModelForGroupId(Guid groupId);
    Task<List<Order>> GetOrdersForGroupId(Guid groupId);
    Task InsertGroupMembershipRemovalQueueItem(GroupMembershipRemovalQueueItem queueItem);
    Task DeleteGroupMembershipRemovalQueueItem(GroupMembershipRemovalQueueItem queueItem);
}

public class GroupService : IGroupService
{
    private readonly IRepository<GenericAttribute> _genericAttributes;
    private readonly IRepository<Product> _products;
    private readonly IRepository<OrderInvoiceItem> _orderInvoiceItems;
    private readonly IRepository<Order> _orders;
    private readonly IRepository<OrderItem> _orderItems;
    private readonly IRepository<GroupMembershipRemovalQueueItem> _groupMembershipRemovalQueues;

    public GroupService(
        IRepository<Order> orders,
        IRepository<GenericAttribute> genericAttributes,
        IRepository<Product> productRepository,
        IRepository<OrderInvoiceItem> orderInvoiceItemRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<GroupMembershipRemovalQueueItem> groupMembershipRemovalQueues)
    {
        _orders = orders;
        _genericAttributes = genericAttributes;
        _products = productRepository;
        _orderInvoiceItems = orderInvoiceItemRepository;
        _orderItems = orderItemRepository;
        _groupMembershipRemovalQueues = groupMembershipRemovalQueues;
    }

    public async Task InsertGroupMembershipRemovalQueueItem(GroupMembershipRemovalQueueItem queueItem)
        => await _groupMembershipRemovalQueues.InsertAsync(queueItem);

    public async Task DeleteGroupMembershipRemovalQueueItem(GroupMembershipRemovalQueueItem queueItem)
        => await _groupMembershipRemovalQueues.DeleteAsync(queueItem);

    public Task<List<Order>> GetOrdersForGroupId(Guid groupId)
        => _genericAttributes.Table.Where(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains($"\"GroupGuid\":\"{groupId}\""))
            .Join(_orders.Table, attribute => attribute.EntityId, order => order.Id, (_, order) => order)
            .ToListAsync();

    public Task<List<GroupProductModel>> GetGroupProductModelForGroupId(Guid groupId)
    {
        var query = _genericAttributes.Table
            .Where(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains("{\"GroupGuid\":\"" + groupId + "\""))
            .Join(_orderItems.Table, attribute => attribute.EntityId, item => item.OrderId, (_, item) => item)
            .Join(_products.Table, item => item.ProductId, product => product.Id, (item, product) => new { item, product })
            .Join(
                _orderInvoiceItems.Table,
                itemAndProduct => new { itemAndProduct.item.OrderId, ItemId = itemAndProduct.item.Id },
                invoiceItem => new { invoiceItem.OrderId, ItemId = invoiceItem.OrderItemId },
                (itemAndProduct, invoiceItem) => new { itemAndProduct, invoiceItem })
            .GroupBy(data => new { data.itemAndProduct.product.Id, data.itemAndProduct.product.Name })
            .Select(data =>
                new GroupProductModel
                {
                    Id = data.Key.Id,
                    Name = data.Key.Name,
                    Available = data.Count(data => data.invoiceItem.RedeemingUserId == null && data.invoiceItem.UtcDateRedemption == null),
                    Awaiting = data.Count(data => data.invoiceItem.RedeemingUserId != null && data.invoiceItem.UtcDateRedemption == null),
                    Redeemed = data.Count(data => data.invoiceItem.RedeemingUserId != null && data.invoiceItem.UtcDateRedemption != null),
                    GroupId = groupId,
                });
        return query.ToListAsync();
    }

    public IEnumerable<GenericAttribute> GetAllGroupForOrders()
    {
        return _genericAttributes.Table
            .Where(attribute => attribute.Key == GROUP_FOR_ORDER)
            .DistinctBy(attribute => attribute.Value)
            .WhereNotNull();
    }

    public Task<GenericAttribute?> GetGroupByGroupId(Guid groupId)
        => _genericAttributes.Table.FirstOrDefaultAsync(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains("{\"GroupGuid\":\"" + groupId + "\""))!;

    public Task<int> GetAvailableGroupProductRedemptionsCount(Guid groupId, int productId)
    {
        var query = _genericAttributes.Table
            .Where(attribute => attribute.Key == GROUP_FOR_ORDER && attribute.Value.Contains("{\"GroupGuid\":\"" + groupId + "\""))
            .Join(_orderItems.Table, attribute => attribute.EntityId, orderItem => orderItem.OrderId, (_, orderItem) => orderItem)
            .Join(_products.Table, orderItem => orderItem.ProductId, product => product.Id, (orderItem, product) => new { orderItem, product })
            .Where(orderItemAndProduct => orderItemAndProduct.product.Id == productId)
            .Join(
                _orderInvoiceItems.Table,
                orderItemAndProduct => new { order = orderItemAndProduct.orderItem.OrderId, item = orderItemAndProduct.orderItem.Id },
                invoiceItem => new { order = invoiceItem.OrderId, item = invoiceItem.OrderItemId },
                (_, invoiceItem) => invoiceItem)
            .Where(invoiceItem => invoiceItem.RedeemingUserId == null);
        return query.CountAsync();
    }
}
