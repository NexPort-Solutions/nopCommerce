using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Services.Common;
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
        private readonly IGenericAttributeService _genericAttributeService;

        public WidgetsOrderSummaryCartFooter(
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IActionContextAccessor actionContextAccessor,
            INexportPluginModelFactory modelFactory,
            IGenericAttributeService genericAttributeService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _actionContextAccessor = actionContextAccessor;
            _modelFactory = modelFactory;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,
                (await _storeContext.GetCurrentStoreAsync()).Id);
            var (_, appliedDiscounts, _, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);


            if ((_actionContextAccessor.ActionContext?.ActionDescriptor as ControllerActionDescriptor)?.ActionName !=
                "Cart")
            {
                ViewData["DiscountList"] = appliedDiscounts;
                ViewData["HideGroupSelect"] = true;
            }

            //TODO - JS: prepare model in the model factory - set purchasing agent boolean and list of available groups
            var model = await _modelFactory.PrepareOrderSummaryCartFooterModel(new OrderSummaryCartFooterModel(),await _workContext.GetCurrentCustomerAsync(),await _storeContext.GetCurrentStoreAsync());

            return View("~/Plugins/Misc.Nexport/Views/Widget/Order/WidgetsOrderSummaryCartFooter.cshtml",model);
        }
    }
}