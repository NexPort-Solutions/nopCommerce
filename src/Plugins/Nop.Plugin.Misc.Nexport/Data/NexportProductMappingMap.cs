using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportProductMappingMap : NopEntityTypeConfiguration<NexportProductMapping>
    {
        public override void Configure(EntityTypeBuilder<NexportProductMapping> builder)
        {
            builder.ToTable(nameof(NexportProductMapping));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.NopProductId);
            builder.Property(m => m.NexportProductName).HasMaxLength(255);
            builder.Property(m => m.DisplayName).HasMaxLength(255);
            builder.Property(m => m.NexportCatalogSyllabusLinkId);
            builder.Property(m => m.NexportSubscriptionOrgId);
            builder.Property(m => m.NexportSubscriptionOrgName);
            builder.Property(m => m.NexportSubscriptionOrgShortName).HasMaxLength(50);
            builder.Property(m => m.NexportCatalogId);
            builder.Property(m => m.NexportSyllabusId);
            builder.Property(m => m.Type);
            builder.Property(m => m.PublishingModel);
            builder.Property(m => m.PricingModel);
            builder.Property(m => m.UtcLastModifiedDate);
            builder.Property(m => m.UtcAvailableDate);
            builder.Property(m => m.UtcEndDate);
            builder.Property(m => m.UtcLastSynchronizationDate);
            builder.Property(m => m.UtcAccessExpirationDate);
            builder.Property(m => m.AccessTimeLimit);
            builder.Property(m => m.CreditHours);
            builder.Property(m => m.SectionCeus).HasMaxLength(64);
            builder.Property(m => m.IsSynchronized);
            builder.Property(m => m.AutoRedeem);
            builder.Property(m => m.IsExtensionProduct);

            base.Configure(builder);
        }
    }
}
