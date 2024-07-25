using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(29, "Add WholesalePurchasingGroup table")]
    [SkipMigration]
    public class M029_AddWholesalePurchasingGroup : Migration
    {
        private const string TABLE_NAME = "WholesalePurchasingGroup";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("NexportGroupId").AsGuid().NotNullable()
                .WithColumn("NexportGroupName").AsString().NotNullable()
                .WithColumn("NexportGroupShortName").AsString(50).NotNullable();
        }

        public override void Down()
        {
           Delete.Table(TABLE_NAME);
        }
    }
}
