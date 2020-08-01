using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components
{
    public class NexportOrganizationListViewComponent : NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly ILogger _logger;

        public NexportOrganizationListViewComponent(NexportService nexportService,
            ILogger logger)
        {
            _nexportService = nexportService;
            _logger = logger;
        }

        public IViewComponentResult Invoke()
        {
            var model = new NexportOrganizationListModel
            {
                Organizations = _nexportService.FindAllOrganizationsUnderRootOrganization()
            };

            if (model.Organizations.Count < 1)
            {
                return Content(string.Empty);
            }

            return View("~/Plugins/Misc.Nexport/Views/Shared/Components/NexportOrganizationList/Default.cshtml", model);
        }
    }
}
