using System.Collections.Generic;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportUserListResponse : NexportApiResponseBase
    {
        public List<GetUserResponse> UserList { get; set; }
    }
}
