using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(42, "Add NexportRedemptionAuditLog table")]
[SkipMigration]
public class M042_AddNexportRedemptionAuditLog : Migration
{
    private const string TABLE_NAME = "NexportRedemptionAuditLog";

    public override void Up()
    {
        Create.Table(TABLE_NAME)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("InvoiceItemId").AsGuid()
            .WithColumn("Description").AsString(int.MaxValue)
            .WithColumn("CustomerId").AsInt32()
            .WithColumn("TargetedCustomerId").AsInt32().Nullable()
            .WithColumn("Type").AsInt32()
            .WithColumn("UtcDateCreated").AsDateTime2();
    }

    public override void Down()
    {
        Delete.Table(TABLE_NAME);
    }
}