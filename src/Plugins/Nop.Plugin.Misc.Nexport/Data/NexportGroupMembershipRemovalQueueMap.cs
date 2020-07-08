using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportGroupMembershipRemovalQueueMap : NopEntityTypeConfiguration<NexportGroupMembershipRemovalQueueItem>
    {
        public override void Configure(EntityTypeBuilder<NexportGroupMembershipRemovalQueueItem> builder)
        {
            builder.ToTable("NexportGroupMembershipRemovalQueue");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CustomerId);
            builder.Property(m => m.NexportMembershipId);
            builder.Property(m => m.UtcDateCreated);

            base.Configure(builder);
        }
    }
}
