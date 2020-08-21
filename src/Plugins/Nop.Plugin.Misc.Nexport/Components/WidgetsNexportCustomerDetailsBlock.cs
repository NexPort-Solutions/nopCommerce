using System;
using Microsoft.AspNetCore.Mvc;
using NexportApi.Client;
using Nop.Core.Domain.Customers;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportCustomerDetailsBlock")]
    public class WidgetsNexportCustomerDetailsBlock : NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public WidgetsNexportCustomerDetailsBlock(
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory,
            INotificationService notificationService,
            ILogger logger)
        {
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _notificationService = notificationService;
            _logger = logger;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customerModel = (CustomerModel) additionalData;

            if (customerModel.Id == 0)
                return Content("");

            try
            {
                var model =
                    _nexportPluginModelFactory.PrepareNexportAdditionalInfoModel(customerModel.ToEntity<Customer>());

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
            catch (Exception ex)
            {
                var errorMsg = $"Unable to retrieve user details from Nexport for customer {customerModel.Id}";

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