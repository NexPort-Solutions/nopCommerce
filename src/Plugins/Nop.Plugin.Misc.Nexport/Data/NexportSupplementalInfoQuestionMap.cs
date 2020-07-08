using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportSupplementalInfoQuestionMap : NopEntityTypeConfiguration<NexportSupplementalInfoQuestion>
    {
        public override void Configure(EntityTypeBuilder<NexportSupplementalInfoQuestion> builder)
        {
            builder.ToTable(nameof(NexportSupplementalInfoQuestion));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.QuestionText);
            builder.Property(m => m.Description).HasMaxLength(1000);
            builder.Property(m => m.Type);
            builder.Property(m => m.IsActive);
            builder.Property(m => m.UtcDateCreated);
            builder.Property(m => m.UtcDateModified);

            base.Configure(builder);
        }
    }
}
