using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportAuthenticationResponseDetails : NexportApiResponseBase
    {
        public AuthenticationTokenResponse Response { get; set; }
    }
}
