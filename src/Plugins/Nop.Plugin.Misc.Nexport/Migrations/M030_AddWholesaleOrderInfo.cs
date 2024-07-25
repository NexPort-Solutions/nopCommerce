using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(30, "Add WholesaleOrderInfo table")]
    [SkipMigration]
    public class M030_AddWholesaleOrderInfo : Migration
    {
        private const string TABLE_NAME = "WholesaleOrderInfo";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("NexportGroupId").AsGuid()
                .WithColumn("OrderId").AsInt32().NotNullable()
                .WithColumn("OrderItemId").AsInt32().NotNullable()
                .WithColumn("ProductId").AsInt32().NotNullable()
                .WithColumn("Available").AsInt32().NotNullable()
                .WithColumn("Awaiting").AsInt32().NotNullable()
                .WithColumn("Redeemed").AsInt32().NotNullable();
        }

        public override void Down()
        {
           Delete.Table(TABLE_NAME);
        }
    }
}
