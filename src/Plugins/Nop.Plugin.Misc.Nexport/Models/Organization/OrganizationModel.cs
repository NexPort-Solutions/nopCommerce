using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Organization;

public record OrganizationModel : BaseNopModel
{
    public required Guid OrgId { get; set; }
    public required string? OrgShortName { get; set; }
    public required string? OrgName { get; set; }
    public required SubscriptionResponse? Subscription { get; set; }
}
