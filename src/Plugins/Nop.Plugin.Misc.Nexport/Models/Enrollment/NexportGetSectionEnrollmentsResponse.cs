using NexportApi.Model;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Enrollment;

public class NexportGetSectionEnrollmentsResponse : NexportApiResponseBase
{
    public List<SectionEnrollmentsResponse> SectionEnrollments { get; set; }
}