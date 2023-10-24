using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Archway.Migrations;

[Tags(PluginDefaults.PLUGIN_MIGRATION_TAG)]
[Migration(2, "Remove FieldId from StudentRegistrationFieldKeyMapping table")]
[SkipMigration]
public class M002_RemoveFieldIdFromArchwayStudentRegistrationFieldKeyMapping : Migration
{
    private const string TABLE_NAME = "StudentRegistrationFieldKeyMapping";

    public override void Up() => Delete.Column("FieldId").FromTable(TABLE_NAME);

    public override void Down()
    {
    }
}
