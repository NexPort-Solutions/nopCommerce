using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(44, "Add AllowPurchaseWithExistingEnrollment to NexportProductMapping table")]
[SkipMigration]
public class M044_AddAllowPurchaseWithExistingEnrollmentToNexportProductMapping : Migration
{
    private const string TABLE_NAME = "NexportProductMapping";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("AllowPurchaseWithExistingEnrollment").AsBoolean().SetExistingRowsTo(false);
    }

    public override void Down()
    {
        Delete.Column("AllowPurchaseWithExistingEnrollment").FromTable(TABLE_NAME);
    }
}