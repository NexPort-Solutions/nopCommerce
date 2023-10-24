using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoQuestionSearchModel : BaseSearchModel
{
    public SupplementalInfoQuestionSearchModel() => SetGridPageSize();
}
