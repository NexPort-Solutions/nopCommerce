using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Misc.Nexport;

public static class ViewUtilities
{
    public static string GetControllerName<TController>()
        where TController : Controller
        => typeof(TController).Name.Replace(nameof(Controller), string.Empty);
}

public record JQueryObject(
    [property: JsonProperty("label")] string Label,
    [property: JsonProperty("value")] string Value);
