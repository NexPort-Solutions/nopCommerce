using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField
{
    public class NexportRegistrationFieldSynchronizationQueueItem : BaseEntity
    {
        public int CustomerId { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateLastAttempt { get; set; }

        public int Attempt { get; set; }
    }
}
