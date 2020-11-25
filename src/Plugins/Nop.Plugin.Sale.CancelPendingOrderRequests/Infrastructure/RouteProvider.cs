using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue - 101;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("CancelPendingOrderRequest", "cancellationrequest/{orderId:min(0)}",
                new { controller = "CancelPendingOrderRequests", action = "CancellationRequest" });
        }
    }
}