using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoOptionGroupAssociationListSearchModel : BaseSearchModel
{
    public SupplementalInfoOptionGroupAssociationListSearchModel() => SetGridPageSize();

    public int OptionId { get; set; }
}
