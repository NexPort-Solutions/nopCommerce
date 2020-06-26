using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Data
{
    public class PendingOrderCancellationRequestReasonsMap : NopEntityTypeConfiguration<PendingOrderCancellationRequestReason>
    {
        public override void Configure(EntityTypeBuilder<PendingOrderCancellationRequestReason> builder)
        {
            builder.ToTable("PendingOrderCancellationRequestReasons");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name).HasMaxLength(400);
            builder.Property(m => m.DisplayOrder);

            base.Configure(builder);
        }
    }
}
