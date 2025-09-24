using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(45, "Add redemption assignment approval request table")]
[SkipMigration]
public class M045_AddRedemptionAssignmentApprovalRequest : Migration
{
    private const string TABLE_NAME = "NexportRedemptionAssignmentApprovalRequest";

    public override void Up()
    {
        Create.Table(TABLE_NAME)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ProductId").AsInt32().NotNullable()
            .WithColumn("RedeemingProductId").AsInt32().Nullable()
            .WithColumn("RedemptionAssignmentType").AsInt32().NotNullable()
            .WithColumn("RedemptionEmail").AsString(254).Nullable()
            .WithColumn("RedemptionFirstName").AsString(1000).Nullable()
            .WithColumn("RedemptionLastName").AsString(1000).Nullable()
            .WithColumn("InvoiceItemId").AsGuid().NotNullable()
            .WithColumn("RedemptionUserId").AsGuid().Nullable()
            .WithColumn("UtcRedemptionStartDate").AsDateTime2().Nullable()
            .WithColumn("StoreId").AsInt32().Nullable()
            .WithColumn("PurchasingGroupId").AsGuid().Nullable()
            .WithColumn("IsOpenEnded").AsBoolean().NotNullable()
            .WithColumn("ExtensionOption").AsInt32().Nullable()
            .WithColumn("UtcCreatedDate").AsDateTime2().NotNullable()
            .WithColumn("UtcModifiedDate").AsDateTime2().Nullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("Notes").AsString().Nullable()
            .WithColumn("RequestedByCustomerId").AsInt32().NotNullable()
            .WithColumn("ApprovedByCustomerId").AsInt32().Nullable();
    }

    public override void Down()
    {
        Delete.Table(TABLE_NAME);
    }
}