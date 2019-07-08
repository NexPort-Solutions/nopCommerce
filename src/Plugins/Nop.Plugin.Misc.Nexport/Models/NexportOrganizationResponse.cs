using System.Collections.Generic;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportOrganizationResponse : NexportApiResponseBase
    {
        public List<OrganizationResponseItem> OrganizationList { get; set; }
    }
}
