using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsAccountNavigationAfter")]
    public class WidgetsAccountNavigationAfter : NopViewComponent
    {
        public WidgetsAccountNavigationAfter()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/NexportSettingNavigation.cshtml");
        }
    }
}
