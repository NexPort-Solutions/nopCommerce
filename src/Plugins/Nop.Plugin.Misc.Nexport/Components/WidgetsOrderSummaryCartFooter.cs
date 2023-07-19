using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsOrderSummaryCartFooter")]
    public class WidgetsOrderSummaryCartFooter : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly INexportPluginModelFactory _modelFactory;

        public WidgetsOrderSummaryCartFooter(
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IActionContextAccessor actionContextAccessor,
            INexportPluginModelFactory modelFactory)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _actionContextAccessor = actionContextAccessor;
            _modelFactory = modelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var (_, appliedDiscounts, _, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);


            //TODO - JS: prepare model in the model factory - set purchasing agent boolean and list of available groups
            var model = await _modelFactory.PrepareOrderSummaryCartFooterModel(new OrderSummaryCartFooterModel(), customer, store, cart);

            if ((_actionContextAccessor.ActionContext?.ActionDescriptor as ControllerActionDescriptor)?.ActionName !=
                "Cart")
            {
                ViewData["DiscountList"] = appliedDiscounts;
                model.PurchasingGroupSelectBoxStyle = "display:none";
            }


            return View("~/Plugins/Misc.Nexport/Views/Widget/Order/WidgetsOrderSummaryCartFooter.cshtml", model);
        }
    }
}