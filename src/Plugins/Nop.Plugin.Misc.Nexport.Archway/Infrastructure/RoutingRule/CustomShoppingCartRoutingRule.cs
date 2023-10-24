using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure.RoutingRule;

public class CustomShoppingCartRoutingRule : IRule
{
    private static readonly TimeSpan _regexTimeout = TimeSpan.FromSeconds(1);
    public static Regex Initial { get; } = new("^cart$", RegexOptions.Compiled | RegexOptions.CultureInvariant, _regexTimeout);
    public string Replacement { get; }

    public CustomShoppingCartRoutingRule(string replacement) => Replacement = replacement;

    // This is suspicious. Why not use built-in Url parsing functionality?
    public void ApplyRule(RewriteContext context)
    {
        var path = context.HttpContext.Request.Path;
        var pathBase = context.HttpContext.Request.PathBase;
        var initMatchResults = Initial.Match(path == PathString.Empty ? path.ToString() : path.ToString()[1..]);

        if (!initMatchResults.Success || !string.Equals(context.HttpContext.Request.Method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var newPath = initMatchResults.Result(Replacement);
        var response = context.HttpContext.Response;
        response.StatusCode = (int)HttpStatusCode.Redirect;
        context.Result = RuleResult.EndResponse;
        if (string.IsNullOrEmpty(newPath))
        {
            response.Headers[HeaderNames.Location] = pathBase.HasValue ? pathBase.Value : "/";
            return;
        }
        if (!newPath.Contains("://", StringComparison.OrdinalIgnoreCase) && newPath[0] != '/')
        {
            newPath = '/' + newPath;
        }
        var split = newPath.IndexOf('?', StringComparison.OrdinalIgnoreCase);
        if (split >= 0)
        {
            var query = context.HttpContext.Request.QueryString.Add(QueryString.FromUriComponent(newPath[split..]));
            // not using the HttpContext.Response.redirect here because status codes may be 301, 302, 307, 308
            response.Headers[HeaderNames.Location] = pathBase + newPath[..split] + query.ToUriComponent();
        }
        else
        {
            response.Headers[HeaderNames.Location] = pathBase + newPath + context.HttpContext.Request.QueryString.ToUriComponent();
        }
    }
}
