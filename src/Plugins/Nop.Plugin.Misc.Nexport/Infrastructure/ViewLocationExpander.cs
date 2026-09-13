using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Services.Themes;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
            return;

        var httpContext = context.ActionContext.HttpContext;
        if (!httpContext.Items.TryGetValue(NopThemeDefaults.HttpContextThemeCacheKey, out var cachedThemeName))
            return;

        context.Values[NopThemeDefaults.ThemeKey] = (string)cachedThemeName;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        viewLocations = new[] {
            "/Plugins/Misc.Nexport/Views/{1}/{0}.cshtml",
            "/Plugins/Misc.Nexport/Areas/Admin/Views/{1}/{0}.cshtml"
        }.Concat(viewLocations);

        if (context.Values.TryGetValue(NopThemeDefaults.ThemeKey, out var theme))
        {
            viewLocations = new[] {
                $"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
            }.Concat(viewLocations);
        }

        return viewLocations;
    }
}
