using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportCustomerSupplementalInfoAnswerEditModel : BaseNopModel
    {
        public NexportCustomerSupplementalInfoAnswerEditModel()
        {
            Options = new List<NexportSupplementalInfoOption>();
        }

        public NexportSupplementalInfoQuestion Question { get; set; }

        public IList<NexportSupplementalInfoOption> Options { get; set; }

        public IList<EditSupplementInfoAnswerRequest> Answers { get; set; }
    }

    [Serializable]
    public class EditSupplementInfoAnswerRequest
    {
        public int AnswerId { get; set; }

        public int OptionId { get; set; }
    }

    [Serializable]
    public class EditSupplementInfoAnswerRequestModel {
        public int QuestionId { get; set; }

        public IList<int> OptionIds { get; set; }
    }
}
