using System.Collections.Generic;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer
{
    public class NexportCustomerRegistrationFieldsModel
    {
        public Dictionary<NexportRegistrationFieldCategoryModel, List<NexportRegistrationFieldModel>> RegistrationFieldsWithCategory { get; set; }

        public IList<NexportRegistrationFieldModel> RegistrationFieldsWithoutCategory { get;set; }
    }
}
