using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportReturnUrlCanonicalizationMiddleware : IMiddleware
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly INexportNavigationContextService _navigationContextService;

    public NexportReturnUrlCanonicalizationMiddleware(
        IUrlHelperFactory urlHelperFactory,
        INexportNavigationContextService navigationContextService)
    {
        _urlHelperFactory = urlHelperFactory;
        _navigationContextService = navigationContextService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!HttpMethods.IsGet(context.Request.Method) ||
            !NexportAuthenticationEndpointClassifier.TryClassify(context, out _))
        {
            await next(context);
            return;
        }

        var actionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor == null)
        {
            await next(context);
            return;
        }

        var actionContext = new ActionContext(
            context,
            new RouteData(context.Request.RouteValues),
            actionDescriptor);
        var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
        var resolution = _navigationContextService.Resolve(context.Request, urlHelper);

        if (resolution.Status is NexportReturnUrlResolutionStatus.Missing or NexportReturnUrlResolutionStatus.Valid)
        {
            await next(context);
            return;
        }

        context.Response.Headers["Cache-Control"] = "no-store";
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        context.Response.Redirect(BuildRedirectUrl(context.Request, resolution.Destination));
    }

    private string BuildRedirectUrl(HttpRequest request, string destination)
    {
        var path = $"{request.PathBase}{request.Path}";
        if (string.IsNullOrEmpty(path))
            path = "/";

        var unrelatedQuery = GetUnrelatedQuery(request.QueryString.Value);
        if (!string.IsNullOrEmpty(unrelatedQuery))
            path += $"?{unrelatedQuery}";

        return string.IsNullOrWhiteSpace(destination)
            ? path
            : QueryHelpers.AddQueryString(path, _navigationContextService.ReturnUrlParameterName, destination);
    }

    private string GetUnrelatedQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
            return string.Empty;

        if (query.StartsWith("?", StringComparison.Ordinal))
            query = query[1..];

        var unrelatedSegments = new List<string>();
        foreach (var segment in query.Split('&', StringSplitOptions.None))
        {
            if (string.IsNullOrEmpty(segment))
                continue;

            var separatorIndex = segment.IndexOf('=');
            var encodedKey = separatorIndex >= 0 ? segment[..separatorIndex] : segment;
            if (!IsReturnUrlKey(encodedKey))
                unrelatedSegments.Add(segment);
        }

        return string.Join('&', unrelatedSegments);
    }

    private bool IsReturnUrlKey(string encodedKey)
    {
        try
        {
            var key = Uri.UnescapeDataString(encodedKey.Replace("+", " ", StringComparison.Ordinal));
            return string.Equals(
                key,
                _navigationContextService.ReturnUrlParameterName,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
}
