using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer
{
    public class NexportCustomerRegistrationFieldAnswerModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }

        public int FieldId { get; set; }

        public int? FieldOptionId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Value")]
        public string FieldValue { get; set; }
    }
}
