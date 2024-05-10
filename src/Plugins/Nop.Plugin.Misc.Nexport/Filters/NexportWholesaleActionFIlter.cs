using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class NexportWholesaleActionFilter : ActionFilterAttribute
    {
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly INotificationService _notificationService;
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;

        public NexportWholesaleActionFilter(
            INexportPluginModelFactory nexportPluginModelFactory,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IAuthenticationService authenticationService,
            IEventPublisher eventPublisher
        )
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _notificationService = notificationService;
            _nexportService = nexportService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _localizationService = localizationService;
            _authenticationService = authenticationService;
            _eventPublisher = eventPublisher;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(Nop.Plugin.Misc.Nexport.Controllers.NexportWholesaleController)
                && actionDescriptor.ActionName is nameof(Nop.Plugin.Misc.Nexport.Controllers.NexportWholesaleController.RedeemByEmail))
            {

            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}