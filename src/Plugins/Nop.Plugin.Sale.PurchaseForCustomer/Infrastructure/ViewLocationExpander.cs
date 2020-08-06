using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            viewLocations = context.AreaName == "Admin" ?
                new[] { "/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/{1}/{0}.cshtml" }.Concat(viewLocations) :
                new[] { "/Plugins/Sale.PurchaseForCustomer/Views/{1}/{0}.cshtml" }.Concat(viewLocations);

            return viewLocations;
        }
    }
}
