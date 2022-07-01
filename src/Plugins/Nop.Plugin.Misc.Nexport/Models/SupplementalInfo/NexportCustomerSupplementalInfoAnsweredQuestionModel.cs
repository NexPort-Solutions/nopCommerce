using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public record NexportCustomerSupplementalInfoAnsweredQuestionModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.Text")]
        public string QuestionText { get; set; }
    }
}
