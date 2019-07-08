using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportOrderProcessingQueueMap : NopEntityTypeConfiguration<NexportOrderProcessingQueueItem>
    {
        public override void Configure(EntityTypeBuilder<NexportOrderProcessingQueueItem> builder)
        {
            builder.ToTable("NexportOrderProcessingQueue");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.OrderId);
            builder.Property(m => m.UtcDateCreated);

            base.Configure(builder);
        }
    }
}
