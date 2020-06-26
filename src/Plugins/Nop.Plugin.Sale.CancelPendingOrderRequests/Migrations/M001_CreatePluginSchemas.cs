using FluentMigrator;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Migrations
{
    [Migration(1, "Initial table creation for CancelPendingOrderRequests plugin")]
    [Tags("CancelPendingOrderRequestPluginMigration")]
    public class M001_CreatePluginSchemas : Migration
    {
        private const string PENDING_ORDER_CANCELLATION_REQUESTS_TABLE_NAME =
            "PendingOrderCancellationRequests";

        private const string PENDING_ORDER_CANCELLATION_REQUEST_REASONS_TABLE_NAME =
            "PendingOrderCancellationRequestReasons";

        public override void Up()
        {
            Create.Table(PENDING_ORDER_CANCELLATION_REQUESTS_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("OrderId").AsInt32().NotNullable()
                .WithColumn("CustomerId").AsInt32().NotNullable()
                .WithColumn("StoreId").AsInt32().NotNullable()
                .WithColumn("CustomerComments").AsString().Nullable()
                .WithColumn("ReasonForCancellation").AsString().NotNullable()
                .WithColumn("StaffNotes").AsString().Nullable()
                .WithColumn("RequestStatus").AsInt32().NotNullable()
                .WithColumn("UtcCreatedDate").AsDateTime2().NotNullable()
                .WithColumn("UtcLastModifiedDate").AsDateTime2().Nullable();

            Create.Table(PENDING_ORDER_CANCELLATION_REQUEST_REASONS_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(400).NotNullable()
                .WithColumn("DisplayOrder").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(PENDING_ORDER_CANCELLATION_REQUESTS_TABLE_NAME);
            Delete.Table(PENDING_ORDER_CANCELLATION_REQUEST_REASONS_TABLE_NAME);
        }
    }
}