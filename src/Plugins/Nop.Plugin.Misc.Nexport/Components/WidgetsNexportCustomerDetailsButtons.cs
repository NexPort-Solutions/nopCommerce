using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportCustomerDetailsButtons")]
    public class WidgetsNexportCustomerDetailsButtons : NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly ILogger _logger;

        public WidgetsNexportCustomerDetailsButtons(
            NexportService nexportService,
            ILogger logger)
        {
            _nexportService = nexportService;
            _logger = logger;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customerModel = (CustomerModel)additionalData;

            if (customerModel == null)
                return Content("");

            if (customerModel.Id == 0)
                return Content("");

            try
            {
                var mapping = _nexportService.FindUserMappingByCustomerId(customerModel.Id);
                if (mapping != null)
                {
                    return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportCustomerDetailsButtons.cshtml", customerModel);
                }

                return Content("");
            }
            catch (Exception ex)
            {
                _logger.Error($"Unable to find Nexport user mapping for customer {customerModel.Id}", ex);

                return Content("");
            }
        }
    }
}