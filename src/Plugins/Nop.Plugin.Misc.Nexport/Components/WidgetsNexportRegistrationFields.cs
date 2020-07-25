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
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IStoreContext _storeContext;

        public WidgetsNexportRegistrationFields(
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory,
            IStoreContext storeContext)
        {
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _storeContext = storeContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model =
                _nexportPluginModelFactory.PrepareNexportCustomerRegistrationFieldsModel(_storeContext.CurrentStore);

            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportRegistrationFields.cshtml", model);
        }
    }
}
