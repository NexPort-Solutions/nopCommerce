using System.Collections.Generic;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public record NexportCustomerSupplementalInfoAnswersModel
    {
        public Dictionary<int, Dictionary<int, int>> QuestionWithAnswersList { get; set; } = new();
    }
}
