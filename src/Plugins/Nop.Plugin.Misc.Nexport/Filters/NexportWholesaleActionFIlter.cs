using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;

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

        public NexportWholesaleActionFilter(
            INexportPluginModelFactory nexportPluginModelFactory,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            INotificationService notificationService,
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ILocalizationService localizationService
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
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(Nop.Plugin.Misc.Nexport.Controllers.NexportWholesaleController)
                && actionDescriptor.ActionName is nameof(Nop.Plugin.Misc.Nexport.Controllers.NexportWholesaleController.RedeemByEmail))
            {

                var customer = await _workContext.GetCurrentCustomerAsync();

                if (!await _customerService.IsRegisteredAsync(customer))
                    context.Result = new ChallengeResult();

                context.ActionArguments.TryGetValue("nexportUserId", out var nexportUserId);

                if (nexportUserId == null){
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Failed to access page. nexportUserId could not be found."));
                    context.Result = new RedirectToRouteResult("Homepage", null);
                }
                else
                {
                    var userMapping = await _nexportService.FindUserMappingByNexportUserId((Guid)nexportUserId);
                    if (userMapping == null || userMapping.NopUserId != customer.Id)
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.RedeemByEmail.NotAuthorized"));
                        context.Result = new RedirectToRouteResult("Homepage", null);
                    }
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
