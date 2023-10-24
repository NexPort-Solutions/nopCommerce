using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(25, "Add ValidationMessage to Nexport registration field table")]
[SkipMigration]
public class M025_AddValidationMessageToNexportRegistrationField : Migration
{
    public override void Up()
    {
        Alter
            .Table("NexportRegistrationField")
            .AddColumn("ValidationMessage").AsString(int.MaxValue).Nullable();
    }

    public override void Down()
    {
        Delete
            .Column("ValidationMessage")
            .FromTable("NexportRegistrationField");
    }
}
