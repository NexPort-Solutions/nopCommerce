using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale
{
    public class NexportSearchGroupsForPermissionResponse : NexportApiResponseBase
    {
        public List<DirectoryResponseItem>? SearchGroupsForPermissionList { get; set; }
    }
}
