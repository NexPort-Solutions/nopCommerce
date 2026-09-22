using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.DependencyInjection;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportRequestPathProtectionMiddleware
{
    private readonly RequestDelegate _next;

    public NexportRequestPathProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context, NexportRequestProtectionPolicyProvider policyProvider)
    {
        var policy = policyProvider.Current;
        if (policy.BlockKnownProbePaths && policy.RequestPathPolicy.ShouldBlock(context.Request.Path))
            return NexportRejectedRequestResponse.WriteAsync(context.Response);

        if (policy.BlockRecognizedCrawlersOnAuthenticationPages &&
            NexportAuthenticationEndpointClassifier.TryClassifyPath(context, policy.AuthenticationPathPolicy))
        {
            var requestEvaluator = context.RequestServices.GetRequiredService<INexportEndpointRequestEvaluator>();
            if (requestEvaluator.ShouldReject(context))
                return NexportRejectedRequestResponse.WriteAsync(context.Response);
        }

        return _next(context);
    }
}
