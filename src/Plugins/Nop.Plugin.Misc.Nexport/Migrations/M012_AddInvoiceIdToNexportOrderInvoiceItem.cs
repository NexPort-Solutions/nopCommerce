using System;
using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(12, "Add InvoiceId to NexportOrderInvoiceItem table")]
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
}
