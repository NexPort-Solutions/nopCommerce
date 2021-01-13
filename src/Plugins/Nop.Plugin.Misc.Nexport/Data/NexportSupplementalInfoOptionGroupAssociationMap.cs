using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportSupplementalInfoOptionGroupAssociationMap: NopEntityBuilder<NexportSupplementalInfoOptionGroupAssociation>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(NexportSupplementalInfoOptionGroupAssociation.NexportGroupShortName)).AsFixedLengthString(50);
        }
    }
}
