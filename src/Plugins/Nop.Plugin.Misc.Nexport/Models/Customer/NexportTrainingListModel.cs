using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Models.Organization;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportTrainingListModel
    {
        public IList<NexportOrganizationModel> RedemptionOrganizations { get; set; }

        public Guid UserId { get; set; }
    }
}
