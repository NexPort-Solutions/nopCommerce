using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            viewLocations = context.AreaName == "Admin"
                ? new[] { "/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/{1}/{0}.cshtml" }.Concat(viewLocations)
                : new[] { "/Plugins/Misc.Nexport.Archway/Views/{1}/{0}.cshtml" }.Concat(viewLocations);

            return viewLocations;
        }
    }
}
