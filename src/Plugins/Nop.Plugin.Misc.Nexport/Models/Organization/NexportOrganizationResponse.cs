using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Organization
{
    public class NexportOrganizationResponse : NexportApiResponseBase
    {
        public List<OrganizationResponseItem> OrganizationList { get; set; }
    }
}
