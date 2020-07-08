using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportUserListResponse : NexportApiResponseBase
    {
        public List<GetUserResponse> UserList { get; set; }
    }
}
