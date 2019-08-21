using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportProductGroupMembershipMappingMap : NopEntityTypeConfiguration<NexportProductGroupMembershipMapping>
    {
        public override void Configure(EntityTypeBuilder<NexportProductGroupMembershipMapping> builder)
        {
            builder.ToTable(nameof(NexportProductGroupMembershipMapping));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.NexportProductMappingId);
            builder.Property(m => m.NexportGroupId);
            builder.Property(m => m.NexportGroupName);
            builder.Property(m => m.NexportGroupShortName);

            base.Configure(builder);
        }
    }
}
