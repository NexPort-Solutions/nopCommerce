using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsHeaderLinksBefore")]
    public class WidgetsHeaderLinksBefore : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        public WidgetsHeaderLinksBefore(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Content("");

            return View("~/Plugins/Misc.Nexport/Views/Widget/WidgetsHeaderLinksBefore.cshtml");
        }
    }
}
