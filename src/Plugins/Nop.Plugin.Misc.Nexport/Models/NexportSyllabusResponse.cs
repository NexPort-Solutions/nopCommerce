using System.Collections.Generic;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportSyllabusResponse : NexportApiResponseBase
    {
        public List<GetSyllabiResponseItem> SyllabusList { get; set; }
    }
}
