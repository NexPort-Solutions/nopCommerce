using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Misc.Nexport.Configuration;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

// Shares authentication rate/concurrency budgets between early path checks and the routed fallback.
// All budgets are in memory and local to this app instance; hosts do not create separate allowances.
internal sealed class NexportRequestAdmission : IDisposable
{
    private readonly NexportRequestProtectionConfig _settings;
    private readonly PartitionedRateLimiter<AdmissionRequest> _rateLimiter;
    private readonly PartitionedRateLimiter<AdmissionRequest> _concurrencyLimiter;

    public NexportRequestAdmission(NexportRequestProtectionConfig settings)
    {
        _settings = settings;
        _rateLimiter = PartitionedRateLimiter.Create<AdmissionRequest, string>(request =>
            RateLimitPartition.GetSlidingWindowLimiter(
                $"{request.Policy}:{request.ClientAddress}",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = request.Settings.RequestsPerWindow,
                    Window = TimeSpan.FromSeconds(settings.RateLimitWindowSeconds),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        _concurrencyLimiter = PartitionedRateLimiter.Create<AdmissionRequest, string>(request =>
            RateLimitPartition.GetConcurrencyLimiter(
                request.Policy,
                _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = request.Settings.MaxConcurrentRequests,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    }

    // Admits once per HTTP request, rejecting immediately when either budget is exhausted.
    // The early call uses only the path; the fallback uses controller metadata for alternate routes.
    public async Task InvokeAsync(HttpContext context, RequestDelegate next, bool useEndpointMetadata = false)
    {
        // Retain this marker across error re-execution; leases belong to the original invocation.
        if (context.Features.Get<AdmissionFeature>() != null)
        {
            await next(context);
            return;
        }

        NexportAuthenticationEndpoint endpoint;
        var classified = useEndpointMetadata
            ? NexportAuthenticationEndpointClassifier.TryClassify(context, out endpoint)
            : NexportAuthenticationEndpointClassifier.TryClassifyPath(context.Request.Path, out endpoint);
        var request = classified ? GetAdmissionRequest(context, endpoint) : null;
        if (request == null)
        {
            await next(context);
            return;
        }

        context.RequestAborted.ThrowIfCancellationRequested();
        // A request rejected by concurrency still counts against its rate budget. It must not be
        // possible to hammer a busy endpoint indefinitely without consuming the client's allowance.
        using var rateLease = _rateLimiter.AttemptAcquire(request);
        if (!rateLease.IsAcquired)
        {
            await NexportRejectedRequestResponse.WriteRateLimitedAsync(context, rateLease, request.Policy, "rate");
            return;
        }

        using var concurrencyLease = _concurrencyLimiter.AttemptAcquire(request);
        if (!concurrencyLease.IsAcquired)
        {
            await NexportRejectedRequestResponse.WriteRateLimitedAsync(
                context, concurrencyLease, request.Policy, "concurrency");
            return;
        }

        context.Features.Set(AdmissionFeature.Instance);
        // Using declarations release permits even when downstream processing throws or is cancelled.
        await next(context);
    }

    private AdmissionRequest GetAdmissionRequest(HttpContext context, NexportAuthenticationEndpoint endpoint)
    {
        var isPage = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
        if (!isPage && !HttpMethods.IsPost(context.Request.Method))
            return null;

        // Use the address resolved by trusted proxy middleware, never an arbitrary forwarded header.
        var clientAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return (endpoint, isPage) switch
        {
            (NexportAuthenticationEndpoint.Login or NexportAuthenticationEndpoint.LoginCheckoutAsGuest, true) =>
                new("login-page", clientAddress, _settings.LoginPageEndpoint),
            (NexportAuthenticationEndpoint.Register, true) =>
                new("registration-page", clientAddress, _settings.RegistrationPageEndpoint),
            (NexportAuthenticationEndpoint.Login or NexportAuthenticationEndpoint.LoginCheckoutAsGuest, false) =>
                new("login-submission", clientAddress, _settings.LoginSubmissionEndpoint),
            (NexportAuthenticationEndpoint.Register, false) =>
                new("registration-submission", clientAddress, _settings.RegistrationSubmissionEndpoint),
            _ => null
        };
    }

    public void Dispose()
    {
        _rateLimiter.Dispose();
        _concurrencyLimiter.Dispose();
    }

    private sealed record AdmissionRequest(
        string Policy, string ClientAddress, NexportEndpointProtectionConfig Settings);

    private sealed class AdmissionFeature
    {
        public static AdmissionFeature Instance { get; } = new();
    }
}