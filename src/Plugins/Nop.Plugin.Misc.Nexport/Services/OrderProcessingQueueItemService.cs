using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IOrderProcessingQueueItemService
{
    Task<bool> HasOrderProcessingQueueItem(int orderId);
    Task Delete(OrderProcessingQueueItem queueItem);
    Task Insert(OrderProcessingQueueItem queueItem);
}

public class OrderProcessingQueueItemService : IOrderProcessingQueueItemService
{
    private readonly IRepository<OrderProcessingQueueItem> _orderProcessingQueues;
    private readonly ILogger _logger;

    public OrderProcessingQueueItemService(ILogger logger, IRepository<OrderProcessingQueueItem> orderProcessingQueues)
    {
        _logger = logger;
        _orderProcessingQueues = orderProcessingQueues;
    }

    public async Task Insert(OrderProcessingQueueItem queueItem)
    {
        if (await _orderProcessingQueues.Table.AnyAsync(queueItem => queueItem.OrderId == queueItem.OrderId))
        {
            return;
        }
        await _logger.InformationAsync($"Order {queueItem.OrderId} has been added to the processing queue and awaiting to be processed.");
        await _orderProcessingQueues.InsertAsync(queueItem);
    }

    public async Task Delete(OrderProcessingQueueItem queueItem) => await _orderProcessingQueues.DeleteAsync(queueItem);
    public Task<bool> HasOrderProcessingQueueItem(int orderId) => _orderProcessingQueues.Table.AnyAsync(queueItem => queueItem.OrderId == orderId);
}
