using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(39, "Add FundingPoolId and UtcRedeemByDate to WholesaleOrderInfo table")]
[SkipMigration]
public class M039_AddFundingPoolIdAndUtcRedeemByToWholesaleOrderInfo : Migration
{
    private const string TABLE_NAME = "WholesaleOrderInfo";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("FundingPoolId").AsInt32().Nullable()
            .AddColumn("UtcRedeemByDate").AsDateTime2().Nullable();
    }

    public override void Down()
    {
        Delete.Column("FundingPoolId").FromTable(TABLE_NAME);
        Delete.Column("UtcRedeemByDate").FromTable(TABLE_NAME);
    }
}