using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public record NexportCustomerSupplementalInfoAnswerEditModel : BaseNopModel
    {
        public NexportSupplementalInfoQuestion Question { get; set; }

        public IList<NexportSupplementalInfoOption> Options { get; set; } = new List<NexportSupplementalInfoOption>();

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
