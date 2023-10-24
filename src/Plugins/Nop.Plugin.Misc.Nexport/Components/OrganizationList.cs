using NexportApi.Client;
using Nop.Services.Logging;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.Nexport.Components;

public class OrganizationList : NopViewComponent
{
    private readonly INotificationService _notification;
    private readonly ILogger _logger;
    private readonly IOrganizationService _organization;

    public OrganizationList(
        INotificationService notificationService,
        ILogger logger,
        IOrganizationService organization)
    {
        _notification = notificationService;
        _logger = logger;
        _organization = organization;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var model = new OrganizationListModel { Organizations = await _organization.FindAllOrganizationsUnderRootOrganization() };
            if (model.Organizations.Count < 1)
            {
                return Content(string.Empty);
            }
            return View(model);
        }
        catch (Exception exception)
        {
            var message = "Unable to retrieve the list of NexPort organizations.";
            if (exception is ApiException)
            {
                message += $" ({exception.Message})";
            }
            await _logger.ErrorAsync(message, exception);
            _notification.ErrorNotification(message);
            return Content(string.Empty);
        }
    }
}
