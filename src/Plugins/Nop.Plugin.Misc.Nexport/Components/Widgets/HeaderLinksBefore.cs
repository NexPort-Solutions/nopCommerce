using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets;

public class HeaderLinksBefore : NopViewComponent
{
    private readonly ICustomerService _customer;
    private readonly IWorkContext _workContext;

    public HeaderLinksBefore(
        ICustomerService customerService,
        IWorkContext workContext)
    {
        _customer = customerService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object __)
    {
        if (!await _customer.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
        {
            return Content(string.Empty);
        }
        return View();
    }
}
