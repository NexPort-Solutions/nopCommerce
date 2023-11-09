using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class FundingPool : BaseEntity
{
    public required string Name { get; init; }
}
