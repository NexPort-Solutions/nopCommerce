using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportCreateUserResponseDetails : NexportApiResponseBase
    {
        public CreateUserResponse Response { get; set; }
    }
}
