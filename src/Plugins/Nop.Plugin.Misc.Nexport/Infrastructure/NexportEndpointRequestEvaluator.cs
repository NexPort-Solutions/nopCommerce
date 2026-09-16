using Microsoft.AspNetCore.Http;
using Nop.Services.Helpers;
using UaDetector.Parsers;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportEndpointRequestEvaluator : INexportEndpointRequestEvaluator
{
    private readonly IBotParser _botParser;
    private readonly IUserAgentHelper _userAgentHelper;

    public NexportEndpointRequestEvaluator(IUserAgentHelper userAgentHelper, IBotParser botParser)
    {
        _userAgentHelper = userAgentHelper;
        _botParser = botParser;
    }

    public bool ShouldReject(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
            return false;

        try
        {
            if (_userAgentHelper.IsSearchEngine())
                return true;
        }
        catch
        {
            // Each detector fails open so the other detector can still classify the request.
        }

        try
        {
            return _botParser.IsBot(userAgent);
        }
        catch
        {
            return false;
        }
    }
}
