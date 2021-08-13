using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer
{
    public class NexportCustomerRegistrationFieldWithAnswersListSearchModel : BaseSearchModel
    {
        public NexportCustomerRegistrationFieldWithAnswersListSearchModel()
        {
            SetGridPageSize();
        }

        public int CustomerId { get; set; }
    }
}