using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale;

public class NexportFundingPool : BaseEntity
{
    public string Name { get; init; }

    public string Code { get; init; }

    public string Description { get; init; }

    public DateTime UtcDateCreated { get; set; }

    public DateTime? UtcDateModified { get; set; }
}