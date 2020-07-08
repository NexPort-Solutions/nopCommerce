using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Organization
{
    public class NexportDirectoryResponse : NexportApiResponseBase
    {
        public List<DirectoryResponseItem> DirectoryList { get; set; }
    }
}