using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Components
{
    [ViewComponent(Name = "WidgetsOrderWholesale")]
    public class WidgetsOrderWholesale : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            //C:\sandbox\nopCommerce\src\Plugins\Nop.Plugin.Misc.Nexport\Areas\Admin\Views\Widget\Order\Wholesale.cshtml
            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Order/Wholesale.cshtml");
        }
    }
}
