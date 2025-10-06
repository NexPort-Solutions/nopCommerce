using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
            return;

        context.Values[THEME_KEY] = EngineContext.Current.Resolve<IThemeContext>().GetWorkingThemeNameAsync().Result;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        viewLocations = context.AreaName == "Admin"
            ? new[] { "/Plugins/Misc.Nexport.DiscountPerCreditHours/Areas/Admin/Views/{1}/{0}.cshtml" }.Concat(viewLocations)
            : new[] { "/Plugins/Misc.Nexport.DiscountPerCreditHours/Views/{1}/{0}.cshtml" }.Concat(viewLocations);

        if (context.Values.TryGetValue(THEME_KEY, out var theme))
        {
            viewLocations = new[] {
                $"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
            }.Concat(viewLocations);
        }

        return viewLocations;
    }
}