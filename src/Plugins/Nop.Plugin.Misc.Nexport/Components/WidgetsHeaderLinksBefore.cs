using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsHeaderLinksBefore")]
public class WidgetsHeaderLinksBefore : NopViewComponent
{
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly NexportService _nexportService;

    public WidgetsHeaderLinksBefore(
        ICustomerService customerService,
        IWorkContext workContext,
        NexportService nexportService,
        IStoreContext storeContext)
    {
        _customerService = customerService;
        _workContext = workContext;
        _nexportService = nexportService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Content("");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        ViewData["CustomerName"] = $"{customer.FirstName} {customer.LastName}";

        var showNexportWholesalePurchases = await _nexportService.CanAccessWholesalePurchasesAsync(customer, store);

        ViewData["ShowNexportWholesalePurchases"] = showNexportWholesalePurchases;

        return View("~/Plugins/Misc.Nexport/Views/Widget/WidgetsHeaderLinksBefore.cshtml");
    }
}
