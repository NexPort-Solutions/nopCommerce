using FluentMigrator;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(22, "Add CustomFieldRender to NexportRegistrationField table")]
    public class M022_AddCustomFieldRenderToNexportRegistrationField : Migration
    {
        public override void Up()
        {
            Alter
                .Table(nameof(NexportRegistrationField))
                .AddColumn("CustomFieldRender").AsString(500).Nullable();

            Alter
                .Table(nameof(NexportRegistrationFieldAnswer))
                .AddColumn("IsCustomField").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete
                .Column("CustomFieldRender")
                .FromTable(nameof(NexportRegistrationField));

            Delete
                .Column("IsCustomField")
                .FromTable(nameof(NexportRegistrationFieldAnswer));
        }
    }
}
