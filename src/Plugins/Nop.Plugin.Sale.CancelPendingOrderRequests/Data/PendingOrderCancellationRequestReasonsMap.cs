using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Data
{
    public class PendingOrderCancellationRequestReasonsMap : NopEntityBuilder<PendingOrderCancellationRequestReason>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(PendingOrderCancellationRequestReason.Name)).AsFixedLengthString(400);
        }
    }
}
