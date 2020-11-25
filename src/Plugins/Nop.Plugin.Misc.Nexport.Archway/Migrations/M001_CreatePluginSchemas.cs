using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Archway.Migrations
{
    [Tags(PluginDefaults.PluginMigrationTag)]
    [Migration(1, "Initial table creation for Archway plugin")]
    [SkipMigration]
    public class M001_CreatePluginSchemas : Migration
    {
        private const string ARCHWAY_STORE_TABLE_NAME = "ArchwayStore";

        private const string ARCHWAY_STORE_EMPLOYEE_POSITION_TABLE_NAME = "ArchwayStoreEmployeePosition";

        private const string ARCHWAY_STUDENT_REGISTRATION_FIELD_KEY_MAPPING = "ArchwayStudentRegistrationFieldKeyMapping";

        private const string ARCHWAY_STUDENT_REGISTRATION_FIELD_ANSWER = "ArchwayStudentRegistrationFieldAnswer";

        public override void Up()
        {
            Create.Table(ARCHWAY_STORE_TABLE_NAME)
                .WithColumn("StoreNumber").AsInt32().PrimaryKey()
                .WithColumn("OperatorId").AsString(255).NotNullable()
                .WithColumn("RegionCode").AsInt32().NotNullable()
                .WithColumn("Address").AsString(500).Nullable()
                .WithColumn("City").AsString(255).NotNullable()
                .WithColumn("State").AsString(100).NotNullable()
                .WithColumn("PostalCode").AsString(100).NotNullable()
                .WithColumn("AdvertisingCoop").AsString(255).NotNullable()
                .WithColumn("StoreType").AsString(255).NotNullable()
                .WithColumn("OperatorFirstName").AsString(255).Nullable()
                .WithColumn("OperatorLastName").AsString(255).Nullable();

            Create.Table(ARCHWAY_STORE_EMPLOYEE_POSITION_TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("JobCode").AsInt32().NotNullable()
                .WithColumn("JobTitle").AsString(255).NotNullable()
                .WithColumn("JobType").AsString(255).NotNullable()
                .WithColumn("JobLevel").AsString(255).NotNullable();

            Create.Table(ARCHWAY_STUDENT_REGISTRATION_FIELD_KEY_MAPPING)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("FieldId").AsInt32().NotNullable()
                .WithColumn("FieldControlName").AsString(1000).NotNullable()
                .WithColumn("FieldKey").AsString(255).Nullable();

            Create.Table(ARCHWAY_STUDENT_REGISTRATION_FIELD_ANSWER)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CustomerId").AsInt32().NotNullable()
                .WithColumn("FieldId").AsInt32().NotNullable()
                .WithColumn("FieldKey").AsString(255).NotNullable()
                .WithColumn("TextValue").AsString().Nullable()
                .WithColumn("UtcDateCreated").AsDateTime2().NotNullable()
                .WithColumn("UtcDateModified").AsDateTime2().Nullable();

            var storeEmployeePositionDataInsertion = @"
                SET IDENTITY_INSERT [dbo].[ArchwayStoreEmployeePosition] ON
                GO
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (1, 641, N'General Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (2, 0, N'Restaurant Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (4, 0, N'Department Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (5, 0, N'First Assistant Manager ', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (6, 0, N'Second Assistant Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (7, 0, N'Assistant Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (8, 647, N'Certified Swing Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (9, 0, N'Shift Manager', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (10, 0, N'Office Staff', N'OO', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (11, 650, N'Crew Person', N'OO', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (12, 646, N'Floor Supervisor', N'OO', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (13, 648, N'Crew Trainer', N'OO', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (14, 671, N'Primary Maintenance', N'OO', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (15, 670, N'Backup Maintenance', N'OO', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (16, 739, N'Shift Manager Trainee', N'OO', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (17, 641, N'General Manager', N'MCD', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (18, 644, N'Store Mgr Trainee-F', N'MCD', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (19, 646, N'Floor Supervisor', N'MCD', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (20, 647, N'Certified Swing Manager', N'MCD', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (21, 648, N'Crew Trainer', N'MCD', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (22, 650, N'Crew Person', N'MCD', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (23, 670, N'Backup Maintenance', N'MCD', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (24, 671, N'Primary Maintenance', N'MCD', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (25, 739, N'Shift Manager Trainee', N'MCD', N'Crew');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (26, 2110, N'Department Manager I', N'MCD', N'Management')
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (27, 2111, N'Department Manager II', N'MCD', N'Management');
                INSERT [dbo].[ArchwayStoreEmployeePosition] ([Id], [JobCode], [JobTitle], [JobType], [JobLevel]) VALUES (28, 2112, N'Department Manager III', N'MCD', N'Management');
                SET IDENTITY_INSERT [dbo].[ArchwayStoreEmployeePosition] OFF
                GO
            ";

            Execute.Sql(storeEmployeePositionDataInsertion);
        }

        public override void Down()
        {
            Delete.Table(ARCHWAY_STORE_TABLE_NAME);
            Delete.Table(ARCHWAY_STORE_EMPLOYEE_POSITION_TABLE_NAME);
            Delete.Table(ARCHWAY_STUDENT_REGISTRATION_FIELD_KEY_MAPPING);
            Delete.Table(ARCHWAY_STUDENT_REGISTRATION_FIELD_ANSWER);
        }
    }
}
