using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer
{
    public class NexportCustomerRegistrationFieldAnswerListSearchModel : BaseSearchModel
    {
        public NexportCustomerRegistrationFieldAnswerListSearchModel()
        {
            SetGridPageSize();
        }

        public int CustomerId { get; set; }

        public int FieldId { get; set; }
    }
}