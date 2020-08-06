using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains
{
    public class ArchwayStudentRegistrationFieldAnswer : BaseEntity
    {
        public int CustomerId { get; set; }

        public int FieldId { get;set; }

        public string FieldKey { get; set; }

        public string TextValue { get;set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }
    }
}
