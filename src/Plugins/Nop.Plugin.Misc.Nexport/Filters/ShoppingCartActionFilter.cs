using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class ShoppingCartActionFilter : ActionFilterAttribute
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INotificationService _notificationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public ShoppingCartActionFilter(
            IProductService productService,
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            ICustomerActivityService customerActivityService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _notificationService = notificationService;
            _customerActivityService = customerActivityService;
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
                if (_workContext.CurrentCustomer.IsRegistered())
                {
                    if (context.Result is ViewResult result && result.Model is ShoppingCartModel shoppingCartModel)
                    {
                        foreach (var item in shoppingCartModel.Items)
                        {
                            var product = _productService.GetProductById(item.ProductId);
                            var canPurchaseProduct =
                                _nexportService.CanRepurchaseNexportProduct(product, _workContext.CurrentCustomer);

                            if (!canPurchaseProduct)
                            {
                                item.Warnings.Add("This item cannot be purchased at this time and will be removed at checkout!");
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
                (actionDescriptor.ActionName == nameof(ShoppingCartController.AddProductToCart_Details) || actionDescriptor.ActionName == nameof(ShoppingCartController.AddProductToCart_Catalog)))
            {
                if (_workContext.CurrentCustomer.IsRegistered())
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

                        if (items.Count > 0)
                        {
                            context.Result = new JsonResult(new
                            {
                                success = false,
                                message = "Cannot add duplicated product to the cart"
                            });
                        } else if (quantity > 1)
                        {
                            context.Result = new JsonResult(new
                            {
                                success = false,
                                message = "Cannot purchase more than one for this product"
                            });
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
