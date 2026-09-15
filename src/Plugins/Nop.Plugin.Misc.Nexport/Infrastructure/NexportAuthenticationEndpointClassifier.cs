using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nop.Plugin.Misc.Nexport.Controllers;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal enum NexportAuthenticationEndpoint
{
    Login,
    LoginCheckoutAsGuest,
    Register
}

internal static class NexportAuthenticationEndpointClassifier
{
    public static bool TryClassify(HttpContext context, out NexportAuthenticationEndpoint endpoint)
    {
        endpoint = default;

        var actionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor?.ControllerTypeInfo.AsType() != typeof(NexportCustomerController))
            return false;

        if (actionDescriptor.ActionName == nameof(NexportCustomerController.Register))
        {
            endpoint = NexportAuthenticationEndpoint.Register;
            return true;
        }

        if (actionDescriptor.ActionName != nameof(NexportCustomerController.Login))
            return false;

        endpoint = IsCheckoutAsGuest(context)
            ? NexportAuthenticationEndpoint.LoginCheckoutAsGuest
            : NexportAuthenticationEndpoint.Login;
        return true;
    }

    private static bool IsCheckoutAsGuest(HttpContext context)
    {
        if (!context.Request.RouteValues.TryGetValue("checkoutAsGuest", out var value))
            return false;

        return value is bool boolValue && boolValue ||
            string.Equals(value?.ToString(), bool.TrueString, StringComparison.OrdinalIgnoreCase);
    }
}
