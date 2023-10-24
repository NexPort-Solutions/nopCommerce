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
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class CheckoutActionFilter : ActionFilterAttribute
{
    private readonly IPluginModelFactory _model;
    private readonly IProductService _product;
    private readonly IShoppingCartService _shoppingCart;
    private readonly INotificationService _notification;
    private readonly INexportService _nexport;
    private readonly IProductMappingService _productMapping;
    private readonly IUserMappingService _mapping;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IUserService _user;
    private readonly ICustomerPurchasingService _purchasing;

    public CheckoutActionFilter(
        IPluginModelFactory pluginModelFactory,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        INotificationService notificationService,
        INexportService nexportService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IProductMappingService productMappingService,
        IUserMappingService userMapping,
        IUserService userService,
        ICustomerPurchasingService purchasing)
    {
        _model = pluginModelFactory;
        _product = productService;
        _shoppingCart = shoppingCartService;
        _notification = notificationService;
        _nexport = nexportService;
        _workContext = workContext;
        _storeContext = storeContext;
        _productMapping = productMappingService;
        _mapping = userMapping;
        _user = userService;
        _purchasing = purchasing;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (actionDescriptor.ControllerTypeInfo == typeof(CheckoutController)
            && actionDescriptor.ActionName is nameof(CheckoutController.Index) or nameof(CheckoutController.OnePageCheckout))
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCart.GetShoppingCartAsync(
                currentCustomer,
                ShoppingCartType.ShoppingCart,
                currentStore.Id);
            var productMappings = await cart.SelectAwait(async shoppingCartItem =>
                await _productMapping.GetByNopProductId(shoppingCartItem.ProductId, currentStore.Id)
                    ?? await _productMapping.GetByNopProductId(shoppingCartItem.ProductId))
                .WhereNotNull()
                .ToListAsync();

            // Check if there are products a product in the cart that have NexPort product mappings
            if (productMappings.Count > 0)
            {
                var userMapping = await _mapping.FindByCustomerId(currentCustomer.Id);
                // Create new NexPort user and map to this customer if the mapping does not existed
                if (userMapping is null)
                {
                    await _user.CreateAndMapNewUserAsync(currentCustomer);
                }
            }

            // Verify if the products in the cart are allowed to be purchased
            foreach (var productMapping in productMappings)
            {
                var product = await _product.GetProductByIdAsync(productMapping.NopProductId);
                var canPurchaseProduct = await _purchasing.CanPurchaseProductAsync(product, currentCustomer);
                if (!canPurchaseProduct)
                {
                    var shoppingCartItem = cart.FirstOrDefault(item => item.ProductId == productMapping.NopProductId);
                    if (shoppingCartItem is not null)
                    {
                        await _shoppingCart.DeleteShoppingCartItemAsync(shoppingCartItem);
                    }
                }
            }
            if (!currentCustomer.HasShoppingCartItems)
            {
                context.Result = new RedirectToActionResult(nameof(ShoppingCartController.Cart), ViewUtilities.GetControllerName<ShoppingCartController>(), null);
            }
        }
        await base.OnActionExecutionAsync(context, next);
    }
}
