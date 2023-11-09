using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(28, "Add NexportFundingPool table")]
[SkipMigration]
public class M0028_AddNexportFundingPool : Migration
{
    private const string TABLE_NAME = "NexportFundingPool";

    public override void Up()
    {
        Create.Table(TABLE_NAME)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsInt32();
    }

    public override void Down()
    {
        Delete.Table(TABLE_NAME);
    }
}
