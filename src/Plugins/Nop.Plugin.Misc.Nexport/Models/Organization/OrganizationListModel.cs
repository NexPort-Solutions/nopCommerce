using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Organization;

public record OrganizationListModel : BaseNopModel
{
    public IList<OrganizationResponseItem> Organizations { get; init; } = new List<OrganizationResponseItem>();
}
