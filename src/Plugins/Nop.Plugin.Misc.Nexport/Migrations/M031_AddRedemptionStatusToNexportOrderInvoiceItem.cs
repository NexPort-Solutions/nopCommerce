using System;
using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(31, "Add RedemptionStatus to NexportOrderInvoiceItem table")]
    [SkipMigration]
    public class M031_AddRedemptionStatusToNexportOrderInvoiceItem : Migration
    {
        private const string TABLE_NAME = "NexportOrderInvoiceItem";

        public override void Up()
        {
            Alter.Table(TABLE_NAME).AddColumn("RedemptionStatusId").AsInt32().NotNullable()
                .SetExistingRowsTo(0);
        }

        public override void Down()
        {
            Delete.Column("RedemptionStatusId").FromTable(TABLE_NAME);
        }
    }
}
