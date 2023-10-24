using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record ProductGroupMembershipMappingListSearchModel : BaseSearchModel
{
    public ProductGroupMembershipMappingListSearchModel() => SetGridPageSize();

    public int ProductMappingId { get; set; }
}
