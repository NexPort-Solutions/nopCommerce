using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Archway.Migrations;

[Tags(PluginDefaults.PluginMigrationTag)]
[Migration(3, "Change StoreNumber in ArchwayStore to string type")]
[SkipMigration]
public class M003_ChangeStoreNumberColumnToStringType : Migration
{
    private const string TABLE_NAME = "ArchwayStore";

    public override void Up()
    {
        Rename.Column("StoreNumber").OnTable(TABLE_NAME).To("Id");
        Alter.Table(TABLE_NAME).AddColumn("StoreNumber").AsString(10).NotNullable().SetExistingRowsTo("00000");

        Execute.Sql("Update ArchwayStore Set StoreNumber = CAST([Id] AS NVARCHAR(5))");
    }

    public override void Down()
    {
    }
}