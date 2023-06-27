using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsNexportOrderListButtons")]
public class WidgetsNexportOrderListButtons : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Order/Wholesale.cshtml");
    }
}