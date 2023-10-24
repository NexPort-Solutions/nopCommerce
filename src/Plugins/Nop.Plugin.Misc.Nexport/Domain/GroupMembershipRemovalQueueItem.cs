using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class GroupMembershipRemovalQueueItem : BaseEntity
{
    public int CustomerId { get; set; }
    public Guid MembershipId { get; set; }
    public DateTime UtcDateCreated { get; set; }
}
