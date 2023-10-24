using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoQuestionMappingModel : BaseNopEntityModel
{
    public int ProductMappingId { get; set; }
    public int SupplementalInfoQuestionId { get; set; }
}
