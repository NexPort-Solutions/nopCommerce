using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            viewLocations = context.AreaName == "Admin"
                ? new[] { "/Plugins/Misc.Nexport.DiscountPerCreditHours/Areas/Admin/Views/{1}/{0}.cshtml" }.Concat(viewLocations)
                : new[] { "/Plugins/Misc.Nexport.DiscountPerCreditHours/Views/{1}/{0}.cshtml" }.Concat(viewLocations);

            return viewLocations;
        }
    }
}
