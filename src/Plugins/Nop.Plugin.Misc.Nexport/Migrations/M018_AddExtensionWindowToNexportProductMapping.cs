using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(18, "Add AllowExtension and RenewalWindow to NexportProductMapping table")]
    [SkipMigration]
    public class M018_AddAllowExtensionAndRenewalWindowToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter
                .Table(TABLE_NAME)
                .AddColumn("AllowExtension").AsBoolean().NotNullable().WithDefaultValue(false)
                .AddColumn("RenewalWindow").AsString(255).Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("AllowExtension")
                .Column("RenewalWindow")
                .FromTable(TABLE_NAME);
        }
    }
}