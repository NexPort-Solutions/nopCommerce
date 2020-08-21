using System;
using Microsoft.AspNetCore.Mvc;
using NexportApi.Client;
using Nop.Services.Logging;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.Nexport.Components
{
    public class NexportOrganizationListViewComponent : NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public NexportOrganizationListViewComponent(NexportService nexportService,
            INotificationService notificationService,
            ILogger logger)
        {
            _nexportService = nexportService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public IViewComponentResult Invoke()
        {
            try
            {
                var model = new NexportOrganizationListModel
                {
                    Organizations = _nexportService.FindAllOrganizationsUnderRootOrganization()
                };

                if (model.Organizations.Count < 1)
                    return Content("");

                return View("~/Plugins/Misc.Nexport/Views/Shared/Components/NexportOrganizationList/Default.cshtml", model);
            }
            catch (Exception ex)
            {
                var errorMsg = "Unable to retrieve the list of Nexport organizations.";

                if (ex is ApiException exception)
                {
                    errorMsg += $" ({exception.Message})";
                }

                _logger.Error(errorMsg, ex);
                _notificationService.ErrorNotification(errorMsg);

                return Content("");
            }
        }
    }
}
