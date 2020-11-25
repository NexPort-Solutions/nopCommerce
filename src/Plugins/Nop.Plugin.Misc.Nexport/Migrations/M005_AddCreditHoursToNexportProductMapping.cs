using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(5, "Add CreditHours to NexportProductMapping table")]
    [SkipMigration]
    public class M005_AddCreditHoursToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME)
                .AddColumn("CreditHours").AsDecimal().Nullable();
        }

        public override void Down()
        {
            Delete.Column("CreditHours").FromTable(TABLE_NAME);
        }
    }
}