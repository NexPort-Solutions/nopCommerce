using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(19, "Add NexportSupplementalInfo related entity tables")]
[SkipMigration]
public class M019_AddNexportSupplementalInfoEntityTables : Migration
{
    public override void Up()
    {
        Create.Table("NexportSupplementalInfoQuestion")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("QuestionText").AsString(int.MaxValue).NotNullable()
            .WithColumn("Description").AsString(1000).Nullable()
            .WithColumn("Type").AsInt32().NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
            .WithColumn("UtcDateModified").AsDateTime2().Nullable();

        Create.Table("NexportSupplementalInfoQuestionMapping")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("QuestionId").AsInt32().NotNullable()
            .WithColumn("ProductMappingId").AsInt32().NotNullable()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
            .WithColumn("UtcDateModified").AsDateTime2().Nullable();

        Create.Table("NexportSupplementalInfoOption")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("QuestionId").AsInt32().NotNullable()
            .WithColumn("OptionText").AsString(int.MaxValue).NotNullable()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
            .WithColumn("UtcDateModified").AsDateTime2().Nullable()
            .WithColumn("Deleted").AsBoolean().NotNullable();

        Create.Table("NexportSupplementalInfoOptionGroupAssociation")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("OptionId").AsInt32().NotNullable()
            .WithColumn("NexportGroupId").AsGuid().NotNullable()
            .WithColumn("NexportGroupName").AsString().NotNullable()
            .WithColumn("NexportGroupShortName").AsString(50).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
            .WithColumn("UtcDateModified").AsDateTime2().Nullable();

        Create.Table("NexportSupplementalInfoAnswer")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("StoreId").AsInt32().NotNullable()
            .WithColumn("QuestionId").AsInt32().NotNullable()
            .WithColumn("OptionId").AsInt32().NotNullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("UtcDateProcessed").AsDateTime2().Nullable()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
            .WithColumn("UtcDateModified").AsDateTime2().Nullable();

        Create.Table("NexportSupplementalInfoAnswerMembership")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("AnswerId").AsInt32().NotNullable()
            .WithColumn("NexportMembershipId").AsGuid().NotNullable();

        Create.Table("NexportRequiredSupplementalInfo")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("StoreId").AsInt32().NotNullable()
            .WithColumn("QuestionId").AsInt32().NotNullable()
            .WithColumn("UtcDateCreated").AsDateTime2().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("NexportSupplementalInfoQuestion");
        Delete.Table("NexportSupplementalInfoQuestionMapping");
        Delete.Table("NexportSupplementalInfoOption");
        Delete.Table("NexportSupplementalInfoOptionGroupAssociation");
        Delete.Table("NexportSupplementalInfoAnswer");
        Delete.Table("NexportSupplementalInfoAnswerMembership");
        Delete.Table("NexportRequiredSupplementalInfo");
    }
}
