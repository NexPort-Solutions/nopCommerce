using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class FundingPool : BaseEntity
{
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required string? Description { get; init; }
}
