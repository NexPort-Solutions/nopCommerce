using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class NexportDashboardNotificationActionFilter : IAsyncActionFilter
{
    private readonly INotificationService _notificationService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;

    public NexportDashboardNotificationActionFilter(
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

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            await next();
            return;
        }

        if (actionDescriptor.ControllerTypeInfo != typeof(HomeController) ||
            !string.Equals(actionDescriptor.ActionName, nameof(HomeController.Index), StringComparison.Ordinal))
        {
            await next();
            return;
        }

        var hasModifiedLocaleResources =
            await _settingService.GetSettingByKeyAsync("Plugin.Misc.Nexport.HasModifiedLocaleResources", false);

        if (!hasModifiedLocaleResources)
        {
            await next();
            return;
        }

        var urlHelper = _urlHelperFactory.GetUrlHelper(context);
        var action = urlHelper.Action("EditPopup", "Plugin",
            new { systemName = NexportDefaults.SystemName }) + "&btnId=btnRefreshList&formId=plugins-form-local";

        var warningTemplate = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.ModifiedLocaleResources");
        _notificationService.WarningNotification(string.Format(warningTemplate, action), false);

        await next();
    }
}
