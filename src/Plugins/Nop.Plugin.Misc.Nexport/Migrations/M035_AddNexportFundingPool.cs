using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(35, "Add NexportFundingPool table")]
[SkipMigration]
public class M035_AddNexportFundingPool : Migration
{
    private const string TABLE_NAME = "NexportFundingPool";

    public override void Up()
    {
        Create.Table(TABLE_NAME)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255)
            .WithColumn("Description").AsString(1000).Nullable()
            .WithColumn("Code").AsString(1000).NotNullable().Unique()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
            .WithColumn("UtcDateModified").AsDateTime2().Nullable();
    }

    public override void Down()
    {
        Delete.Table(TABLE_NAME);
    }
}