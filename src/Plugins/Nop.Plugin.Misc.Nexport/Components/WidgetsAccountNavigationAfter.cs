using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsAccountNavigationAfter")]
public class WidgetsAccountNavigationAfter : NopViewComponent
{
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly NexportService _nexportService;

    public WidgetsAccountNavigationAfter(
        IWorkContext workContext,
        NexportService nexportService,
        IStoreContext storeContext)
    {
        _workContext = workContext;
        _nexportService = nexportService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var showNexportWholesalePurchases = await _nexportService.CanAccessWholesalePurchasesAsync(customer, store);

        ViewData["ShowNexportWholesalePurchases"] = showNexportWholesalePurchases;

        return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportSettingNavigation.cshtml");
    }
}
