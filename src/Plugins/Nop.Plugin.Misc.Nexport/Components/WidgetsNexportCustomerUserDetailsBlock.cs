using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportCustomerUserDetailsBlock")]
    public class WidgetsNexportCustomerUserDetailsBlock: NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;

        public WidgetsNexportCustomerUserDetailsBlock(
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory)
        {
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customerUserSettingsModel = (CustomerUserSettingsModel) additionalData;

            if (customerUserSettingsModel == null)
                return Content("");

            var model = _nexportPluginModelFactory.PrepareNexportCustomerAdditionalSettingsModel();

            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Setting/NexportCustomerUserSettings.cshtml", model);
        }
    }
}
