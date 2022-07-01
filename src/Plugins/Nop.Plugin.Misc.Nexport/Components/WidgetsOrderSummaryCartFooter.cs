using System.Threading.Tasks;
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

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,
                (await _storeContext.GetCurrentStoreAsync()).Id);
            var (_, appliedDiscounts, _, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);

            if ((_actionContextAccessor.ActionContext?.ActionDescriptor as ControllerActionDescriptor)?.ActionName == "Cart")
                return Content("");

            ViewData["DiscountList"] = appliedDiscounts;

            return View("~/Plugins/Misc.Nexport/Views/Widget/Order/WidgetsOrderSummaryCartFooter.cshtml");
        }
    }
}