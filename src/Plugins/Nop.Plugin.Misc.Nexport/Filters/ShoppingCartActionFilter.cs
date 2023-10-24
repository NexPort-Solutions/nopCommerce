using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services;
using System.Globalization;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class ShoppingCartActionFilter : ActionFilterAttribute
{
    private readonly ICustomerService _customer;
    private readonly IProductService _product;
    private readonly IShoppingCartService _shoppingCart;
    private readonly INotificationService _notification;
    private readonly ILocalizationService _localization;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly INexportService _nexportService;
    private readonly IProductMappingService _productMappingService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerPurchasingService _purchasing;

    public ShoppingCartActionFilter(
        ICustomerService customerService,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IGenericAttributeService genericAttributeService,
        INexportService nexportService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IProductMappingService productMappingService,
        ICustomerPurchasingService purchasing)
    {
        _customer = customerService;
        _product = productService;
        _shoppingCart = shoppingCartService;
        _notification = notificationService;
        _localization = localizationService;
        _genericAttribute = genericAttributeService;
        _nexportService = nexportService;
        _workContext = workContext;
        _storeContext = storeContext;
        _productMappingService = productMappingService;
        _purchasing = purchasing;
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (actionDescriptor.ControllerTypeInfo == typeof(ShoppingCartController)
            && actionDescriptor.ActionName == nameof(ShoppingCartController.Cart))
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (await _customer.IsRegisteredAsync(currentCustomer)
                && context.Result is ViewResult { Model: ShoppingCartModel shoppingCartModel })
            {
                foreach (var item in shoppingCartModel.Items)
                {
                    var product = await _product.GetProductByIdAsync(item.ProductId);
                    var canPurchaseProduct = await _purchasing.CanPurchaseProductAsync(product, currentCustomer);
                    if (!canPurchaseProduct)
                    {
                        item.Warnings.Add(await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved"));
                    }
                }
            }
        }
        await base.OnResultExecutionAsync(context, next);
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (actionDescriptor.ControllerTypeInfo == typeof(ShoppingCartController)
            && (actionDescriptor.ActionName is nameof(ShoppingCartController.AddProductToCart_Details) or nameof(ShoppingCartController.AddProductToCart_Catalog)
                || (actionDescriptor.ActionName == nameof(ShoppingCartController.Cart) && context.HttpContext.Request.Method == HttpMethods.Post)))
        {
            await CheckProductPurchaseEligibility(context);
        }
        await base.OnActionExecutionAsync(context, next);
    }

    private Task CheckProductPurchaseEligibility(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return Task.CompletedTask;
        }
        if (actionDescriptor.ActionName == nameof(ShoppingCartController.Cart))
        {
            return UpdateShoppingCartOnCartPage(context);
        }
        return AddToCartFromSearchOrProductPage(context, actionDescriptor);
    }

    private async Task UpdateShoppingCartOnCartPage(ActionExecutingContext context)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        if (!context.ActionArguments.TryGetValue("form", out var formValue) || formValue is not FormCollection form)
        {
            return;
        }
        var cart = await _shoppingCart.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
        var formCollection = form
            .ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value)
            .AsNameValueCollection();
        var displayError = false;
        foreach (var shoppingCartItem in cart)
        {
            var productMapping = await _productMappingService.GetByNopProductId(shoppingCartItem.ProductId, store.Id)
                ?? await _productMappingService.GetByNopProductId(shoppingCartItem.ProductId);
            if (productMapping is not { AutoRedeem: true })
            {
                continue;
            }
            var fieldName = $"itemquantity{shoppingCartItem.Id}";
            var itemQuantityValue = formCollection.Get(fieldName);
            var itemQuantity = int.TryParse(itemQuantityValue, out var quantity1) ? quantity1 : 0;
            if (itemQuantity <= 1)
            {
                continue;
            }
            formCollection.Set(fieldName, 1.ToString(CultureInfo.InvariantCulture));
            displayError = true;
        }
        var fields = formCollection.AllKeys.ToDictionary<string?, string, StringValues>(key => key ?? string.Empty, key => formCollection.Get(key));
        var newForm = new FormCollection(fields);
        context.ActionArguments.Remove("form");
        context.ActionArguments.Add("form", newForm);
        if (!displayError)
        {
            return;
        }
        _notification.ErrorNotification(await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart"));
    }

    private async Task AddToCartFromSearchOrProductPage(ActionExecutingContext context, ControllerActionDescriptor actionDescriptor)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        context.ActionArguments.TryGetValue("productId", out var productIdValue);
        context.ActionArguments.TryGetValue("shoppingCartTypeId", out var shoppingCartTypeValue);
        if (!(productIdValue is int productId and > 0)
            || shoppingCartTypeValue is not int shoppingCartType
            || (ShoppingCartType)shoppingCartType is not ShoppingCartType.ShoppingCart)
        {
            return;
        }
        var quantity = GetQuantity(context, actionDescriptor, productId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        var items = await _shoppingCart.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id, productId);
        var productMapping = await _productMappingService.GetByNopProductId(productId, store.Id)
            ?? await _productMappingService.GetByNopProductId(productId);
        if (productMapping is null)
        {
            return;
        }
        await ValidateThatProductIsPurchasable(context, customer, store, productId);
        var cart = await _shoppingCart.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
        // check if mixing auto redeem and manual redeem
        foreach (var shoppingCartItem in cart)
        {
            var npmInCart = await _productMappingService.GetByNopProductId(shoppingCartItem.ProductId, store.Id)
                ?? await _productMappingService.GetByNopProductId(shoppingCartItem.ProductId);
            if (npmInCart is null || productMapping.AutoRedeem == npmInCart.AutoRedeem)
            {
                continue;
            }
            var value = new
            {
                success = false,
                message = "You cannot mix redemption types in the shopping cart",
            };
            context.Result = new JsonResult(value);
            return;
        }

        // only check quantity and item count if the product is autoredeem, otherwise allow multiple quantities
        // assuming the user will distribute them in the manual redemption process later
        if (!productMapping.AutoRedeem)
        {
            return;
        }
        await CheckIfItemIsAlreadyInCart(context, quantity, items);
    }

    private static int GetQuantity(ActionExecutingContext context, ControllerActionDescriptor actionDescriptor, int productId)
    {
        // This condition is entered when you search for a product and click add to cart below its thumbnail without going to the products page
        if (actionDescriptor.ActionName is nameof(ShoppingCartController.AddProductToCart_Catalog)
            && context.ActionArguments.TryGetValue("quantity", out var quantityValue)
            && quantityValue is int quantityValueValue)
        {
            return quantityValueValue;
        }
        // This condition is entered if you click add to cart from the products page
        else if (context.ActionArguments.TryGetValue("form", out var value)
            && value is IFormCollection form
            && form.Keys.FirstOrDefault(formKey => formKey.Equals($"addtocart_{productId}.EnteredQuantity", StringComparison.OrdinalIgnoreCase)) is { } key
            && int.TryParse(form[key], out var enteredQuantity))
        {
            return enteredQuantity;
        }
        return 1;
    }

    private async Task ValidateThatProductIsPurchasable(ActionExecutingContext context, Customer customer, Store store, int productId)
    {
        if (await _product.GetProductByIdAsync(productId) is not { } product)
        {
            return;
        }
        if (await _customer.IsRegisteredAsync(customer) && !await _purchasing.CanPurchaseProductAsync(product, customer))
        {
            var value = new
            {
                success = false,
                message = await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase"),
            };
            context.Result = new JsonResult(value);
        }
        else
        {
            await CheckCategoryPurchaseEligibilityAsync(context, product, store.Id);
        }
    }

    private async Task CheckIfItemIsAlreadyInCart(ActionExecutingContext context, int quantity, IList<ShoppingCartItem> items)
    {
        if (items.Count is not 0)
        {
            // If the nexport product is restricted to one and is already in the cart we just want to redirect to the cart
            context.Result = new JsonResult(new { success = false, redirect = "cart1" });
        }
        else if (quantity is > 1)
        {
            var value = new
            {
                success = false,
                message = await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed"),
            };
            context.Result = new JsonResult(value);
        }
    }

    private async Task CheckCategoryPurchaseEligibilityAsync(ActionExecutingContext context, Product product, int storeId)
    {
        var productInTheSameCategory = await _purchasing.CanPurchaseProductInCategoryAsync(product, storeId);
        if (productInTheSameCategory is null)
        {
            return;
        }
        var autoSwapProduct = await _genericAttribute.GetAttributeAsync<bool>(productInTheSameCategory.Value.Item2, Defaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY);
        if (!autoSwapProduct)
        {
            var value = new
            {
                success = false,
                message = await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.SingleProductInCatalog"),
            };
            context.Result = new JsonResult(value);
            return;
        }
        await _shoppingCart.DeleteShoppingCartItemAsync(productInTheSameCategory.Value.Item1);
    }
}
