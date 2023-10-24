using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Data;

public class SupplementalInfoAnswerMap : NopEntityBuilder<Answer>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table.WithColumn(nameof(Answer.Status)).AsInt32();
}
