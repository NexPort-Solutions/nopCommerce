using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportRegistrationFields")]
    public class WidgetsNexportRegistrationFields : NopViewComponent
    {
        private readonly NexportSettings _nexportSettings;
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IStoreContext _storeContext;

        public WidgetsNexportRegistrationFields(
            NexportSettings nexportSettings,
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory,
            IStoreContext storeContext)
        {
            _nexportSettings = nexportSettings;
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return Content("");

            var model =
                await _nexportPluginModelFactory.PrepareNexportCustomerRegistrationFieldsModelAsync(await _storeContext.GetCurrentStoreAsync());

            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportRegistrationFields.cshtml", model);
        }
    }
}
