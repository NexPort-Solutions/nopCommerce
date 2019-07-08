using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core;
using Nop.Core.Domain.Orders;
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

        public WidgetsOrderSummaryCartFooter(
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IActionContextAccessor actionContextAccessor)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _actionContextAccessor = actionContextAccessor;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart,
                _storeContext.CurrentStore.Id);
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                false,
                out _,
                out var appliedDiscounts,
                out _, out _);

            if ((_actionContextAccessor.ActionContext.ActionDescriptor as ControllerActionDescriptor)?.ActionName == "Cart")
                return Content("");

            ViewData["DiscountList"] = appliedDiscounts;

            return View("~/Plugins/Misc.Nexport/Views/Widget/Order/WidgetsOrderSummaryCartFooter.cshtml");
        }
    }
}