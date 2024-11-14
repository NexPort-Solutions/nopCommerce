using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Enrollment;

public class NexportEnrollmentsResponse : NexportApiResponseBase
{
    public List<ApiEnrollmentItem> EnrollmentList { get; set; }
}