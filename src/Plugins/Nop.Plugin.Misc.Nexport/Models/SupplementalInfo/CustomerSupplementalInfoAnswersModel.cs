namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record CustomerSupplementalInfoAnswersModel(Dictionary<int, Dictionary<int, int>> QuestionWithAnswersList);
