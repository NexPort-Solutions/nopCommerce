using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;

namespace Nop.Plugin.Misc.Nexport.Data.NexportWholesale;

public class NexportRedemptionAssignmentApprovalRequestMap : NopEntityBuilder<NexportRedemptionAssignmentApprovalRequest>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(NexportRedemptionAssignmentApprovalRequest.RedemptionAssignmentType)).AsInt32()
            .WithColumn(nameof(NexportRedemptionAssignmentApprovalRequest.Status)).AsInt32()
            .WithColumn(nameof(NexportRedemptionAssignmentApprovalRequest.RedemptionEmail)).AsFixedLengthString(254)
            .WithColumn(nameof(NexportRedemptionAssignmentApprovalRequest.RedemptionFirstName)).AsFixedLengthString(1000)
            .WithColumn(nameof(NexportRedemptionAssignmentApprovalRequest.RedemptionLastName)).AsFixedLengthString(1000);
    }
}