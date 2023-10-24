using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains;

public class StudentRegistrationFieldAnswer : BaseEntity
{
    public required int CustomerId { get; set; }
    public required int FieldId { get; set; }
    public required string FieldKey { get; set; }
    public required string TextValue { get; set; }
    public required DateTime UtcDateCreated { get; set; }
    public required DateTime UtcDateModified { get; set; }
}
