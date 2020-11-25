using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(2, "Add NexportOrderProcessingQueue table")]
    [SkipMigration]
    public class M002_AddNexportOrderProcessingQueue : Migration
    {
        private const string TABLE_NAME = "NexportOrderProcessingQueue";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("OrderId").AsInt32().NotNullable()
                .WithColumn("UtcDateCreated").AsDateTime2();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
