using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record CustomerSupplementalInfoAnswersEditModel : BaseNopModel
{
    public Question? Question { get; set; }
    public IList<Option> Options { get; init; } = new List<Option>();
    public IList<EditSupplementInfoAnswerRequest> Answers { get; init; } = new List<EditSupplementInfoAnswerRequest>();
}

public class EditSupplementInfoAnswerRequest
{
    public int? AnswerId { get; set; }
    public int? OptionId { get; set; }
}

public class EditSupplementInfoAnswerRequestModel
{
    public int? QuestionId { get; set; }
    public IList<int> OptionIds { get; init; } = new List<int>();
}
