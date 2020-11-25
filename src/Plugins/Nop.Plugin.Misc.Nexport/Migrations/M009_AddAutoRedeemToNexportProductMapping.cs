using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(9, "Add AutoRedeem to NexportProductMapping table")]
    [SkipMigration]
    public class M009_AddAutoRedeemToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME).AddColumn("AutoRedeem").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("AutoRedeem").FromTable(TABLE_NAME);
        }
    }
}