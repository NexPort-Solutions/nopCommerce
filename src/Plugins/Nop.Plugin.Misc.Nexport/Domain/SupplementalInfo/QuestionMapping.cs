using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class QuestionMapping : BaseEntity
{
    public required int QuestionId { get; init; }
    public required int ProductMappingId { get; init; }
    public required DateTime UtcDateCreated { get; init; }
    public required DateTime UtcDateModified { get; init; }
}
