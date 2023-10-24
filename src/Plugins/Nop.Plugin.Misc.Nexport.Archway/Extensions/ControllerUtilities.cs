using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Misc.Nexport.Archway.Extensions;

public static class ControllerUtilities
{
    public static string GetControllerName<TController>()
        where TController : Controller
        => typeof(TController).Name.Replace(nameof(Controller), string.Empty, StringComparison.OrdinalIgnoreCase);
}
