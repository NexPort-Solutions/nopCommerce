using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsAccountNavigationAfter")]
    public class WidgetsAccountNavigationAfter : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly NexportService _nexportService;

        public WidgetsAccountNavigationAfter(IWorkContext workContext,NexportService nexportService)
        {
            _workContext = workContext;
            _nexportService = nexportService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if(await _nexportService.HasWholesaleOrders(customer))
                ViewData["ShowNexportWholesalePurchases"] = true;

            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportSettingNavigation.cshtml");
        }
    }
}
