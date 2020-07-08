using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoAnswerMembership : BaseEntity
    {
        public int AnswerId { get; set; }

        public Guid NexportMembershipId { get; set; }
    }
}
