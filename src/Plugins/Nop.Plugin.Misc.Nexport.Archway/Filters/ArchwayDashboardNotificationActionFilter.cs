using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Messages;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Misc.Nexport.Archway.Filters;

public class ArchwayDashboardNotificationActionFilter : ActionFilterAttribute
{
    private readonly INotificationService _notificationService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;

    public ArchwayDashboardNotificationActionFilter(
        INotificationService notificationService,
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

        if (actionDescriptor.ControllerTypeInfo == typeof(HomeController) &&
            actionDescriptor.ActionName == "Index")
        {
            var archwaySetting = await _settingService.GetSettingAsync("Plugin.Misc.Nexport.Archway.HasModifiedLocaleResources");

            if (archwaySetting != null)
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(context);

                var action = urlHelper.Action("EditPopup", "Plugin", new { systemName = PluginDefaults.SystemName }) + "&btnId=btnRefreshList&formId=plugins-form-local";

                _notificationService.WarningNotification(
                    string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Archway.Errors.ModifiedLocaleResources"),
                        action), false);
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}