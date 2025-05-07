using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(43, "Add UtcProcessingDate to NexportOrderInvoiceRedemptionQueue table")]
[SkipMigration]
public class M043_AddUtcProcessingDateToNexportOrderInvoiceRedemptionQueue : Migration
{
    private const string TABLE_NAME = "NexportOrderInvoiceRedemptionQueue";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("RedeemingProductMappingId").AsInt32().Nullable()
            .AddColumn("UtcProcessingDate").AsDateTime2().Nullable();
    }

    public override void Down()
    {
        Delete.Column("RedeemingProductMappingId").FromTable(TABLE_NAME);
        Delete.Column("UtcProcessingDate").FromTable(TABLE_NAME);
    }
}