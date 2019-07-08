using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportUserMappingMap : NopEntityTypeConfiguration<NexportUserMapping>
    {
        public override void Configure(EntityTypeBuilder<NexportUserMapping> builder)
        {
            builder.ToTable(nameof(NexportUserMapping));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.NopUserId);
            builder.Property(m => m.NexportUserId);

            base.Configure(builder);
        }
    }
}
