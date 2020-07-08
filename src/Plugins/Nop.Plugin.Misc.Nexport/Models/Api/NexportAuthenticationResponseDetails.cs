using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models.Api
{
    public class NexportAuthenticationResponseDetails : NexportApiResponseBase
    {
        public AuthenticationTokenResponse Response { get; set; }
    }
}
