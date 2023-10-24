using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(8, "Add NexportProductGroupMembershipMapping table")]
[SkipMigration]
public class M008_AddNexportProductGroupMembershipMappingTable : Migration
{
    private const string TABLE_NAME = "NexportProductGroupMembershipMapping";

    public override void Up()
    {
        Create.Table(TABLE_NAME)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("NexportProductMappingId").AsInt32().NotNullable()
            .WithColumn("NexportGroupId").AsGuid().NotNullable()
            .WithColumn("NexportGroupName").AsString().NotNullable()
            .WithColumn("NexportGroupShortName").AsString(50).NotNullable();
    }

    public override void Down()
    {
        Delete.Table(TABLE_NAME);
    }
}
