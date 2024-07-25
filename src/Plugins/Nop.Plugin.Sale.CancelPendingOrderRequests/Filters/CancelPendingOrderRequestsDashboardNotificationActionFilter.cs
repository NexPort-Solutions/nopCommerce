using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Filters;

public class CancelPendingOrderRequestsDashboardNotificationActionFilter : ActionFilterAttribute
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

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            return;

        if (actionDescriptor.ControllerTypeInfo == typeof(HomeController) &&
            actionDescriptor.ActionName == "Index")
        {
            var cancelPendingOrderRequestsSetting =
                await _settingService.GetSettingAsync("Plugin.Sale.CancelPendingOrderRequests.HasModifiedLocaleResources");

            if (cancelPendingOrderRequestsSetting != null)
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(context);
                var action = urlHelper.Action("EditPopup", "Plugin", new { systemName = PluginDefaults.SystemName }) + "&btnId=btnRefreshList&formId=plugins-form-local";

                _notificationService.WarningNotification(
                    string.Format(await _localizationService.GetResourceAsync("Admin.PendingOrderCancellationRequests.Errors.ModifiedLocaleResources"), action), false);
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}