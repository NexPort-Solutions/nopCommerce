using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.DependencyInjection;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

// Rejects cheap host/path/crawler matches before acquiring permits or invoking SQL-backed middleware.
// Registered before themes and routing; the health-check branch has already run.
internal sealed class NexportRequestPathProtectionMiddleware
{
    private readonly RequestDelegate _next;

    public NexportRequestPathProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context, NexportRequestProtectionPolicyProvider policyProvider,
        NexportRequestAdmission admission, NexportStoreDomainProtection storeDomainProtection)
    {
        if (storeDomainProtection.ShouldReject(context))
            return NexportRejectedRequestResponse.WriteAsync(context, "unknown-host");

        var policy = policyProvider.Current;
        if (policy.BlockKnownProbePaths && policy.RequestPathPolicy.ShouldBlock(context.Request.Path))
            return NexportRejectedRequestResponse.WriteAsync(context, "probe");

        if (policy.BlockRecognizedCrawlersOnAuthenticationPages &&
            NexportAuthenticationEndpointClassifier.TryClassifyPath(context, policy.AuthenticationPathPolicy))
        {
            var requestEvaluator = context.RequestServices.GetRequiredService<INexportEndpointRequestEvaluator>();
            if (requestEvaluator.ShouldReject(context))
                return NexportRejectedRequestResponse.WriteAsync(context, "crawler");
        }

        return admission.InvokeAsync(context, _next);
    }
}