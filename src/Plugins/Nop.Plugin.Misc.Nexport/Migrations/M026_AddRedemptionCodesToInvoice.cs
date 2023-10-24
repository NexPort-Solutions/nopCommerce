using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(26, "Add Redemption codes to invoice table")]
[SkipMigration]
public class M026_AddRedemptionCodesToInvoice : Migration
{
    public override void Up()
    {
        Alter
            .Table("NexportOrderInvoiceItem")
            .AddColumn("InvoiceRedemptionCode").AsString(255).Nullable()
            .AddColumn("InvoiceItemRedemptionCode").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete
            .Column("InvoiceRedemptionCode")
            .Column("InvoiceItemRedemptionCode")
            .FromTable("NexportOrderInvoiceItem");
    }
}
