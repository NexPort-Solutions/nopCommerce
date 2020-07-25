using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField
{
    public class NexportRegistrationFieldAnswer : BaseEntity
    {
        public int CustomerId { get; set; }

        public int FieldId { get; set; }

        public string TextValue { get; set; }

        public int? NumericValue { get; set; }

        public DateTime? DateTimeValue { get; set; }

        public bool? BooleanValue { get; set; }

        public int? FieldOptionId { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }
    }
}
