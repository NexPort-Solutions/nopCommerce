using System.Collections.Generic;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportDirectoryResponse : NexportApiResponseBase
    {
        public List<DirectoryResponseItem> DirectoryList { get; set; }
    }
}