using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportCustomerAdditionalSettingsModel : BaseNopModel
    {
        public NexportCustomerAdditionalSettingsModel()
        {
            NexportRegistrationFieldCategorySearchModel = new NexportRegistrationFieldCategorySearchModel();
            NexportRegistrationFieldSearchModel = new NexportRegistrationFieldSearchModel();
        }

        public NexportRegistrationFieldCategorySearchModel NexportRegistrationFieldCategorySearchModel { get; set; }

        public NexportRegistrationFieldSearchModel NexportRegistrationFieldSearchModel { get; set; }
    }
}
