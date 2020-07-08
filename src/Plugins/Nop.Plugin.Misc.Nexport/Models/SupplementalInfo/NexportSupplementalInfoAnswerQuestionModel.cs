using System.Collections.Generic;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportSupplementalInfoAnswerQuestionModel
    {
        public NexportSupplementalInfoAnswerQuestionModel()
        {
            QuestionIds = new List<int>();
            QuestionWithoutAnswerIds = new List<int>();
        }

        public IList<int> QuestionIds { get; set; }

        public IList<int> QuestionWithoutAnswerIds { get; set; }

        public string ReturnUrl { get; set; }
    }
}
