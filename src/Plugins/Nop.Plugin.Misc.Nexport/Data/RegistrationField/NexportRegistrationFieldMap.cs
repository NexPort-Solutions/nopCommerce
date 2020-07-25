using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Data.RegistrationField
{
    public class NexportRegistrationFieldMap : NopEntityTypeConfiguration<NexportRegistrationField>
    {
        public override void Configure(EntityTypeBuilder<NexportRegistrationField> builder)
        {
            builder.ToTable(nameof(NexportRegistrationField));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name);
            builder.Property(m => m.Type);
            builder.Property(m => m.NexportCustomProfileFieldKey).HasMaxLength(255);
            builder.Property(m => m.IsRequired);
            builder.Property(m => m.IsActive);
            builder.Property(m => m.Validation);
            builder.Property(m => m.ValidationRegex).HasMaxLength(1000);
            builder.Property(m => m.FieldCategoryId);
            builder.Property(m => m.DisplayOrder);

            base.Configure(builder);
        }
    }
}
