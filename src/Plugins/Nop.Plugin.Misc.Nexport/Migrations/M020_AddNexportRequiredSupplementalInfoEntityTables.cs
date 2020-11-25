using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(20, "Add processing required supplemental info answer entity tables")]
    [SkipMigration]
    public class M020_AddNexportRequiredSupplementalInfoEntityTables : Migration
    {
        private const string PROCESSING_REQUIRED_ANSWER_TABLE_NAME = "NexportSupplementalInfoAnswerProcessingQueue";

        private const string GROUP_MEMBERSHIP_REMOVAL_TABLE_NAME = "NexportGroupMembershipRemovalQueue";

        public override void Up()
        {
            Create.Table(PROCESSING_REQUIRED_ANSWER_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("AnswerId").AsInt32().NotNullable()
                .WithColumn("UtcDateCreated").AsDateTime2().NotNullable();

            Create.Table(GROUP_MEMBERSHIP_REMOVAL_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CustomerId").AsInt32().NotNullable()
                .WithColumn("NexportMembershipId").AsGuid().NotNullable()
                .WithColumn("UtcDateCreated").AsDateTime2().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(PROCESSING_REQUIRED_ANSWER_TABLE_NAME);
            Delete.Table(GROUP_MEMBERSHIP_REMOVAL_TABLE_NAME);
        }
    }
}