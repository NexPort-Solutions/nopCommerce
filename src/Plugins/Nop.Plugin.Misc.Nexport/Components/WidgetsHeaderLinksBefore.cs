using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsHeaderLinksBefore")]
    public class WidgetsHeaderLinksBefore : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public WidgetsHeaderLinksBefore(
            ICustomerService customerService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Content("");

            return View("~/Plugins/Misc.Nexport/Views/Widget/WidgetsHeaderLinksBefore.cshtml");
        }
    }
}
