using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Data
{
    public class ArchwayStudentRegistrationFieldKeyMappingMap : NopEntityTypeConfiguration<ArchwayStudentRegistrationFieldKeyMapping>
    {
        public override void Configure(EntityTypeBuilder<ArchwayStudentRegistrationFieldKeyMapping> builder)
        {
            builder.ToTable(nameof(ArchwayStudentRegistrationFieldKeyMapping));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.FieldId);
            builder.Property(m => m.FieldControlName);
            builder.Property(m => m.FieldKey);

            base.Configure(builder);
        }
    }
}
