using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class ProductDetailsActionFilter : ActionFilterAttribute
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly NexportService _nexportService;

        public ProductDetailsActionFilter(
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            NexportService nexportService)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _nexportService = nexportService;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;
            if (actionDescriptor.ControllerTypeInfo == typeof(ProductController) &&
                actionDescriptor.ActionName == nameof(ProductController.ProductDetails))
            {
                var customer = _workContext.CurrentCustomer;
                if (customer != null && customer.IsRegistered())
                {
                    if (context.Result is ViewResult result && result.Model is ProductDetailsModel productDetailsModel)
                    {
                        var store = _storeContext.CurrentStore;
                        var storeModel = _genericAttributeService.GetAttribute<NexportStoreSaleModel>(
                            store, "NexportStoreSaleModel", store.Id);

                        if (storeModel == NexportStoreSaleModel.Retail)
                        {
                            var items = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart,
                                _storeContext.CurrentStore.Id, productDetailsModel.Id);

                            if (items.Count > 0)
                            {
                                if (_genericAttributeService.GetAttribute<bool>(store,
                                    NexportDefaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
                                {
                                    productDetailsModel.AddToCart.DisableBuyButton = true;
                                }
                            }
                            else
                            {
                                var product = _productService.GetProductById(productDetailsModel.Id);
                                var canPurchaseProduct =
                                    _nexportService.CanRepurchaseNexportProduct(product, customer);

                                if (_genericAttributeService.GetAttribute<bool>(store,
                                    NexportDefaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
                                {
                                    productDetailsModel.AddToCart.DisableBuyButton = !canPurchaseProduct;
                                }
                            }
                        }
                    }
                }
            }
            //} else if (actionDescriptor.ControllerTypeInfo == typeof(CatalogController) &&
            //           actionDescriptor.ActionName == nameof(CatalogController.Category))
            //{
            //    var customer = _workContext.CurrentCustomer;
            //    if (customer != null && customer.IsRegistered())
            //    {
            //        if (context.Result is ViewResult result && result.Model is CategoryModel categoryModel)
            //        {
            //            var store = _storeContext.CurrentStore;
            //            var storeModel = _genericAttributeService.GetAttribute<NexportStoreSaleModel>(
            //                store, "NexportStoreSaleModel", store.Id);

            //            if (storeModel == NexportStoreSaleModel.Retail)
            //            {
            //                var productModels = categoryModel.Products;

            //                foreach (var productModel in productModels)
            //                {
            //                    var items = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart,
            //                        _storeContext.CurrentStore.Id, productModel.Id);

            //                    if (items.Count > 0)
            //                    {
            //                        productModel.ProductPrice.DisableBuyButton = true;
            //                    }
            //                    else
            //                    {
            //                        var product = _productService.GetProductById(productModel.Id);
            //                        var canPurchaseProduct =
            //                            _nexportService.CanRepurchaseNexportProduct(product, customer);

            //                        productModel.ProductPrice.DisableBuyButton = !canPurchaseProduct;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            base.OnResultExecuting(context);
        }
    }
}
