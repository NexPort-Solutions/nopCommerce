using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportOrderInvoiceRedemptionQueueMap : NopEntityTypeConfiguration<NexportOrderInvoiceRedemptionQueueItem>
    {
        public override void Configure(EntityTypeBuilder<NexportOrderInvoiceRedemptionQueueItem> builder)
        {
            builder.ToTable("NexportOrderInvoiceRedemptionQueue");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.OrderInvoiceItemId);
            builder.Property(m => m.RedeemingUserId);
            builder.Property(m => m.UtcDateCreated);
            builder.Property(m => m.UtcLastFailedDate);
            builder.Property(m => m.RetryCount);

            base.Configure(builder);
        }
    }
}
