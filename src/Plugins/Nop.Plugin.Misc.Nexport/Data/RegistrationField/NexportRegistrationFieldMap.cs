using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Data.RegistrationField
{
    public class NexportRegistrationFieldMap : NopEntityBuilder<NexportRegistrationField>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(NexportRegistrationField.Type)).AsInt32()
                .WithColumn(nameof(NexportRegistrationField.NexportCustomProfileFieldKey)).AsFixedLengthString(255)
                .WithColumn(nameof(NexportRegistrationField.ValidationRegex)).AsFixedLengthString(1000);
        }
    }
}
