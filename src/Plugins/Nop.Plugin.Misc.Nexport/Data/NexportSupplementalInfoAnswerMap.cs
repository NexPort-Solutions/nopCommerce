using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportSupplementalInfoAnswerMap : NopEntityTypeConfiguration<NexportSupplementalInfoAnswer>
    {
        public override void Configure(EntityTypeBuilder<NexportSupplementalInfoAnswer> builder)
        {
            builder.ToTable(nameof(NexportSupplementalInfoAnswer));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CustomerId);
            builder.Property(m => m.StoreId);
            builder.Property(m => m.QuestionId);
            builder.Property(m => m.OptionId);
            builder.Property(m => m.Status);
            builder.Property(m => m.UtcDateProcessed);
            builder.Property(m => m.UtcDateCreated);
            builder.Property(m => m.UtcDateModified);

            base.Configure(builder);
        }
    }
}
