using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data;

public class ProductMappingMap : NopEntityBuilder<ProductMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(ProductMapping.ProductName)).AsFixedLengthString(255)
            .WithColumn(nameof(ProductMapping.DisplayName))
            .AsFixedLengthString(255)
            .WithColumn(nameof(ProductMapping.Type))
            .AsInt32()
            .WithColumn(nameof(ProductMapping.PublishingModel))
            .AsInt32()
            .WithColumn(nameof(ProductMapping.PricingModel))
            .AsInt32()
            .WithColumn(nameof(ProductMapping.SubscriptionOrgName))
            .AsFixedLengthString(255)
            .WithColumn(nameof(ProductMapping.SubscriptionOrgShortName))
            .AsFixedLengthString(50)
            .WithColumn(nameof(ProductMapping.SectionCeus))
            .AsFixedLengthString(64)
            .WithColumn(nameof(ProductMapping.RenewalWindow))
            .AsFixedLengthString(255)
            .WithColumn(nameof(ProductMapping.RenewalDuration))
            .AsFixedLengthString(255);
    }
}
