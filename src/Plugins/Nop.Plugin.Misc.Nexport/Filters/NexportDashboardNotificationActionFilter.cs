using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    /* Action Filters attributes can apply to a controller action or an entire controller
     * - It modifies the way in which the action is executed
     * - We would need to add the filters to our pluginstarup's configureservice method
     */
    public class NexportDashboardNotificationActionFilter : ActionFilterAttribute
    {
        private readonly INotificationService _notificationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public NexportDashboardNotificationActionFilter(INotificationService notificationService, 
            IUrlHelperFactory urlHelperFactory,
            ILocalizationService localizationService,
            ISettingService settingService
            )
        {
            _notificationService = notificationService;
            _urlHelperFactory = urlHelperFactory;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        // this is called before the index action of the home controller is executed
        // we want to verify if there are any modified resources and if there are
        // we need to show a message for the admin to resolve them
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(HomeController) &&
                actionDescriptor.ActionName == "Index")
            {
                var nexportSetting =
                   await _settingService.GetSettingAsync("Plugin.Misc.Nexport.HasModifiedLocaleResources");

                // not neccessary to check if its false, since i'm never going to set it to true
                if (nexportSetting != null) 
                {
                    // using urlhelperfactory to build links for us rather than hardcoding them
                    var urlHelper = _urlHelperFactory.GetUrlHelper(context);

                    var action = urlHelper.Action("EditPopup", "Plugin", new { systemName = NexportDefaults.SystemName }) + "&btnId=btnRefreshList&formId=plugins-form-local";

                    _notificationService.WarningNotification(
                        string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.ModifiedLocaleResources"),
                            action),
                        //do not encode URLs
                        false);
                }
            }
            await base.OnActionExecutionAsync(context, next);
        }
    }
}
