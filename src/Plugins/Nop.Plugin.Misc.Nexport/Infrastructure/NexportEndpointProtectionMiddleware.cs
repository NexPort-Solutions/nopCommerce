using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportEndpointProtectionMiddleware
{
    private readonly RequestDelegate _next;

    public NexportEndpointProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context, NexportRequestProtectionPolicyProvider policyProvider)
    {
        if (!policyProvider.Current.BlockRecognizedCrawlersOnAuthenticationPages)
            return _next(context);

        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            return _next(context);

        if (!NexportAuthenticationEndpointClassifier.TryClassify(context, out _))
            return _next(context);

        var requestEvaluator = context.RequestServices.GetRequiredService<INexportEndpointRequestEvaluator>();
        if (!requestEvaluator.ShouldReject(context))
            return _next(context);

        return NexportRejectedRequestResponse.WriteAsync(context.Response);
    }
}
