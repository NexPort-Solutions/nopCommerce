using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(12, "Add InvoiceId to NexportOrderInvoiceItem table")]
[SkipMigration]
public class M012_AddInvoiceIdToNexportOrderInvoiceItem : Migration
{
    private const string TABLE_NAME = "NexportOrderInvoiceItem";

    public override void Up()
    {
        Alter.Table(TABLE_NAME).AddColumn("InvoiceId").AsGuid().NotNullable()
            .SetExistingRowsTo(new Guid());
    }

    public override void Down()
    {
        Delete.Column("InvoiceId").FromTable(TABLE_NAME);
    }
}
