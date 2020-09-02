using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class CheckoutActionFilter : ActionFilterAttribute
    {
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INotificationService _notificationService;
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public CheckoutActionFilter(
            INexportPluginModelFactory nexportPluginModelFactory,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _notificationService = notificationService;
            _nexportService = nexportService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(CheckoutController)
                && (actionDescriptor.ActionName == nameof(CheckoutController.Index)
                    || actionDescriptor.ActionName == nameof(CheckoutController.OnePageCheckout)))
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                var nexportProductMappings =
                    cart.Select(shoppingCartItem =>
                        _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId, _storeContext.CurrentStore.Id) ??
                        _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId))
                        .Where(mapping => mapping != null).ToList();

                // Check if there are products a product in the cart that have Nexport product mappings
                if (nexportProductMappings.Count > 0)
                {
                    var userMapping = _nexportService.FindUserMappingByCustomerId(_workContext.CurrentCustomer.Id);
                    // Create new Nexport user and map to this customer if the mapping does not existed
                    if (userMapping == null)
                    {
                        _nexportService.CreateAndMapNewNexportUser(_workContext.CurrentCustomer);
                    }
                }

                // Verify if the products in the cart are allowed to be purchased
                foreach (var productMapping in nexportProductMappings)
                {
                    var product = _productService.GetProductById(productMapping.NopProductId);
                    var canPurchaseProduct = _nexportService.CanRepurchaseNexportProduct(product, _workContext.CurrentCustomer);
                    if (!canPurchaseProduct)
                    {
                        var shoppingCartItem = cart.FirstOrDefault(i => i.ProductId == productMapping.NopProductId);
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
