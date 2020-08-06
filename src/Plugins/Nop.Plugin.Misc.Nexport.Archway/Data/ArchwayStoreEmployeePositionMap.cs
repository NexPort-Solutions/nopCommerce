using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Data
{
    public class ArchwayStoreEmployeePositionMap: NopEntityTypeConfiguration<ArchwayStoreEmployeePosition>
    {
        public override void Configure(EntityTypeBuilder<ArchwayStoreEmployeePosition> builder)
        {
            builder.ToTable("ArchwayStoreEmployeePosition");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.JobCode);
            builder.Property(m => m.JobTitle);
            builder.Property(m => m.JobType);
            builder.Property(m => m.JobLevel);

            base.Configure(builder);
        }
    }
}
