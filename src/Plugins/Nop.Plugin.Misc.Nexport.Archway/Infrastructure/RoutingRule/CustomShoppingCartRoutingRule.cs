using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure.RoutingRule
{
    public class CustomShoppingCartRoutingRule : IRule
    {
        private readonly TimeSpan _regexTimeout = TimeSpan.FromSeconds(1);
        public Regex InitialMatch { get; }
        public string Replacement { get; }

        public CustomShoppingCartRoutingRule(string replacement)
        {
            if (string.IsNullOrEmpty(replacement))
            {
                throw new ArgumentException(nameof(replacement));
            }

            InitialMatch = new Regex("^cart$", RegexOptions.Compiled | RegexOptions.CultureInvariant, _regexTimeout);
            Replacement = replacement;
        }

        public void ApplyRule(RewriteContext context)
        {
            var path = context.HttpContext.Request.Path;
            var pathBase = context.HttpContext.Request.PathBase;

            var initMatchResults = InitialMatch.Match(path == PathString.Empty ? path.ToString() : path.ToString().Substring(1));

            if (initMatchResults.Success)
            {
                if (context.HttpContext.Request.Method == HttpMethods.Get)
                {
                    var newPath = initMatchResults.Result(Replacement);
                    var response = context.HttpContext.Response;

                    response.StatusCode = (int)HttpStatusCode.Redirect;
                    context.Result = RuleResult.EndResponse;

                    if (string.IsNullOrEmpty(newPath))
                    {
                        response.Headers[HeaderNames.Location] = pathBase.HasValue ? pathBase.Value : "/";
                        return;
                    }

                    if (newPath.IndexOf("://", StringComparison.Ordinal) == -1 && newPath[0] != '/')
                    {
                        newPath = '/' + newPath;
                    }

                    var split = newPath.IndexOf('?');
                    if (split >= 0)
                    {
                        var query = context.HttpContext.Request.QueryString.Add(
                            QueryString.FromUriComponent(
                                newPath.Substring(split)));
                        // not using the HttpContext.Response.redirect here because status codes may be 301, 302, 307, 308
                        response.Headers[HeaderNames.Location] = pathBase + newPath.Substring(0, split) + query.ToUriComponent();
                    }
                    else
                    {
                        response.Headers[HeaderNames.Location] = pathBase + newPath + context.HttpContext.Request.QueryString.ToUriComponent();
                    }
                }
            }
        }
    }
}