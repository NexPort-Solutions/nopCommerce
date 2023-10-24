using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Order;

public class OrderDetails : NopViewComponent
{
    private readonly Settings _settings;
    private readonly IOrderService _order;
    private readonly IPermissionService _permission;

    public OrderDetails(
        Settings settings,
        IOrderService orderService,
        IPermissionService permissionService)
    {
        _settings = settings;
        _order = orderService;
        _permission = permissionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageOrderInvoice)
            || additionalData is not OrderModel orderModel
            || await _order.GetOrderByIdAsync(orderModel.Id) is not { } order)
        {
            return Content(string.Empty);
        }
        var model = new AdditionalInfoModel
        {
            Id = order.Id,
            ApprovalModel = new() { SearchModel = new() { Id = order.Id } },
        };
        return View(model);
    }
}
