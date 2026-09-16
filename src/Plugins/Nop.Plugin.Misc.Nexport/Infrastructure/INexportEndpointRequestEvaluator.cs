using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal interface INexportEndpointRequestEvaluator
{
    bool ShouldReject(HttpContext context);
}
