using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface INexportNavigationContextService
{
    string LoginRouteName { get; }

    string RegisterRouteName { get; }

    string ReturnUrlParameterName { get; }

    /// <summary>
    /// Read and sanitize the configured return URL query parameter from the current request.
    /// </summary>
    string GetSanitizedReturnUrl(HttpRequest request, IUrlHelper urlHelper);

    /// <summary>
    /// Sanitize a raw return URL value (typically an action parameter).
    /// </summary>
    string Sanitize(string returnUrl, IUrlHelper urlHelper);

    /// <summary>
    /// Build route values containing only a sanitized return URL when present.
    /// </summary>
    RouteValueDictionary CreateRouteValues(string returnUrl);
}
