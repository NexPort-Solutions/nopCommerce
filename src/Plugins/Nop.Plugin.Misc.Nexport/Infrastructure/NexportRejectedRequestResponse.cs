using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal static class NexportRejectedRequestResponse
{
    public static Task WriteAsync(HttpResponse response)
    {
        response.StatusCode = StatusCodes.Status404NotFound;
        response.ContentLength = 0;
        response.Headers[HeaderNames.CacheControl] = "no-store";
        response.Headers["X-Robots-Tag"] = "noindex, nofollow";

        return Task.CompletedTask;
    }
}