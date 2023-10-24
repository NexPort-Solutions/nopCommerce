using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record CustomerSupplementalInfoAnsweredQuestionListSearchModel : BaseSearchModel
{
    public CustomerSupplementalInfoAnsweredQuestionListSearchModel() => SetGridPageSize();

    public int CustomerId { get; set; }
}
