using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(17, "Change AccessTimeLimit in NexportProductMapping table to string type")]
    [SkipMigration]
    public class M017_ChangeAccessTimeLimitToStringType : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter
                .Table(TABLE_NAME)
                .AlterColumn("AccessTimeLimit").AsString(255).Nullable();
        }

        public override void Down()
        {
            Alter
                .Table(TABLE_NAME)
                .AlterColumn("AccessTimeLimit").AsInt64().Nullable();
        }
    }
}