using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Data;

public class RegistrationFieldMap : NopEntityBuilder<RegistrationField>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(RegistrationField.Type))
            .AsInt32()
            .WithColumn(nameof(RegistrationField.CustomProfileFieldKey))
            .AsFixedLengthString(255)
            .WithColumn(nameof(RegistrationField.ValidationRegex))
            .AsFixedLengthString(1000);
    }
}
