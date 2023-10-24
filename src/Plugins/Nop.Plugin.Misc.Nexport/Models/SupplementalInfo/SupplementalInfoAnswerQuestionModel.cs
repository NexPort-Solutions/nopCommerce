namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoAnswerQuestionModel
{
    public IList<int> QuestionIds { get; init; } = new List<int>();

    public IList<int> QuestionWithoutAnswerIds { get; init; } = new List<int>();

    public required string ReturnUrl { get; init; }
}
