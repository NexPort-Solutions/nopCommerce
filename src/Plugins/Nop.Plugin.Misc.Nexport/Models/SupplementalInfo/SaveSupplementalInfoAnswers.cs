namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

[Serializable]
public class SaveSupplementalInfoAnswers
{
    public required IList<SupplementInfoAnswerRequest> Answers { get; init; } = new List<SupplementInfoAnswerRequest>();
}

[Serializable]
public class SupplementInfoAnswerRequest
{
    public int QuestionId { get; set; }
    public required IList<int> Options { get; init; } = new List<int>();
}
