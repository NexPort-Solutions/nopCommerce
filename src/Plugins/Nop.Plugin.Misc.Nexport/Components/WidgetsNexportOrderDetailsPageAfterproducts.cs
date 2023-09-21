using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsOrderDetailsPageAfterproducts")]
    public class WidgetsNexportOrderDetailsPageAfterproducts : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INexportPluginModelFactory _modelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;

        public WidgetsNexportOrderDetailsPageAfterproducts(
            IWorkContext workContext,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            INexportPluginModelFactory modelFactory,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _modelFactory = modelFactory;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var orderDetailsModel = (OrderDetailsModel)additionalData;

            var order = await _orderService.GetOrderByIdAsync(orderDetailsModel.Id);
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            var group = await _genericAttributeService.GetAttributeAsync<string>(order, $"GroupForOrder", store.Id);
            var deserialized = JsonConvert.DeserializeObject<NexportGroupModel>(group);
            ViewData["GroupNameForOrder"] = deserialized?.Name;

            
            return View("~/Plugins/Misc.Nexport/Views/Widget/Order/WidgetsNexportOrderDetailsPageAfterproducts.cshtml");
        }
    }
}