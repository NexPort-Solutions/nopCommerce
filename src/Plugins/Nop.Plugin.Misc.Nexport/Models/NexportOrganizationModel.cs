using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportOrganizationModel : BaseNopModel
    {
        public Guid OrgId { get; set; }

        public string OrgShortName { get; set; }

        public string OrgName { get; set; }
    }
}
