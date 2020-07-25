using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(21, "Add NexportRegistrationField related entity tables")]
    public class M021_AddNexportRegistrationFieldEntityTables : Migration
    {
        private const string REGISTRATION_FIELD_TABLE_NAME = "NexportRegistrationField";

        private const string REGISTRATION_FIELD_CATEGORY_TABLE_NAME = "NexportRegistrationFieldCategory";

        private const string REGISTRATION_FIELD_OPTION_TABLE_NAME = "NexportRegistrationFieldOption";

        private const string REGISTRATION_FIELD_STORE_MAPPING_TABLE_NAME = "NexportRegistrationFieldStoreMapping";

        private const string REGISTRATION_FIELD_ANSWER_TABLE_NAME = "NexportRegistrationFieldAnswer";

        private const string REGISTRATION_FIELD_SYNCHRONIZATION_QUEUE_TABLE_NAME = "NexportRegistrationFieldSynchronizationQueue";

        public override void Up()
        {
            Create.Table(REGISTRATION_FIELD_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("NexportCustomProfileFieldKey").AsString(255).Nullable()
                .WithColumn("IsRequired").AsBoolean().NotNullable()
                .WithColumn("IsActive").AsBoolean().NotNullable()
                .WithColumn("Validation").AsBoolean().NotNullable()
                .WithColumn("ValidationRegex").AsString(1000).Nullable()
                .WithColumn("FieldCategoryId").AsInt32().Nullable()
                .WithColumn("DisplayOrder").AsInt32().NotNullable();

            Create.Table(REGISTRATION_FIELD_CATEGORY_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Title").AsString(255).NotNullable()
                .WithColumn("Description").AsString(2000).Nullable()
                .WithColumn("DisplayOrder").AsInt32().NotNullable();

            Create.Table(REGISTRATION_FIELD_OPTION_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("FieldId").AsInt32().NotNullable()
                .WithColumn("OptionValue").AsString().NotNullable();

            Create.Table(REGISTRATION_FIELD_STORE_MAPPING_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("FieldId").AsInt32().NotNullable()
                .WithColumn("StoreId").AsInt32().NotNullable();

            Create.Table(REGISTRATION_FIELD_ANSWER_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CustomerId").AsInt32().NotNullable()
                .WithColumn("FieldId").AsInt32().NotNullable()
                .WithColumn("TextValue").AsString().Nullable()
                .WithColumn("NumericValue").AsInt32().Nullable()
                .WithColumn("DateTimeValue").AsDateTime2().Nullable()
                .WithColumn("BooleanValue").AsBoolean().Nullable()
                .WithColumn("FieldOptionId").AsInt32().Nullable()
                .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
                .WithColumn("UtcDateModified").AsDateTime2().Nullable();

            Create.Table(REGISTRATION_FIELD_SYNCHRONIZATION_QUEUE_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CustomerId").AsInt32().NotNullable()
                .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
                .WithColumn("UtcDateLastAttempt").AsDateTime2().Nullable()
                .WithColumn("Attempt").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(REGISTRATION_FIELD_TABLE_NAME);
            Delete.Table(REGISTRATION_FIELD_CATEGORY_TABLE_NAME);
            Delete.Table(REGISTRATION_FIELD_OPTION_TABLE_NAME);
            Delete.Table(REGISTRATION_FIELD_STORE_MAPPING_TABLE_NAME);
            Delete.Table(REGISTRATION_FIELD_ANSWER_TABLE_NAME);
            Delete.Table(REGISTRATION_FIELD_SYNCHRONIZATION_QUEUE_TABLE_NAME);
        }
    }
}
