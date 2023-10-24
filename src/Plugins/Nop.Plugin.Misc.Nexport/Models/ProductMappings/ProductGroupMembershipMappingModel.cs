using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record ProductGroupMembershipMappingModel : BaseNopEntityModel
{
    public int ProductMappingId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.GroupId")]
    public Guid GroupId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.GroupName")]
    public string? GroupName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.GroupShortName")]
    public string? GroupShortName { get; set; }
}
