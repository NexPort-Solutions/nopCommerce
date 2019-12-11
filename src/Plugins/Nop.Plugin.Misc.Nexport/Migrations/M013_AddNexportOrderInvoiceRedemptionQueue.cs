using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(13, "Add NexportOrderInvoiceRedemptionQueue table")]
    public class M013_AddNexportOrderInvoiceRedemptionQueue : Migration
    {
        private const string TABLE_NAME = "NexportOrderInvoiceRedemptionQueue";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("OrderInvoiceItemId").AsInt32().NotNullable()
                .WithColumn("RedeemingUserId").AsGuid().NotNullable()
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
