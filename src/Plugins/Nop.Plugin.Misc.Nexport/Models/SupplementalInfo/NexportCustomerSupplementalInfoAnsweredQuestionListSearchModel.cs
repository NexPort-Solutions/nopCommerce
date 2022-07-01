using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public record NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel: BaseSearchModel
    {
        public NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel()
        {
            SetGridPageSize();
        }

        public int CustomerId { get; set; }
    }
}
