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

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                return Content("");

            return View("~/Plugins/Misc.Nexport/Views/Widget/WidgetsHeaderLinksBefore.cshtml");
        }
    }
}
