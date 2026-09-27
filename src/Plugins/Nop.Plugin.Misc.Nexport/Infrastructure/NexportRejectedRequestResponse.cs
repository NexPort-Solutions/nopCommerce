using System.Diagnostics.Metrics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

// Writes terminal responses without resolving nopCommerce's SQL logger or rendering an error page.
// An explicit zero content length also prevents empty-response status-code pages from re-executing.
// Metrics use bounded labels and never include request URLs or client identities.
internal static class NexportRejectedRequestResponse
{
    private static readonly Meter RequestProtectionMeter = new("Nop.Plugin.Misc.Nexport.RequestProtection");
    private static readonly Counter<long> RejectedRequests =
        RequestProtectionMeter.CreateCounter<long>("nexport.request_protection.rejected");
    private static readonly string WorkerName =
        Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;

    public static Task WriteAsync(HttpContext context, string reason)
    {
        var response = context.Response;
        response.StatusCode = StatusCodes.Status404NotFound;
        response.ContentLength = 0;
        response.Headers[HeaderNames.CacheControl] = "no-store";
        response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        var policy = reason switch
        {
            "probe" => "probe",
            "unknown-host" => "store-host",
            _ => "authentication"
        };
        RecordRejection(context, policy, reason);

        return Task.CompletedTask;
    }

    public static Task WriteRateLimitedAsync(HttpContext context, RateLimitLease lease, string policy, string reason)
    {
        var retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out var duration)
            ? Math.Max(1, (int)Math.Ceiling(duration.TotalSeconds))
            : 1;
        var response = context.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        response.ContentLength = 0;
        response.Headers.RetryAfter = retryAfter.ToString(CultureInfo.InvariantCulture);
        response.Headers[HeaderNames.CacheControl] = "no-store";
        RecordRejection(context, policy, reason);
        return Task.CompletedTask;
    }

    private static void RecordRejection(HttpContext context, string policy, string reason)
    {
        var method = context.Request.Method.ToUpperInvariant();
        if (method is not ("GET" or "HEAD" or "POST" or "PUT" or "PATCH" or "DELETE" or "OPTIONS" or "TRACE" or "CONNECT"))
            method = "OTHER";

        RejectedRequests.Add(1,
            new KeyValuePair<string, object>("policy", policy),
            new KeyValuePair<string, object>("method", method),
            new KeyValuePair<string, object>("reason", reason),
            new KeyValuePair<string, object>("worker", WorkerName));
    }
}