using Nop.Plugin.Misc.Nexport.Models.Organization;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public class TrainingListModel
{
    public IList<OrganizationModel> RedemptionOrganizations { get; init; } = new List<OrganizationModel>();
    public Guid UserId { get; set; }
    public IDictionary<Guid, List<TrainingItemModel>> Trainings { get; init; } = new Dictionary<Guid, List<TrainingItemModel>>();
}
