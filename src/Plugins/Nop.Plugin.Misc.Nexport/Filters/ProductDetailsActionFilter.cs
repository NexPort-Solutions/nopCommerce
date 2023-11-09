using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Services;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class ProductDetailsActionFilter : ActionFilterAttribute
{
    private readonly ICustomerService _customer;
    private readonly IProductService _product;
    private readonly IShoppingCartService _shoppingCart;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly INexportService _nexportService;
    private readonly ICustomerPurchasingService _purchasing;

    public ProductDetailsActionFilter(
        ICustomerService customerService,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        IGenericAttributeService genericAttributeService,
        IStoreContext storeContext,
        IWorkContext workContext,
        INexportService nexportService,
        ICustomerPurchasingService purchasing)
    {
        _customer = customerService;
        _product = productService;
        _shoppingCart = shoppingCartService;
        _genericAttribute = genericAttributeService;
        _storeContext = storeContext;
        _workContext = workContext;
        _nexportService = nexportService;
        _purchasing = purchasing;
    }

    //public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    //{
    //    if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
    //    {
    //        return;
    //    }
    //    if (actionDescriptor.ControllerTypeInfo != typeof(ProductController)
    //        || actionDescriptor.ActionName is not nameof(ProductController.ProductDetails)
    //        || await _workContext.GetCurrentCustomerAsync() is not { } customer
    //        || !await _customer.IsRegisteredAsync(customer)
    //        || context.Result is not ViewResult { Model: ProductDetailsModel productDetailsModel }
    //        || await _storeContext.GetCurrentStoreAsync() is not { } store
    //        || await _genericAttribute.GetAttributeAsync<StoreSaleModel>(store, Defaults.STORE_SALE_MODEL_SETTING_KEY, store.Id) is not StoreSaleModel.Retail)
    //    {
    //        return;
    //    }
    //    if ((await _shoppingCart.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id, productDetailsModel.Id)) is not []
    //        && await _genericAttribute.GetAttributeAsync<bool>(store, Defaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
    //    {
    //        productDetailsModel.AddToCart.DisableBuyButton = true;
    //        await base.OnActionExecutionAsync(context, next);
    //        return;
    //    }
    //    var product = await _product.GetProductByIdAsync(productDetailsModel.Id);
    //    var canPurchaseProduct = await _purchasing.CanPurchaseProductAsync(product, customer);
    //    if (await _genericAttribute.GetAttributeAsync<bool>(store, Defaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
    //    {
    //        productDetailsModel.AddToCart.DisableBuyButton = !canPurchaseProduct;
    //    }
    //}

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (actionDescriptor.ControllerTypeInfo == typeof(ProductController)
            && actionDescriptor.ActionName == nameof(ProductController.ProductDetails)
            && await _workContext.GetCurrentCustomerAsync() is { } customer
            && await _customer.IsRegisteredAsync(customer)
            && context.Result is ViewResult { Model: ProductDetailsModel productDetailsModel })
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            if (await _genericAttribute.GetAttributeAsync<StoreSaleModel>(store, "NexportStoreSaleModel", store.Id) is not StoreSaleModel.Retail)
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }
            if (await _shoppingCart.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id, productDetailsModel.Id) is { Count: > 0 })
            {
                if (await _genericAttribute.GetAttributeAsync<bool>(store, Defaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
                {
                    productDetailsModel.AddToCart.DisableBuyButton = true;
                }
                await base.OnActionExecutionAsync(context, next);
                return;
            }
            if (await _genericAttribute.GetAttributeAsync<bool>(store, Defaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
            {
                var product = await _product.GetProductByIdAsync(productDetailsModel.Id);
                productDetailsModel.AddToCart.DisableBuyButton = !await _purchasing.CanPurchaseProductAsync(product, customer);
            }
        }
        await base.OnActionExecutionAsync(context, next);
    }
}
