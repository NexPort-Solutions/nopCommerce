using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class NexportOrderController : BaseAdminController
{
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IOrderModelFactory _orderModelFactory;
    private readonly IPermissionService _permissionService;

    public NexportOrderController(INexportPluginModelFactory nexportPluginModelFactory, IPermissionService permissionService, IOrderModelFactory orderModelFactory)
    {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _permissionService = permissionService;
            _orderModelFactory = orderModelFactory;
        }

    [HttpPost]
    public async Task<IActionResult> OrderList(OrderSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
            return await AccessDeniedJsonAsync();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareOrderListModelAsync(searchModel);

        return Json(model);
    }
}
