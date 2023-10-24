using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Archway.Components.CustomerFields.Control;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public class CustomRender : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(bool renderAdminView, bool isRequired) => (renderAdminView, isRequired) switch
    {
        (true, true) => View("AdminRegistrationRequired"),
        (true, false) => View("AdminRegistrationNotRequired"),
        (false, true) => View("NonAdminRegistrationRequired"),
        (false, false) => View("NonAdminRegistrationNotRequired")
    };
}
