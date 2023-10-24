using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains;

public class StoreEmployeePosition : BaseEntity
{
    public required int JobCode { get; set; }
    public required string JobTitle { get; set; }
    public required string JobType { get; set; }
    public required string JobLevel { get; set; }
}
