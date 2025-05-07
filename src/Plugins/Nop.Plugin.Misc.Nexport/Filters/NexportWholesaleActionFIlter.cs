using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using IAuthenticationService = Nop.Services.Authentication.IAuthenticationService;

namespace Nop.Plugin.Misc.Nexport.Filters;

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
    private readonly IWebHelper _webHelper;

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
        IEventPublisher eventPublisher,
        IWebHelper webHelper
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
        _webHelper = webHelper;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            return;

        if (actionDescriptor.ControllerTypeInfo == typeof(Controllers.NexportWholesaleController)
            && actionDescriptor.ActionName is nameof(Controllers.NexportWholesaleController.RedeemByEmail))
        {
            // Force the customer to re-authenticate when they click the link to redeem from their email
            var isMarketplaceUrl = _webHelper.GetUrlReferrer() != null;

            if (!isMarketplaceUrl)
            {
                await _authenticationService.SignOutAsync();
                context.Result = new ChallengeResult();
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}