using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(23, "Add additional renewal extension properties to Nexport product mapping table")]
[SkipMigration]
public class M023_AddRenewalExtensionPropertiesToNexportProductMapping : Migration
{
    public override void Up()
    {
        Alter
            .Table("NexportProductMapping")
            .AddColumn("RenewalDuration").AsString(255).Nullable()
            .AddColumn("RenewalCompletionThreshold").AsInt32().Nullable()
            .AddColumn("RenewalApprovalMethod").AsInt32().Nullable()
            .AddColumn("ExtensionPurchaseLimit").AsInt32().Nullable();

        Alter
            .Table("NexportOrderInvoiceItem")
            .AddColumn("RequireManualApproval").AsBoolean().Nullable();

        Alter
            .Table("NexportOrderInvoiceRedemptionQueue")
            .AddColumn("ManualApprovalAction").AsInt32().Nullable();
    }

    public override void Down()
    {
        Delete
            .Column("RenewalDuration")
            .Column("RenewalCompletionThreshold")
            .Column("RenewalApprovalMethod")
            .Column("ExtensionPurchaseLimit")
            .FromTable("NexportProductMapping");

        Delete
            .Column("RequireManualApproval")
            .FromTable("NexportOrderInvoiceItem");

        Delete
            .Column("ManualApprovalAction")
            .FromTable("NexportOrderInvoiceRedemptionQueue");
    }
}
