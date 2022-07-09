using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Models.Organization;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportTrainingListModel
    {
        public NexportTrainingListModel()
        {
            RedemptionOrganizations = new List<NexportOrganizationModel>();
            Trainings = new Dictionary<Guid, List<NexportTrainingItemModel>>();
        }

        public List<NexportOrganizationModel> RedemptionOrganizations { get; set; }

        public Guid UserId { get; set; }

        public Dictionary<Guid, List<NexportTrainingItemModel>> Trainings { get; set; }
    }
}