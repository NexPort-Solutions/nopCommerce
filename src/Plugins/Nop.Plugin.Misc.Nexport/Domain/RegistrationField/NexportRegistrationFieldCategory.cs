using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField
{
    public class NexportRegistrationFieldCategory: BaseEntity
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }
    }
}
