using System.Linq;
using System.Threading.Tasks;
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

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(CheckoutController)
                && actionDescriptor.ActionName is nameof(CheckoutController.Index) or nameof(CheckoutController.OnePageCheckout))
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer,
                    ShoppingCartType.ShoppingCart, currentStore.Id);

                var nexportProductMappings =
                    await cart.SelectAwait(async shoppingCartItem =>
                        await _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId, currentStore.Id) ??
                        await _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId))
                        .Where(mapping => mapping != null).ToListAsync();

                // Check if there are products a product in the cart that have Nexport product mappings
                if (nexportProductMappings.Count > 0)
                {
                    var userMapping = await _nexportService.FindUserMappingByCustomerId(currentCustomer.Id);
                    // Create new Nexport user and map to this customer if the mapping does not existed
                    if (userMapping == null)
                    {
                        await _nexportService.CreateAndMapNewNexportUserAsync(currentCustomer);
                    }
                }

                // Verify if the products in the cart are allowed to be purchased
                foreach (var productMapping in nexportProductMappings)
                {
                    var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);
                    var canPurchaseProduct = await _nexportService.CanPurchaseNexportProductAsync(product, currentCustomer);
                    if (!canPurchaseProduct)
                    {
                        var shoppingCartItem = cart.FirstOrDefault(i => i.ProductId == productMapping.NopProductId);
                        if (shoppingCartItem != null)
                        {
                            await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem);
                        }
                    }
                }

                if (!currentCustomer.HasShoppingCartItems)
                {
                    context.Result = new RedirectToActionResult("Cart", "ShoppingCart", null);
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
