using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(22, "Add CustomFieldRender to NexportRegistrationField table")]
[SkipMigration]
public class M022_AddCustomFieldRenderToNexportRegistrationField : Migration
{
    public override void Up()
    {
        Alter
            .Table("NexportRegistrationField")
            .AddColumn("CustomFieldRender").AsString(500).Nullable();

        Alter
            .Table("NexportRegistrationFieldAnswer")
            .AddColumn("IsCustomField").AsBoolean().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete
            .Column("CustomFieldRender")
            .FromTable("NexportRegistrationField");

        Delete
            .Column("IsCustomField")
            .FromTable("NexportRegistrationFieldAnswer");
    }
}
