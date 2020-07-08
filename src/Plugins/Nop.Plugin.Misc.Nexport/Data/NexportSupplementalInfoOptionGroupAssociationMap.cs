using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportSupplementalInfoOptionGroupAssociationMap: NopEntityTypeConfiguration<NexportSupplementalInfoOptionGroupAssociation>
    {
        public override void Configure(EntityTypeBuilder<NexportSupplementalInfoOptionGroupAssociation> builder)
        {
            builder.ToTable(nameof(NexportSupplementalInfoOptionGroupAssociation));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.OptionId);
            builder.Property(m => m.NexportGroupId);
            builder.Property(m => m.NexportGroupName);
            builder.Property(m => m.NexportGroupShortName).HasMaxLength(50);
            builder.Property(m => m.IsActive);
            builder.Property(m => m.UtcDateCreated);
            builder.Property(m => m.UtcDateModified);

            base.Configure(builder);
        }
    }
}
