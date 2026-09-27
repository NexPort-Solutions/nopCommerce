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
    public static bool TryClassifyPath(PathString requestPath, out NexportAuthenticationEndpoint endpoint)
    {
        endpoint = default;
        var path = requestPath.Value;
        if (string.IsNullOrEmpty(path))
            return false;
        if (path.EndsWith('/'))
            path = path[..^1];

        if (string.Equals(path, "/" + NexportDefaults.NexportLoginRoutePattern.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(path, "/" + NexportDefaults.NexportLoginCheckoutAsGuestRoutePattern.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase))
        {
            endpoint = NexportAuthenticationEndpoint.LoginCheckoutAsGuest;
            return true;
        }

        if (string.Equals(path, "/" + NexportDefaults.NexportRegistrationRoutePattern.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase))
        {
            endpoint = NexportAuthenticationEndpoint.Register;
            return true;
        }

        return false;
    }

    public static bool TryClassifyPath(HttpContext context, NexportAuthenticationPathPolicy pathPolicy)
    {
        ArgumentNullException.ThrowIfNull(pathPolicy);

        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            return false;

        return pathPolicy.Contains(context.Request.Path);
    }

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