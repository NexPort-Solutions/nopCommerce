using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(8, "Add NexportProductGroupMembershipMapping table")]
    [SkipMigration]
    public class M008_AddNexportProductGroupMembershipMappingTable : Migration
    {
        private const string TABLE_NAME = nameof(NexportProductGroupMembershipMapping);

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
}