﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
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

            if (customerModel.Id == 0)
                return Content("");

            var model = _nexportPluginModelFactory.PrepareNexportAdditionalInfoModel(customerModel.ToEntity<Customer>());

            var mapping = _nexportService.FindUserMappingByCustomerId(customerModel.Id);
            if (mapping != null)
            {
                var nexportUser = _nexportService.GetNexportUser(mapping.NexportUserId);

                if (nexportUser != null)
                {
                    model.NexportEmail = nexportUser.InternalEmail;
                    if (nexportUser.OwnerOrgId != null)
                    {
                        model.OwnerOrgId = nexportUser.OwnerOrgId;
                    }

                    if (!string.IsNullOrWhiteSpace(nexportUser.OwnerOrgShortName))
                    {
                        model.OwnerOrgShortName = nexportUser.OwnerOrgShortName;
                    }
                }
            }

            model.Editable = true;

            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportCustomerDetails.cshtml", model);
        }
    }
}