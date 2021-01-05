using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportProductMappingMap : NopEntityBuilder<NexportProductMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(NexportProductMapping.NexportProductName)).AsFixedLengthString(255)
                .WithColumn(nameof(NexportProductMapping.DisplayName)).AsFixedLengthString(255)
                .WithColumn(nameof(NexportProductMapping.Type)).AsInt32()
                .WithColumn(nameof(NexportProductMapping.PublishingModel)).AsInt32()
                .WithColumn(nameof(NexportProductMapping.PricingModel)).AsInt32()
                .WithColumn(nameof(NexportProductMapping.NexportSubscriptionOrgName)).AsFixedLengthString(255)
                .WithColumn(nameof(NexportProductMapping.NexportSubscriptionOrgShortName)).AsFixedLengthString(50)
                .WithColumn(nameof(NexportProductMapping.SectionCeus)).AsFixedLengthString(64)
                .WithColumn(nameof(NexportProductMapping.RenewalWindow)).AsFixedLengthString(255)
                .WithColumn(nameof(NexportProductMapping.RenewalDuration)).AsFixedLengthString(255);
        }
    }
}