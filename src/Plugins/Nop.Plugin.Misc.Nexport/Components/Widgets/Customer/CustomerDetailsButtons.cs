using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Customer;

public class CustomerDetailsButtons : NopViewComponent
{
    private readonly ILogger _logger;
    private readonly IUserMappingService _userMapping;

    public CustomerDetailsButtons(ILogger logger, IUserMappingService userMapping)
    {
        _logger = logger;
        _userMapping = userMapping;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (additionalData is not CustomerModel { Id: not default(int) } customerModel)
        {
            return Content(string.Empty);
        }

        try
        {
            if (await _userMapping.FindByCustomerId(customerModel.Id) is not { } mapping)
            {
                return Content(string.Empty);
            }

            return View(customerModel);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync($"Unable to find NexPort user mapping for customer {customerModel.Id}", exception);
            return Content(string.Empty);
        }
    }
}
