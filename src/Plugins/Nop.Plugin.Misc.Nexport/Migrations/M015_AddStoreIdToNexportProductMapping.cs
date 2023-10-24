using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(15, "Add StoreId to NexportProductMapping table")]
[SkipMigration]
public class M015_AddStoreIdToNexportProductMapping : Migration
{
    private const string TABLE_NAME = "NexportProductMapping";

    public override void Up()
    {
        Alter.Table(TABLE_NAME).AddColumn("StoreId").AsInt32().Nullable();
    }

    public override void Down()
    {
        Delete.Column("StoreId").FromTable(TABLE_NAME);
    }
}
