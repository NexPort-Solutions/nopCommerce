using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
        return Resolve(request, urlHelper).Destination ?? string.Empty;
    }

    /// <summary>
    /// Resolve the return URL query parameter from the current request.
    /// </summary>
    public NexportReturnUrlResolutionResult Resolve(HttpRequest request, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(urlHelper);

        var query = request.QueryString.Value ?? string.Empty;
        if (query.StartsWith("?", StringComparison.Ordinal))
            query = query[1..];

        if (!TryGetSingleReturnUrlParameter(query, out var returnUrl, out var hasReturnUrl))
            return new NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus.Rejected);

        if (!hasReturnUrl)
            return new NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus.Missing);

        return ResolveCore(returnUrl, urlHelper);
    }

    /// <summary>
    /// Resolve a raw return URL value, typically an action parameter.
    /// </summary>
    public NexportReturnUrlResolutionResult Resolve(string returnUrl, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(urlHelper);

        if (string.IsNullOrWhiteSpace(returnUrl))
            return new NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus.Missing);

        return ResolveCore(returnUrl, urlHelper);
    }

    /// <summary>
    /// Sanitize a raw return URL value while retaining controller and POST compatibility.
    /// </summary>
    public string Sanitize(string returnUrl, IUrlHelper urlHelper)
    {
        return Resolve(returnUrl, urlHelper).Destination ?? string.Empty;
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
    /// Extract and resolve nested return URLs until the chain stabilizes or reaches its maximum depth.
    /// </summary>
    private NexportReturnUrlResolutionResult ResolveCore(string returnUrl, IUrlHelper urlHelper)
    {
        if (returnUrl.Length > NexportDefaults.DefaultEncodedReturnUrlMaxLength)
            return new NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus.Rejected);

        var candidate = returnUrl;
        var canonicalized = false;

        for (var depth = 0; depth < NexportDefaults.DefaultReturnUrlUnwrapDepth; depth++)
        {
            if (!TryExtractNestedReturnUrl(candidate, urlHelper, out var nestedReturnUrl))
                break;

            candidate = nestedReturnUrl;
            canonicalized = true;

            if (candidate.Length > NexportDefaults.DefaultEncodedReturnUrlMaxLength)
                return new NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus.Rejected);
        }

        if (!TryValidateFinalReturnUrl(candidate, urlHelper))
            return new NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus.Rejected);

        return new NexportReturnUrlResolutionResult(
            canonicalized
                ? NexportReturnUrlResolutionStatus.Canonicalized
                : NexportReturnUrlResolutionStatus.Valid,
            candidate);
    }

    /// <summary>
    /// Extract the nested return URL only when the source URL is a local auth page URL.
    /// </summary>
    protected virtual bool TryExtractNestedReturnUrl(string returnUrl, IUrlHelper urlHelper, out string nestedReturnUrl)
    {
        nestedReturnUrl = null;

        if (string.IsNullOrWhiteSpace(returnUrl) ||
            returnUrl.Length > NexportDefaults.DefaultEncodedReturnUrlMaxLength ||
            ContainsControlCharacters(returnUrl) ||
            returnUrl.IndexOf('\\') >= 0 ||
            !HasValidPercentEncoding(returnUrl))
        {
            return false;
        }

        var queryIndex = returnUrl.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex <= 0 || queryIndex == returnUrl.Length - 1)
            return false;

        // Validate the source before using its path for endpoint classification. This keeps an absolute
        // URL from being normalized into a path that happens to match one of the auth endpoints.
        if (!urlHelper.IsLocalUrl(returnUrl))
            return false;

        var path = returnUrl[..queryIndex];
        if (!IsAuthPath(path, urlHelper))
            return false;

        var query = returnUrl[(queryIndex + 1)..];
        if (!TryGetSingleReturnUrlParameter(query, out nestedReturnUrl, out var hasReturnUrl) ||
            !hasReturnUrl ||
            string.IsNullOrWhiteSpace(nestedReturnUrl))
        {
            nestedReturnUrl = null;
            return false;
        }

        return true;
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
        return !TryGetSingleReturnUrlParameter(query, out _, out var hasReturnUrl) || hasReturnUrl;
    }

    /// <summary>
    /// Determine whether a final return URL is safe and no longer points to an auth page.
    /// </summary>
    private bool TryValidateFinalReturnUrl(string returnUrl, IUrlHelper urlHelper)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            returnUrl.Length > NexportDefaults.DefaultReturnUrlMaxLength ||
            ContainsControlCharacters(returnUrl) ||
            returnUrl.IndexOf('\\') >= 0 ||
            !HasValidPercentEncoding(returnUrl) ||
            !urlHelper.IsLocalUrl(returnUrl))
        {
            return false;
        }

        var queryIndex = returnUrl.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0 && ContainsReturnUrlParameter(returnUrl))
            return false;

        return !IsAuthPath(returnUrl, urlHelper);
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
        AddAuthPath(authPaths, urlHelper.RouteUrl(NexportDefaults.NexportLoginCheckoutAsGuestRouteName));
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

        try
        {
            value = Uri.UnescapeDataString(value);
        }
        catch (UriFormatException)
        {
            return string.Empty;
        }

        value = value.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(value))
            return "/";

        if (!value.StartsWith("/", StringComparison.Ordinal))
            value = "/" + value;

        return value;
    }

    /// <summary>
    /// Read exactly one case-insensitive return URL parameter from a raw query string.
    /// </summary>
    private bool TryGetSingleReturnUrlParameter(
        string query,
        out string returnUrl,
        out bool hasReturnUrl)
    {
        returnUrl = null;
        hasReturnUrl = false;

        if (string.IsNullOrEmpty(query))
            return true;

        if (query.StartsWith("?", StringComparison.Ordinal))
            query = query[1..];

        var values = new List<string>();
        foreach (var segment in query.Split('&', StringSplitOptions.None))
        {
            if (string.IsNullOrEmpty(segment))
                continue;

            var separatorIndex = segment.IndexOf('=');
            var encodedKey = separatorIndex >= 0 ? segment[..separatorIndex] : segment;
            if (!TryDecodeQueryComponent(encodedKey, out var key) ||
                !string.Equals(key, ReturnUrlParameterName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            hasReturnUrl = true;
            var encodedValue = separatorIndex >= 0 ? segment[(separatorIndex + 1)..] : string.Empty;
            if (encodedValue.Length > NexportDefaults.DefaultEncodedReturnUrlMaxLength ||
                !TryDecodeQueryComponent(encodedValue, out var value))
            {
                return false;
            }

            values.Add(value);
        }

        if (!hasReturnUrl)
            return true;

        if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]))
            return false;

        returnUrl = values[0];
        return true;
    }

    private static bool TryDecodeQueryComponent(string encodedValue, out string value)
    {
        value = null;

        if (!HasValidPercentEncoding(encodedValue))
            return false;

        try
        {
            value = Uri.UnescapeDataString(encodedValue.Replace("+", " ", StringComparison.Ordinal));
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private static bool HasValidPercentEncoding(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '%')
                continue;

            if (index + 2 >= value.Length ||
                !IsHexDigit(value[index + 1]) ||
                !IsHexDigit(value[index + 2]))
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static bool IsHexDigit(char value)
    {
        return value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
    }

    private static bool ContainsControlCharacters(string value)
    {
        foreach (var character in value)
        {
            if (char.IsControl(character))
                return true;
        }

        return false;
    }
}
