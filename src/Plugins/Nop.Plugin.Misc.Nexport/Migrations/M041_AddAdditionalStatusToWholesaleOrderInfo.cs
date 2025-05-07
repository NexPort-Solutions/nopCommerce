using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(41, "Add additional status to WholesaleOrderInfo table")]
[SkipMigration]
public class M041_AddAdditionalStatusToWholesaleOrderInfo : Migration
{
    private const string TABLE_NAME = "WholesaleOrderInfo";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("ProcessingAvailable").AsInt32().SetExistingRowsTo(0)
            .AddColumn("ProcessingAwaiting").AsInt32().SetExistingRowsTo(0)
            .AddColumn("Refunded").AsInt32().SetExistingRowsTo(0)
            .AddColumn("ProcessingRefund").AsInt32().SetExistingRowsTo(0);
    }

    public override void Down()
    {
        Delete.Column("ProcessingAvailable").FromTable(TABLE_NAME);
        Delete.Column("ProcessingAwaiting").FromTable(TABLE_NAME);
        Delete.Column("Refunded").FromTable(TABLE_NAME);
        Delete.Column("ProcessingRefund").FromTable(TABLE_NAME);
    }
}