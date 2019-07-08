using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(2, "Add NexportOrderProcessingQueue table")]
    public class M002_AddNexportOrderProcessingQueue : Migration
    {
        private const string TABLE_NAME = "NexportOrderProcessingQueue";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("OrderId").AsInt32().NotNullable()
                //.WithColumn("OrderItemId").AsInt32().NotNullable()
                //.WithColumn("NexportProductMappingId").AsInt32().NotNullable()
                .WithColumn("UtcDateCreated").AsDateTime2();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
