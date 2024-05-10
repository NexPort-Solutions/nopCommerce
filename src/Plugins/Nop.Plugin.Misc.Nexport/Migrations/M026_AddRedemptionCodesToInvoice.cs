using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(26, "Add Redemption codes to invoice table")]
    [SkipMigration]
    public class M026_AddRedemptionCodesToInvoice : Migration
    {

        public override void Up()
        {
            Alter
                .Table(nameof(NexportOrderInvoiceItem))
                .AddColumn("InvoiceRedemptionCode").AsString(255).Nullable()
                .AddColumn("InvoiceItemRedemptionCode").AsString(255).Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("InvoiceRedemptionCode")
                .Column("InvoiceItemRedemptionCode")
                .FromTable(nameof(NexportOrderInvoiceItem));
        }
    }
}