using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class OptionGroupAssociation : BaseEntity
{
    public int OptionId { get; set; }
    public Guid GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? GroupShortName { get; set; }
    public bool IsActive { get; set; }
    public DateTime UtcDateCreated { get; set; }
    public DateTime? UtcDateModified { get; set; }
}
