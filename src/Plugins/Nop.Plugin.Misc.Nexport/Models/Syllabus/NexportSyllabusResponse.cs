using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Syllabus
{
    public class NexportSyllabusResponse : NexportApiResponseBase
    {
        public List<GetSyllabiResponseItem> SyllabusList { get; set; }
    }
}
