using System;
using System.Linq;
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

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ShoppingCartController) &&
                actionDescriptor.ActionName == nameof(ShoppingCartController.Cart))
            {
                if (_customerService.IsRegistered(_workContext.CurrentCustomer))
                {
                    if (context.Result is ViewResult { Model: ShoppingCartModel shoppingCartModel })
                    {
                        foreach (var item in shoppingCartModel.Items)
                        {
                            var product = _productService.GetProductById(item.ProductId);
                            var canPurchaseProduct =
                                _nexportService.CanPurchaseNexportProduct(product, _workContext.CurrentCustomer);

                            if (!canPurchaseProduct)
                            {
                                item.Warnings.Add(_localizationService.GetResource("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved"));
                            }
                        }
                    }
                }
            }

            base.OnResultExecuting(context);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ShoppingCartController) &&
                (actionDescriptor.ActionName == nameof(ShoppingCartController.AddProductToCart_Details) ||
                 actionDescriptor.ActionName == nameof(ShoppingCartController.AddProductToCart_Catalog) ||
                 actionDescriptor.ActionName == nameof(ShoppingCartController.Cart) && context.HttpContext.Request.Method == HttpMethods.Post))
            {
                CheckProductPurchaseEligibility(context);
            }

            base.OnActionExecuting(context);
        }

        protected void CheckProductPurchaseEligibility(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            // Verify the product quantities when customers update the cart
            if (actionDescriptor.ActionName == nameof(ShoppingCartController.Cart))
            {
                context.ActionArguments.TryGetValue("form", out var formValue);
                if (formValue is FormCollection form)
                {
                    var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer,
                        ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                    var store = _storeContext.CurrentStore;
                    var storeModel = _genericAttributeService.GetAttribute<NexportStoreSaleModel>(
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
                                _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId, store.Id) ??
                                _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId);
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
                                _localizationService.GetResource("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart"));
                    }
                }
            }
            else
            {
                var quantity = 1;

                context.ActionArguments.TryGetValue("productId", out var productIdValue);
                context.ActionArguments.TryGetValue("shoppingCartTypeId", out var shoppingCartTypeValue);

                if (productIdValue is int productId && productId > 0 &&
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

                    var items = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart,
                        _storeContext.CurrentStore.Id, productId);

                    var store = _storeContext.CurrentStore;
                    var storeModel = _genericAttributeService.GetAttribute<NexportStoreSaleModel>(
                        store, "NexportStoreSaleModel", store.Id);

                    if (storeModel == NexportStoreSaleModel.Retail)
                    {
                        var nexportProductMapping =
                            _nexportService.GetProductMappingByNopProductId(productId, store.Id) ??
                            _nexportService.GetProductMappingByNopProductId(productId);
                        if (nexportProductMapping != null)
                        {
                            var product = _productService.GetProductById(productId);
                            if (product != null)
                            {
                                var customer = _workContext.CurrentCustomer;
                                if (_customerService.IsRegistered(customer))
                                {
                                    var canPurchaseProduct =
                                        _nexportService.CanPurchaseNexportProduct(product, _workContext.CurrentCustomer);

                                    if (!canPurchaseProduct)
                                    {
                                        context.Result = new JsonResult(new
                                        {
                                            success = false,
                                            message = _localizationService.GetResource("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase")
                                        });
                                    }
                                    else
                                    {
                                        CheckNexportCategoryPurchaseEligibility(context, product, store.Id);
                                    }
                                }
                                else
                                {
                                    CheckNexportCategoryPurchaseEligibility(context, product, store.Id);
                                }
                            }

                            if (items.Count > 0)
                            {
                                context.Result = new JsonResult(new
                                {
                                    success = false,
                                    message = _localizationService.GetResource("Plugins.Misc.Nexport.Errors.DuplicatedProduct")
                                });
                            }
                            else if (quantity > 1)
                            {
                                context.Result = new JsonResult(new
                                {
                                    success = false,
                                    message = _localizationService.GetResource("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed")
                                });
                            }
                        }
                    }
                }
            }
        }

        private void CheckNexportCategoryPurchaseEligibility(ActionExecutingContext context, Product product, int storeId)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productInTheSameCategory =
                _nexportService.CanPurchaseProductInNexportCategory(product, storeId);

            if (productInTheSameCategory.Item1 != null)
            {
                var autoSwapProduct = _genericAttributeService.GetAttribute<bool>(
                    productInTheSameCategory.Item2, NexportDefaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY);
                if (autoSwapProduct)
                {
                    _shoppingCartService.DeleteShoppingCartItem(productInTheSameCategory.Item1);
                }
                else
                {
                    context.Result = new JsonResult(new
                    {
                        success = false,
                        message = _localizationService.GetResource("Plugins.Misc.Nexport.Errors.SingleProductInCatalog")
                    });
                }
            }
        }
    }
}
