using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.AreaName?.Equals(AreaNames.Admin) ?? false)
            return;

        context.Values[THEME_KEY] = EngineContext.Current.Resolve<IThemeContext>().GetWorkingThemeNameAsync().Result;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        viewLocations = context.AreaName == "Admin" ?
            new[] { "/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/{1}/{0}.cshtml" }.Concat(viewLocations) :
            new[] { "/Plugins/Sale.PurchaseForCustomer/Views/{1}/{0}.cshtml" }.Concat(viewLocations);

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