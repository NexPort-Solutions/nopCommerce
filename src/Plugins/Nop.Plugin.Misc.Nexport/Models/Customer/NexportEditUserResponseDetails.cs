using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportEditUserResponseDetails : NexportApiResponseBase
    {
        public EditUserResponse Response { get; set; }
    }
}