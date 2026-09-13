using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Services.Themes;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Infrastructure;

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
        viewLocations = context.AreaName == "Admin"
            ? new[] { "/Plugins/Misc.Nexport.DiscountPerCreditHours/Areas/Admin/Views/{1}/{0}.cshtml" }.Concat(viewLocations)
            : new[] { "/Plugins/Misc.Nexport.DiscountPerCreditHours/Views/{1}/{0}.cshtml" }.Concat(viewLocations);

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
