using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public record NexportCustomerAdditionalSettingsModel : BaseNopModel
    {
        public NexportRegistrationFieldCategorySearchModel NexportRegistrationFieldCategorySearchModel { get; set; } = new();

        public NexportRegistrationFieldSearchModel NexportRegistrationFieldSearchModel { get; set; } = new();
    }
}
