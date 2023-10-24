using System.Net;
using NexportApi.Client;

namespace Nop.Plugin.Misc.Nexport.Models.Api;

public record Response<T>(HttpStatusCode StatusCode, int TotalRecord, int RecordPerPage, int CurrentPage, T Data);

public static class ResponseExtensions
{
    public static async Task<Response<T>> ToResponse<T>(this Task<ApiResponse<T>> responseTask)
    {
        var response = await responseTask;
        return new Response<T>(
            response.StatusCode,
            GetHeader(response, "Auth-Total-Count"),
            GetHeader(response, "Auth-Per-Page"),
            GetHeader(response, "Auth-Page"),
            response.Data);
    }

    private static int GetHeader<T>(ApiResponse<T> response, string key)
        => response.Headers.TryGetValue(key, out var totalCounts)
            && int.TryParse(totalCounts.FirstOrDefault(), out var totalCount)
            ? totalCount : default;
}
