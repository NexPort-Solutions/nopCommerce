using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    [Serializable]
    public class SaveSupplementalInfoAnswers
    {
        public IList<SupplementInfoAnswerRequest> Answers { get; set; }
    }

    [Serializable]
    public class SupplementInfoAnswerRequest {
        public int QuestionId { get; set; }

        public IList<int> Options { get; set; }
    }
}
