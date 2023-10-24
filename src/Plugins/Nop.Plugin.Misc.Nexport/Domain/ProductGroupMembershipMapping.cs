using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class ProductGroupMembershipMapping : BaseEntity
{
    public int ProductMappingId { get; set; }
    public Guid GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? GroupShortName { get; set; }
}
