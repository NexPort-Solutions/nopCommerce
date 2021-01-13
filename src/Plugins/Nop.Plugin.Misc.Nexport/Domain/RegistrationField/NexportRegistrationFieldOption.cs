using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField
{
    public class NexportRegistrationFieldOption : BaseEntity
    {
        public int FieldId { get; set; }

        public string OptionValue { get; set; }

        public int DisplayOrder { get; set; }
    }
}