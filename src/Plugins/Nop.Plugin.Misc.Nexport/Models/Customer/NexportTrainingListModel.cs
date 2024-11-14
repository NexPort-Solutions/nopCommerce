using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Models.Enrollment;
using Nop.Plugin.Misc.Nexport.Models.Organization;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public class NexportTrainingListModel
{
    public List<NexportOrganizationModel> RedemptionOrganizations { get; set; } = new();

    public Guid UserId { get; set; }

    public Dictionary<Guid, List<NexportTrainingItemModel>> Trainings { get; set; } = new();

    public List<NexportOrganizationModel> Organizations { get; set; } = new();
}