using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class CheckoutActionFilter : ActionFilterAttribute
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INotificationService _notificationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public CheckoutActionFilter(
            IProductService productService,
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            ICustomerActivityService customerActivityService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _notificationService = notificationService;
            _customerActivityService = customerActivityService;
            _nexportService = nexportService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(CheckoutController) &&
                actionDescriptor.ActionName == "Index")
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                var nexportProductMappings = cart.Select(shoppingCartItem => _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId)).Where(mapping => mapping != null).ToList();

                // Check if there is at least a product in the cart that has Nexport product mapping
                //var hasNexportProduct = cart.Select(t => _nexportService.GetProductMappingByNopProductId(t.ProductId)).Any(nexportProductMapping => nexportProductMapping != null);
                if (nexportProductMappings.Count > 0)
                {
                    var userMapping = _nexportService.FindUserMappingByCustomerId(_workContext.CurrentCustomer.Id);
                    // Create new Nexport user and map to this customer if the mapping does not existed
                    if (userMapping == null)
                    {
                        _nexportService.CreateAndMapNewNexportUser(_workContext.CurrentCustomer);
                    }
                }

                foreach (var productMapping in nexportProductMappings)
                {
                    var product = _productService.GetProductById(productMapping.NopProductId);
                    var canPurchaseProduct = _nexportService.CanRepurchaseNexportProduct(product, _workContext.CurrentCustomer);
                    if (!canPurchaseProduct)
                    {
                        var shoppingCartItem = cart.FirstOrDefault(i => i.ProductId == productMapping.NopProductId);
                        //cart.Remove(shoppingCartItem);
                        if (shoppingCartItem != null)
                        {
                            _shoppingCartService.DeleteShoppingCartItem(shoppingCartItem);
                        }

                    }
                }

                if (!_workContext.CurrentCustomer.HasShoppingCartItems)
                {
                    context.Result = new RedirectToActionResult("Cart", "ShoppingCart", null);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
