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
                actionDescriptor.ActionName == "ProductDetails")
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
                                productDetailsModel.AddToCart.DisableBuyButton = true;
                            }
                            else
                            {
                                var product = _productService.GetProductById(productDetailsModel.Id);
                                var canPurchaseProduct =
                                    _nexportService.CanRepurchaseNexportProduct(product, customer);

                                productDetailsModel.AddToCart.DisableBuyButton = !canPurchaseProduct;
                            }
                        }
                    }
                }
            }

            base.OnResultExecuting(context);
        }
    }
}
