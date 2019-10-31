using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class CheckoutActionFilter : ActionFilterAttribute
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INotificationService _notificationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public CheckoutActionFilter(
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            ICustomerActivityService customerActivityService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _shoppingCartService = shoppingCartService;
            _notificationService = notificationService;
            _customerActivityService = customerActivityService;
            _nexportService = nexportService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(CheckoutController) &&
                actionDescriptor.ActionName == "Index")
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                // Check if there is at least a product in the cart that has Nexport product mapping
                var hasNexportProduct = cart.Select(t => _nexportService.GetProductMappingByNopProductId(t.ProductId)).Any(nexportProductMapping => nexportProductMapping != null);
                if (hasNexportProduct)
                {
                    var userMapping = _nexportService.FindUserMappingByCustomerId(_workContext.CurrentCustomer.Id);
                    // Create new Nexport user and map to this customer if the mapping does not existed
                    if (userMapping == null)
                    {
                        _nexportService.CreateAndMapNewNexportUser(_workContext.CurrentCustomer);
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
