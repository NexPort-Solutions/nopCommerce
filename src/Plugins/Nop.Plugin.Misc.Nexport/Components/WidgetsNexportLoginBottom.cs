using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = nameof(WidgetsNexportLoginBottom))]
public class WidgetsNexportLoginBottom : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        return await ViewAsync("~/Plugins/Misc.Nexport/Views/Widget/WidgetsNexportLoginBottom.cshtml");
    }
}
