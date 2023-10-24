using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Order;

public class OrderDetailsProductLine : NopViewComponent
{
    private readonly IOrderService _order;

    public OrderDetailsProductLine(IOrderService orderService) => _order = orderService;

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (additionalData is not OrderDetailsModel.OrderItemModel model
            || await _order.GetOrderItemByGuidAsync(model.OrderItemGuid) is not { OrderId: var orderId }
            || await _order.GetOrderByIdAsync(orderId) is not { OrderStatus: OrderStatus.Complete })
        {
            return Content(string.Empty);
        }
        return View(model);
    }
}
