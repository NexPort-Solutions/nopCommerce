using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class ProductDetailsActionFilter : ActionFilterAttribute
    {
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly NexportService _nexportService;

        public ProductDetailsActionFilter(
            ICustomerService customerService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            NexportService nexportService)
        {
            _customerService = customerService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _nexportService = nexportService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ProductController) &&
                actionDescriptor.ActionName == nameof(ProductController.ProductDetails))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (customer != null && await _customerService.IsRegisteredAsync(customer))
                {
                    if (context.Result is ViewResult { Model: ProductDetailsModel productDetailsModel })
                    {
                        var store = await _storeContext.GetCurrentStoreAsync();

                        var items = await _shoppingCartService.GetShoppingCartAsync(customer,
                            ShoppingCartType.ShoppingCart,
                            store.Id, productDetailsModel.Id);

                        if (items.Count > 0)
                        {
                            if (await _genericAttributeService.GetAttributeAsync<bool>(store,
                                NexportDefaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
                            {
                                productDetailsModel.AddToCart.DisableBuyButton = true;
                            }
                        }
                        else
                        {

                            var product = await _productService.GetProductByIdAsync(productDetailsModel.Id);

                            try
                            {
                                var canPurchaseProduct =
                                    await _nexportService.CanPurchaseNexportProductAsync(product, customer);

                                if (await _genericAttributeService.GetAttributeAsync<bool>(store,
                                    NexportDefaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id))
                                {
                                    productDetailsModel.AddToCart.DisableBuyButton = !canPurchaseProduct;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
