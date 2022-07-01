using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class ShoppingCartActionFilter : ActionFilterAttribute
    {
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public ShoppingCartActionFilter(
            ICustomerService customerService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _customerService = customerService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _nexportService = nexportService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ShoppingCartController) &&
                actionDescriptor.ActionName == nameof(ShoppingCartController.Cart))
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (await _customerService.IsRegisteredAsync(currentCustomer))
                {
                    if (context.Result is ViewResult { Model: ShoppingCartModel shoppingCartModel })
                    {
                        foreach (var item in shoppingCartModel.Items)
                        {
                            var product = await _productService.GetProductByIdAsync(item.ProductId);
                            var canPurchaseProduct =
                                await _nexportService.CanPurchaseNexportProductAsync(product, currentCustomer);

                            if (!canPurchaseProduct)
                            {
                                item.Warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved"));
                            }
                        }
                    }
                }
            }

            await base.OnResultExecutionAsync(context, next);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ShoppingCartController) &&
                (actionDescriptor.ActionName is nameof(ShoppingCartController.AddProductToCart_Details) or nameof(ShoppingCartController.AddProductToCart_Catalog) ||
                 actionDescriptor.ActionName == nameof(ShoppingCartController.Cart) && context.HttpContext.Request.Method == HttpMethods.Post))
            {
                await CheckProductPurchaseEligibilityAsync(context);
            }

            await base.OnActionExecutionAsync(context, next);
        }


        protected async Task CheckProductPurchaseEligibilityAsync(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            // Verify the product quantities when customers update the cart
            if (actionDescriptor.ActionName == nameof(ShoppingCartController.Cart))
            {
                context.ActionArguments.TryGetValue("form", out var formValue);
                if (formValue is FormCollection form)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                        ShoppingCartType.ShoppingCart, store.Id);

                    var storeModel = await _genericAttributeService.GetAttributeAsync<NexportStoreSaleModel>(
                        store, "NexportStoreSaleModel", store.Id);

                    if (storeModel == NexportStoreSaleModel.Retail)
                    {
                        var formCollection = form
                            .ToDictionary(x => x.Key, x => x.Value)
                            .AsNameValueCollection();

                        var displayError = false;

                        foreach (var shoppingCartItem in cart)
                        {
                            var nexportProductMapping =
                                await _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId, store.Id) ??
                                await _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId);
                            if (nexportProductMapping == null)
                                continue;

                            var fieldName = $"itemquantity{shoppingCartItem.Id}";
                            var itemQuantityValue = formCollection.Get(fieldName);
                            var itemQuantity = int.TryParse(itemQuantityValue, out var quantity) ? quantity : 0;

                            if (itemQuantity <= 1)
                                continue;

                            formCollection.Set(fieldName, 1.ToString());
                            displayError = true;
                        }

                        var newForm = new FormCollection(formCollection.AllKeys
                            .ToDictionary<string, string, StringValues>(
                                key => key,
                                key => formCollection.Get(key)));

                        context.ActionArguments.Remove("form");
                        context.ActionArguments.Add("form", newForm);

                        if (displayError)
                            _notificationService.ErrorNotification(
                                await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart"));
                    }
                }
            }
            else
            {
                var quantity = 1;

                context.ActionArguments.TryGetValue("productId", out var productIdValue);
                context.ActionArguments.TryGetValue("shoppingCartTypeId", out var shoppingCartTypeValue);

                if (productIdValue is int productId and > 0 &&
                    shoppingCartTypeValue is int shoppingCartType && (ShoppingCartType)shoppingCartType == ShoppingCartType.ShoppingCart)
                {
                    if (actionDescriptor.ActionName == nameof(ShoppingCartController.AddProductToCart_Catalog))
                    {
                        if (context.ActionArguments.TryGetValue("quantity", out var value)
                            && value is int quantityValue)
                        {
                            quantity = quantityValue;
                        }
                    }
                    else
                    {
                        if (context.ActionArguments.TryGetValue("form", out var value)
                            && value is IFormCollection form)
                        {
                            foreach (var formKey in form.Keys)
                                if (formKey.Equals($"addtocart_{productId}.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    int.TryParse(form[formKey], out quantity);
                                    break;
                                }
                        }
                    }

                    var customer = await _workContext.GetCurrentCustomerAsync();
                    var store = await _storeContext.GetCurrentStoreAsync();

                    var items = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                        store.Id, productId);

                    var storeModel = await _genericAttributeService.GetAttributeAsync<NexportStoreSaleModel>(
                        store, "NexportStoreSaleModel", store.Id);

                    if (storeModel == NexportStoreSaleModel.Retail)
                    {
                        var nexportProductMapping =
                            await _nexportService.GetProductMappingByNopProductId(productId, store.Id) ??
                            await _nexportService.GetProductMappingByNopProductId(productId);
                        if (nexportProductMapping != null)
                        {
                            var product = await _productService.GetProductByIdAsync(productId);
                            if (product != null)
                            {
                                if (await _customerService.IsRegisteredAsync(customer))
                                {
                                    var canPurchaseProduct =
                                        await _nexportService.CanPurchaseNexportProductAsync(product, customer);

                                    if (!canPurchaseProduct)
                                    {
                                        context.Result = new JsonResult(new
                                        {
                                            success = false,
                                            message = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase")
                                        });
                                    }
                                    else
                                    {
                                        await CheckNexportCategoryPurchaseEligibilityAsync(context, product, store.Id);
                                    }
                                }
                                else
                                {
                                    await CheckNexportCategoryPurchaseEligibilityAsync(context, product, store.Id);
                                }
                            }

                            if (items.Count > 0)
                            {
                                context.Result = new JsonResult(new
                                {
                                    success = false,
                                    message = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.DuplicatedProduct")
                                });
                            }
                            else if (quantity > 1)
                            {
                                context.Result = new JsonResult(new
                                {
                                    success = false,
                                    message = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed")
                                });
                            }
                        }
                    }
                }
            }
        }

        private async Task CheckNexportCategoryPurchaseEligibilityAsync(ActionExecutingContext context, Product product, int storeId)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productInTheSameCategory =
                await _nexportService.CanPurchaseProductInNexportCategoryAsync(product, storeId);

            if (productInTheSameCategory.Item1 != null)
            {
                var autoSwapProduct = await _genericAttributeService.GetAttributeAsync<bool>(
                    productInTheSameCategory.Item2, NexportDefaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY);
                if (autoSwapProduct)
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(productInTheSameCategory.Item1);
                }
                else
                {
                    context.Result = new JsonResult(new
                    {
                        success = false,
                        message = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.SingleProductInCatalog")
                    });
                }
            }
        }
    }
}
