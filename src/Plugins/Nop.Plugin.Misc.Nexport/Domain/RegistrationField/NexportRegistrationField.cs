using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField
{
    public class NexportRegistrationField: BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }

        public NexportRegistrationFieldType Type { get; set; }

        public string NexportCustomProfileFieldKey { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public bool Validation { get; set; }

        public string ValidationRegex { get; set; }

        public int? FieldCategoryId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
