using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(16, "Add ProductMappingId and OrderItemId to NexportOrderInvoiceRedemptionQueue table")]
    public class M016_AddProductMappingIdAndOrderItemIdToNexportOrderInvoiceRedemptionQueueItem : Migration
    {
        private const string TABLE_NAME = "NexportOrderInvoiceRedemptionQueue";

        public override void Up()
        {
            Alter
                .Table(TABLE_NAME)
                .AddColumn("ProductMappingId").AsInt32()
                .AddColumn("OrderItemId").AsInt32();
        }

        public override void Down()
        {
            Delete
                .Column("ProductMappingId")
                .Column("OrderItemId")
                .FromTable(TABLE_NAME);
        }
    }
}
