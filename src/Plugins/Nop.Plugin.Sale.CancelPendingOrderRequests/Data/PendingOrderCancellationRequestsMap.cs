using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Data
{
    public class PendingOrderCancellationRequestsMap : NopEntityTypeConfiguration<PendingOrderCancellationRequest>
    {
        public override void Configure(EntityTypeBuilder<PendingOrderCancellationRequest> builder)
        {
            builder.ToTable("PendingOrderCancellationRequests");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.OrderId);
            builder.Property(m => m.CustomerId);
            builder.Property(m => m.StoreId);
            builder.Property(m => m.CustomerComments);
            builder.Property(m => m.ReasonForCancellation);
            builder.Property(m => m.StaffNotes);
            builder.Property(m => m.RequestStatus);
            builder.Property(m => m.UtcCreatedDate);
            builder.Property(m => m.UtcLastModifiedDate);
        }
    }
}
