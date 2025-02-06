using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(40, "Add UtcProcessingDate to NexportOrderProcessingQueue table")]
[SkipMigration]
public class M040_AddUtcProcessingDateToNexportOrderProcessingQueue : Migration
{
    private const string TABLE_NAME = "NexportOrderProcessingQueue";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("UtcProcessingDate").AsDateTime2().Nullable();
    }

    public override void Down()
    {
        Delete.Column("UtcProcessingDate").FromTable(TABLE_NAME);
    }
}