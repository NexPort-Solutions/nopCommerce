using System.Collections.Generic;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportCustomerSupplementalInfoAnswersModel
    {
        public NexportCustomerSupplementalInfoAnswersModel()
        {
            QuestionWithAnswersList = new Dictionary<int, Dictionary<int, int>>();
        }

        public Dictionary<int, Dictionary<int, int>> QuestionWithAnswersList { get; set; }
    }
}
