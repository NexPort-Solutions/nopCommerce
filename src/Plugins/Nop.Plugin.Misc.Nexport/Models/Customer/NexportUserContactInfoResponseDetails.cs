using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportUserContactInfoResponseDetails : NexportApiResponseBase
    {
        public UserContactInfoResponse Response { get; set; }
    }
}
