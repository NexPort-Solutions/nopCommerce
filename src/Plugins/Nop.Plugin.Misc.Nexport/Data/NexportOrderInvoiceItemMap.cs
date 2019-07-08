using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportOrderInvoiceItemMap : NopEntityTypeConfiguration<NexportOrderInvoiceItem>
    {
        public override void Configure(EntityTypeBuilder<NexportOrderInvoiceItem> builder)
        {
            builder.ToTable(nameof(NexportOrderInvoiceItem));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.OrderId);
            builder.Property(m => m.OrderItemId);
            builder.Property(m => m.InvoiceItemId);
            builder.Property(m => m.UtcDateProcessed);
            builder.Property(m => m.RedeemingUserId);
            builder.Property(m => m.RedemptionEnrollmentId);
            builder.Property(m => m.UtcDateRedemption);

            base.Configure(builder);
        }
    }
}
