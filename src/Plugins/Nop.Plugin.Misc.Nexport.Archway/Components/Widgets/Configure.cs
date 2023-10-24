using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Archway.Components.Widgets;

public class Configure : NopViewComponent
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<IViewComponentResult> InvokeAsync(string _, object __) => View("~/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/Customer/Configure.cshtml");
#pragma warning restore CS1998
}
