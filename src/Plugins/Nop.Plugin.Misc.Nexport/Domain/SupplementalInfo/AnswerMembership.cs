using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class AnswerMembership : BaseEntity
{
    public int AnswerId { get; set; }
    public Guid MembershipId { get; set; }
}
