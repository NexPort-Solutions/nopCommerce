using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Misc.Nexport.Extensions;

public static class ViewUtilities
{
    public static string GetControllerName<TController>()
        where TController : Controller
        => typeof(TController).Name[..typeof(TController).Name.LastIndexOf(nameof(Controller), StringComparison.OrdinalIgnoreCase)];
}
