using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Data
{
    public class PendingOrderCancellationRequestMap : NopEntityBuilder<PendingOrderCancellationRequest>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(PendingOrderCancellationRequest.RequestStatus)).AsInt32();
        }
    }
}
