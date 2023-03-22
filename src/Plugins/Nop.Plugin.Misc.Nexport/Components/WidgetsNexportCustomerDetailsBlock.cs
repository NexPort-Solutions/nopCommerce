using System;
using System.Threading.Tasks;
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

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customerModel = (CustomerModel) additionalData;

            if (customerModel.Id == 0)
            {
                var model =
                    await _nexportPluginModelFactory.PrepareAddNexportAdditionalInfoModel(customerModel.ToEntity<Customer>());

                return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/AddNexportCustomerAdditionalInfo.cshtml", model);
            }

            try
            {
                var model =
                    await _nexportPluginModelFactory.PrepareNexportAdditionalInfoModelAsync(customerModel.ToEntity<Customer>());

                var mapping = await _nexportService.FindUserMappingByCustomerId(customerModel.Id);
                if (mapping != null)
                {
                    try
                    {
                        var nexportUser = await _nexportService.GetNexportUserAsync(mapping.NexportUserId)!;

                        if (nexportUser != null)
                        {
                            ViewBag.GotUserFromNexport = true;
                            model.NexportUserFullName = $"{nexportUser.FirstName} {nexportUser.LastName}";
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
                    catch (Exception ex)
                    {
                        ViewBag.GotUserFromNexport = false;
                        var errorMsg = $"Unable to retrieve user information from Nexport for customer {customerModel.Id}";

                        if (ex is ApiException exception)
                        {
                            errorMsg += $" ({exception.Message})";
                        }

                        await _logger.ErrorAsync(errorMsg, ex);
                        _notificationService.ErrorNotification(errorMsg);
                    }
                }

                model.Editable = true;

                return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportCustomerDetails.cshtml", model);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unable to retrieve additional information for customer {customerModel.Id}";

                if (ex is ApiException exception)
                {
                    errorMsg += $" ({exception.Message})";
                }

                await _logger.ErrorAsync(errorMsg, ex);
                _notificationService.ErrorNotification(errorMsg);

                return Content("");
            }
        }
    }
}
