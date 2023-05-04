using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers
{
    public class NexportOrderController : BasePluginController
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


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> WholesaleOrder()
        {
            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Order/WholesaleOrder.cshtml");
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> List(List<int> orderStatuses = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            {
                return AccessDeniedView();
            }
            var model = await _orderModelFactory.PrepareOrderSearchModelAsync(new OrderSearchModel
            {
                OrderStatusIds = orderStatuses,
                PaymentStatusIds = paymentStatuses,
                ShippingStatusIds = shippingStatuses
            });
            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Order/List.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> OrderList(OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _nexportPluginModelFactory.PrepareOrderListModelAsync(searchModel);

            return Json(model);
        }
    }
}
