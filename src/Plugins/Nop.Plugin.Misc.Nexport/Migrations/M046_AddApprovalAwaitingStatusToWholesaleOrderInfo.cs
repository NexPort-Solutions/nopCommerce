using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(46, "Add approval awaiting status to WholesaleOrderInfo table")]
[SkipMigration]
public class M046_AddApprovalAwaitingStatusToWholesaleOrderInfo : Migration
{
    private const string TABLE_NAME = "WholesaleOrderInfo";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("ApprovalAwaiting").AsInt32().NotNullable().SetExistingRowsTo(0);
    }

    public override void Down()
    {
        Delete.Column("ApprovalAwaiting").FromTable(TABLE_NAME);
    }
}