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
            ViewData["CustomerName"] = $"{customer.FirstName} {customer.LastName}'s";
            var ordersForCustomer = await _nexportService.GetOrdersForCustomer(customer);
            if (ordersForCustomer != null)
            {
                foreach (var order in ordersForCustomer)
                {
                    var orderInfo = await _nexportService.GetWholesaleOrderInfoForOrderAsync(order.Id);
                    if (orderInfo != null)
                    {
                        ViewData["ShowNexportWholesalePurchases"] = true;
                        break;
                    }
                }
            }

            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportSettingNavigation.cshtml");
        }
    }
}
