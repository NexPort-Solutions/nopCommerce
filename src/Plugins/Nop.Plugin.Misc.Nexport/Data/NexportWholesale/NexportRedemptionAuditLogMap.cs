using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;

namespace Nop.Plugin.Misc.Nexport.Data.NexportWholesale;

public class NexportRedemptionAuditLogMap : NopEntityBuilder<NexportRedemptionAuditLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(NexportRedemptionAuditLog.Type)).AsInt32();
    }
}
