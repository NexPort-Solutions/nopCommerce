using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(4, "Add NexportOrderInvoiceItem table")]
    [SkipMigration]
    public class M004_AddNexportOrderInvoiceItem : Migration
    {
        private const string TABLE_NAME = "NexportOrderInvoiceItem";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("OrderId").AsInt32().NotNullable()
                .WithColumn("OrderItemId").AsInt32().NotNullable()
                .WithColumn("InvoiceItemId").AsGuid().NotNullable()
                .WithColumn("UtcDateProcessed").AsDateTime2().Nullable()
                .WithColumn("RedeemingUserId").AsGuid().Nullable()
                .WithColumn("RedemptionEnrollmentId").AsGuid().Nullable()
                .WithColumn("UtcDateRedemption").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}