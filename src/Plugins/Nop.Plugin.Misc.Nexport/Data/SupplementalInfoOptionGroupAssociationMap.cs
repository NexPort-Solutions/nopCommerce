using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Data;

public class SupplementalInfoOptionGroupAssociationMap : NopEntityBuilder<OptionGroupAssociation>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
        => table.WithColumn(nameof(OptionGroupAssociation.GroupShortName)).AsFixedLengthString(50);
}
