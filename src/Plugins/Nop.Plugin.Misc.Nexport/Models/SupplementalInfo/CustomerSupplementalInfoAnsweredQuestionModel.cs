using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record CustomerSupplementalInfoAnsweredQuestionModel : BaseNopEntityModel
{
    public int CustomerId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.NONE")]
    public string? QuestionText { get; set; }
}
