using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class UserMapping : BaseEntity
{
    public required int NopUserId { get; init; }
    public required Guid UserId { get; init; }
    public DateTime? UtcDateSynchronize { get; init; }
}
