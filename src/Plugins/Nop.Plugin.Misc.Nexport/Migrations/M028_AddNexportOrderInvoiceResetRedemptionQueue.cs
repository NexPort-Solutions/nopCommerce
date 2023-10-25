using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(28, "Add NexportOrderInvoiceResetRedemptionQueue table")]
    [SkipMigration]
    public class M028_AddNexportOrderInvoiceResetRedemptionQueue : Migration
    {
        private const string TABLE_NAME = "NexportOrderInvoiceResetRedemptionQueue";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("OrderInvoiceItemId").AsInt32().NotNullable()
                .WithColumn("UtcDateCreated").AsDateTime2()
                .WithColumn("UtcLastFailedDate").AsDateTime2().Nullable()
                .WithColumn("RetryCount").AsInt32().NotNullable().WithDefaultValue(0);
        }

        public override void Down()
        {
           Delete.Table(TABLE_NAME);
        }
    }
}
