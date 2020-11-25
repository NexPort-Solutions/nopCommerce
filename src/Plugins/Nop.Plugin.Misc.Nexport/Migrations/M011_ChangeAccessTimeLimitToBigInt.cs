using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(11, "Change AccessTimeLimit on NexportProductMapping table to BigInt type ")]
    [SkipMigration]
    public class M011_ChangeAccessTimeLimitToBigInt : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME).AlterColumn("AccessTimeLimit").AsInt64().Nullable();
        }

        public override void Down()
        {
            Alter.Table(TABLE_NAME).AlterColumn("AccessTimeLimit").AsInt32().Nullable();
        }
    }
}
