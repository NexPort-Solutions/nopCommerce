using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Archway.Migrations
{
    [Tags(PluginDefaults.PluginMigrationTag)]
    [Migration(2, "Remove FieldId from ArchwayStudentRegistrationFieldKeyMapping table")]
    public class M002_RemoveFieldIdFromArchwayStudentRegistrationFieldKeyMapping : Migration
    {
        private const string TABLE_NAME = "ArchwayStudentRegistrationFieldKeyMapping";

        public override void Up()
        {
            Delete.Column("FieldId").FromTable(TABLE_NAME);
        }

        public override void Down()
        {
        }
    }
}