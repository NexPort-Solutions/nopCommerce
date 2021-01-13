using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportSupplementalInfoQuestionMap : NopEntityBuilder<NexportSupplementalInfoQuestion>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(NexportSupplementalInfoQuestion.Description)).AsFixedLengthString(1000)
                .WithColumn(nameof(NexportSupplementalInfoQuestion.Type)).AsInt32();
        }
    }
}
