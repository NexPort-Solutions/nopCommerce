using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;

namespace Nop.Plugin.Misc.Nexport.Data.NexportWholesale;

public class NexportRedemptionUnassignmentRequestMap : NopEntityBuilder<NexportRedemptionUnassignmentRequest>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(NexportRedemptionUnassignmentRequest.RequestStatus)).AsInt32();
    }
}