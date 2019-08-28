using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportCustomerDetailsBlock")]
    public class WidgetsNexportCustomerDetailsBlock : NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;

        public WidgetsNexportCustomerDetailsBlock(
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory)
        {
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customerModel = (CustomerModel) additionalData;
            var mapping = _nexportService.FindUserMappingByCustomerId(customerModel.Id);
            if (mapping == null)
                return Content("");

            var nexportUser = _nexportService.GetNexportUser(mapping.NexportUserId);

            if (nexportUser == null)
                return Content("");

            var model = _nexportPluginModelFactory.PrepareNexportUserMappingModel(customerModel.ToEntity<Customer>());

            model.NexportEmail = nexportUser.Email;
            if (!string.IsNullOrWhiteSpace(nexportUser.OwnerOrgId))
            {
                model.OwnerOrgId = Guid.Parse(nexportUser.OwnerOrgId);
            }

            if (!string.IsNullOrWhiteSpace(nexportUser.OwnerOrgShortName))
            {
                model.OwnerOrgShortName = nexportUser.OwnerOrgShortName;
            }

            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportCustomerDetails.cshtml", model);
        }
    }
}