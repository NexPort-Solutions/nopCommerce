using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.DiscountPerCreditHours.Configure",
                "Admin/NexportDiscountPerCreditHours/Configure",
                new { controller = "NexportDiscountPerCreditHoursController", action = "Configure" });
        }

        public int Priority => int.MaxValue - 99;
    }
}
