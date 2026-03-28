using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Filters;

public sealed class CancelPendingOrderRequestsDashboardNotificationActionFilter : IAsyncActionFilter
{
    private readonly INotificationService _notificationService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;

    public CancelPendingOrderRequestsDashboardNotificationActionFilter(
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
            await _settingService.GetSettingByKeyAsync("Plugin.Sale.CancelPendingOrderRequests.HasModifiedLocaleResources", false);
        if (!hasModifiedLocaleResources)
        {
            await next();
            return;
        }

        var urlHelper = _urlHelperFactory.GetUrlHelper(context);
        var action = urlHelper.Action("EditPopup", "Plugin", new { systemName = PluginDefaults.SystemName }) +
                     "&btnId=btnRefreshList&formId=plugins-form-local";

        var warningTemplate = await _localizationService.GetResourceAsync("Admin.PendingOrderCancellationRequests.Errors.ModifiedLocaleResources");
        _notificationService.WarningNotification(string.Format(warningTemplate, action), false);

        await next();
    }
}
