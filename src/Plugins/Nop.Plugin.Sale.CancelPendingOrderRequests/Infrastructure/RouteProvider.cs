using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue - 101;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapLocalizedRoute("CancelPendingOrderRequest", "cancellationrequest/{orderId:min(0)}",
                new { controller = "CancelPendingOrderRequests", action = "CancellationRequest" });
        }
    }
}