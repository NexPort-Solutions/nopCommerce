using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(25, "Add ValidationMessage to Nexport registration field table")]
    [SkipMigration]
    public class M025_AddValidationMessageToNexportRegistrationField : Migration
    {
        public override void Up()
        {
            Alter
                .Table(nameof(NexportRegistrationField))
                .AddColumn("ValidationMessage").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("ValidationMessage")
                .FromTable(nameof(NexportRegistrationField));
        }
    }
}