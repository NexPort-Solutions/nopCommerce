using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Data;

public class SupplementalInfoQuestionMap : NopEntityBuilder<Question>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Question.Description))
            .AsFixedLengthString(1000)
            .WithColumn(nameof(Question.Type))
            .AsInt32();
    }
}
