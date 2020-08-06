using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Data
{
    public class ArchwayStudentRegistrationFieldAnswerMap : NopEntityTypeConfiguration<ArchwayStudentRegistrationFieldAnswer>
    {
        public override void Configure(EntityTypeBuilder<ArchwayStudentRegistrationFieldAnswer> builder)
        {
            builder.ToTable(nameof(ArchwayStudentRegistrationFieldAnswer));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CustomerId);
            builder.Property(m => m.FieldId);
            builder.Property(m => m.FieldKey);
            builder.Property(m => m.TextValue);
            builder.Property(m => m.UtcDateCreated);
            builder.Property(m => m.UtcDateModified);

            base.Configure(builder);
        }
    }
}
