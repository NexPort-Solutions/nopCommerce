using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportGetUserResponseDetails : NexportApiResponseBase
    {
        public GetUserResponse Response { get; set; }
    }
}
