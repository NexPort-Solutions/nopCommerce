using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers
{
    public class NexportOrderController : BaseAdminController
    {
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IPermissionService _permissionService;

        public NexportOrderController(INexportPluginModelFactory nexportPluginModelFactory, IPermissionService permissionService, IOrderModelFactory orderModelFactory)
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _orderModelFactory = orderModelFactory;
            _permissionService = permissionService;
        }

        [HttpPost]
        public override async Task<IActionResult> OrderList(OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _nexportPluginModelFactory.PrepareOrderListModelAsync(searchModel);

            return Json(model);
        }
    }
}
