using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(32, "Add NexportFundingPool table")]
[SkipMigration]
public class M0032_CreateNexportFundingPool : Migration
{
    private const string TABLE_NAME = "NexportFundingPool";

    public override void Up()
    {
        Create.Table(TABLE_NAME)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255);
    }

    public override void Down()
    {
        Delete.Table(TABLE_NAME);
    }
}
