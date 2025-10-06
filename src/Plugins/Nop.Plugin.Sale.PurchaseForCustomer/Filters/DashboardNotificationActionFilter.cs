using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Filters;

public class DashboardNotificationActionFilter : ActionFilterAttribute
{
    private readonly INotificationService _notificationService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;

    public DashboardNotificationActionFilter(INotificationService notificationService,
        IUrlHelperFactory urlHelperFactory,
        ILocalizationService localizationService,
        ISettingService settingService)
    {
        _notificationService = notificationService;
        _urlHelperFactory = urlHelperFactory;
        _localizationService = localizationService;
        _settingService = settingService;
    }


    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            return;

        // Display conflict message in the Admin dashboard area
        if (actionDescriptor.ControllerTypeInfo == typeof(HomeController) &&
            actionDescriptor.ActionName == "Index")
        {
            var setting = await _settingService.GetSettingAsync("Plugin.Sale.PurchaseForCustomer.HasModifiedLocaleResources");

            if (setting != null)
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(context);
                var action = urlHelper.Action("EditPopup", "Plugin", new { systemName = PluginDefaults.SystemName }) + "&btnId=btnRefreshList&formId=plugins-form-local";

                _notificationService.WarningNotification(
                    string.Format(await _localizationService.GetResourceAsync("Plugins.Sale.PurchaseForCustomer.Errors.ModifiedLocaleResources"), action),
                    false);
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}