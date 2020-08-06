using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains
{
    public class ArchwayStudentRegistrationFieldKeyMapping : BaseEntity
    {
        public int FieldId { get; set; }

        public string FieldControlName { get; set; }

        public string FieldKey { get; set; }
    }
}
