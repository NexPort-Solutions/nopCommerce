using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoOptionGroupAssociationModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.GroupId")]
    public Guid GroupId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.GroupName")]
    public string? GroupName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.GroupShortName")]
    public string? GroupShortName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Active")]
    public bool IsActive { get; set; }
}
