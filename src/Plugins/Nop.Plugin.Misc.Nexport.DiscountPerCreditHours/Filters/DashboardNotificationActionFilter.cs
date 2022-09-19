using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Filters
{
    public  class 
        DashboardNotificationActionFilter : ActionFilterAttribute
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
                var discountPerCreditHoursSetting = 
                    await _settingService.GetSettingAsync("Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources");

                if (discountPerCreditHoursSetting != null)
                {
                    // using urlhelperfactory to build links for us rather than hardcoding them
                    var urlHelper = _urlHelperFactory.GetUrlHelper(context);

                    // this action will point to this plugins "edit plugin detail" action and view
                    var action = urlHelper.Action("EditPopup", "Plugin", new {systemName = NexportDiscountDefaults.SystemName }) + "&btnId=btnRefreshList&formId=plugins-form-local";

                    _notificationService.WarningNotification(
                        string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.DiscountPerCreditHours.Errors.ModifiedLocaleResources"),
                            action),
                        //do not encode URLs    
                        false);
                }

            }
            await base.OnActionExecutionAsync(context, next);
        }

    }
}
