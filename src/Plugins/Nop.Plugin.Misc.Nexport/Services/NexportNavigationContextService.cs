using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Nop.Plugin.Misc.Nexport.Services;

public class NexportNavigationContextService : INexportNavigationContextService
{
    public string LoginRouteName => NexportDefaults.NexportLoginRouteName;

    public string RegisterRouteName => NexportDefaults.NexportRegistrationRouteName;

    public string ReturnUrlParameterName => NexportDefaults.DefaultReturnUrlParameterName;

    /// <summary>
    /// Extract and sanitize the return URL query parameter from the current request.
    /// </summary>
    public string GetSanitizedReturnUrl(HttpRequest request, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Query.TryGetValue(ReturnUrlParameterName, out var values) && !StringValues.IsNullOrEmpty(values))
            return Sanitize(values.ToString(), urlHelper);

        return string.Empty;
    }

    /// <summary>
    /// Normalize nested auth redirects and validate the final return URL target.
    /// </summary>
    public string Sanitize(string returnUrl, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(urlHelper);

        if (string.IsNullOrWhiteSpace(returnUrl))
            return string.Empty;

        // Unwrap recursive auth redirects like /login?returnUrl=%2Fregister%3FreturnUrl...
        // until the chain stabilizes or max depth is reached.
        var candidate = returnUrl;
        var maxUnwrapDepth = NexportDefaults.DefaultReturnUrlUnwrapDepth;

        for (var i = 0; i < maxUnwrapDepth; i++)
        {
            if (!TryExtractNestedReturnUrl(candidate, urlHelper, out var nestedReturnUrl))
                break;

            candidate = nestedReturnUrl;
        }

        var maxReturnUrlLength = NexportDefaults.DefaultReturnUrlMaxLength;

        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > maxReturnUrlLength)
            return string.Empty;

        // Keep redirects local and block nested returnUrl payloads/auth-page loops.
        if (!urlHelper.IsLocalUrl(candidate))
            return string.Empty;

        if (ContainsReturnUrlParameter(candidate))
            return string.Empty;

        if (IsAuthPath(candidate, urlHelper))
            return string.Empty;

        return candidate;
    }

    /// <summary>
    /// Create route values that include only a non-empty sanitized return URL.
    /// </summary>
    public RouteValueDictionary CreateRouteValues(string returnUrl)
    {
        var routeValues = new RouteValueDictionary();

        if (!string.IsNullOrWhiteSpace(returnUrl))
            routeValues[ReturnUrlParameterName] = returnUrl;

        return routeValues;
    }

    /// <summary>
    /// Extract the nested return URL only when the source URL is an auth page URL.
    /// </summary>
    protected virtual bool TryExtractNestedReturnUrl(string returnUrl, IUrlHelper urlHelper, out string nestedReturnUrl)
    {
        nestedReturnUrl = null;

        if (string.IsNullOrWhiteSpace(returnUrl))
            return false;

        var queryIndex = returnUrl.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex <= 0 || queryIndex == returnUrl.Length - 1)
            return false;

        var path = returnUrl[..queryIndex];
        if (!IsAuthPath(path, urlHelper))
            return false;

        var query = returnUrl[(queryIndex + 1)..];
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var parsedQuery = QueryHelpers.ParseQuery(query);
        if (!parsedQuery.TryGetValue(ReturnUrlParameterName, out var value) || StringValues.IsNullOrEmpty(value))
            return false;

        nestedReturnUrl = value.ToString();
        return !string.IsNullOrWhiteSpace(nestedReturnUrl);
    }

    /// <summary>
    /// Check whether the URL query contains the return URL parameter.
    /// </summary>
    protected virtual bool ContainsReturnUrlParameter(string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return false;

        var queryIndex = returnUrl.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex <= 0 || queryIndex == returnUrl.Length - 1)
            return false;

        var query = returnUrl[(queryIndex + 1)..];
        var parsedQuery = QueryHelpers.ParseQuery(query);
        return parsedQuery.ContainsKey(ReturnUrlParameterName);
    }

    /// <summary>
    /// Determine whether a path matches one of the auth endpoints.
    /// </summary>
    protected virtual bool IsAuthPath(string path, IUrlHelper urlHelper)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Compare against route-generated auth endpoints to avoid hardcoded path fragments.
        var normalizedPath = NormalizePath(path);
        if (string.IsNullOrWhiteSpace(normalizedPath))
            return false;

        var authPaths = GetAuthPaths(urlHelper);
        return authPaths.Contains(normalizedPath);
    }

    /// <summary>
    /// Build the normalized set of auth endpoint paths from route names.
    /// </summary>
    protected virtual ISet<string> GetAuthPaths(IUrlHelper urlHelper)
    {
        var authPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddAuthPath(authPaths, urlHelper.RouteUrl(LoginRouteName));
        AddAuthPath(authPaths, urlHelper.RouteUrl(RegisterRouteName));

        return authPaths;
    }

    /// <summary>
    /// Add a route path into the auth path set after normalization.
    /// </summary>
    protected virtual void AddAuthPath(ISet<string> authPaths, string routeUrl)
    {
        var path = NormalizePath(routeUrl);
        if (!string.IsNullOrWhiteSpace(path))
            authPaths.Add(path);
    }

    /// <summary>
    /// Convert a relative/absolute URL to a canonical local path for comparisons.
    /// </summary>
    protected virtual string NormalizePath(string pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
            return string.Empty;

        var value = pathOrUrl.Trim();
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            value = uri.AbsolutePath;

        var queryIndex = value.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0)
            value = value[..queryIndex];

        value = value.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(value))
            return "/";

        if (!value.StartsWith("/", StringComparison.Ordinal))
            value = "/" + value;

        return value;
    }
}
