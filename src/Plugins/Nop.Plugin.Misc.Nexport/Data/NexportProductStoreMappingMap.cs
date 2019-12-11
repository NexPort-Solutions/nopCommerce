using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportProductStoreMappingMap : NopEntityTypeConfiguration<NexportProductStoreMapping>
    {
        public override void Configure(EntityTypeBuilder<NexportProductStoreMapping> builder)
        {
            builder.ToTable(nameof(NexportProductStoreMapping));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.NexportProductMappingId);
            builder.Property(m => m.StoreId);

            base.Configure(builder);
        }
    }
}
