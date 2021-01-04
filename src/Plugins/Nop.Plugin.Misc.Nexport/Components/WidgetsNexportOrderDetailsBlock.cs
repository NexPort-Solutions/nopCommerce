using Microsoft.AspNetCore.Mvc;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportOrderDetailsBlock")]
    public class WidgetsNexportOrderDetailsBlock : NopViewComponent
    {
        private readonly NexportSettings _nexportSettings;
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IGenericAttributeService _genericAttributeService;

        public WidgetsNexportOrderDetailsBlock(
            NexportSettings nexportSettings,
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory,
            IOrderService orderService,
            IPermissionService permissionService,
            IGenericAttributeService genericAttributeService)
        {
            _nexportSettings = nexportSettings;
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken) ||
                !_permissionService.Authorize(NexportPermissionProvider.ManageNexportOrderInvoice))
                return Content("");

            var orderModel = (OrderModel)additionalData;

            var order = _orderService.GetOrderById(orderModel.Id);

            if (order == null)
                return Content("");

            var model = new NexportOrderAdditionalInfoModel()
            {
                OrderId = order.Id,
                NexportOrderApprovalModel = new NexportOrderApprovalModel
                {
                    SearchModel = new NexportOrderInvoiceItemSearchModel
                    {
                        OrderId = order.Id
                    }
                }
            };

            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Order/NexportOrderDetails.cshtml", model);
        }
    }
}
