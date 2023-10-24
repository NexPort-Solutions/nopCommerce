using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(Defaults.PLUGIN_MIGRATION_TAG)]
[Migration(24, "Add DisplayOrder to NexportRegistrationFieldOption table")]
[SkipMigration]
public class M024_AddDisplayOrderToNexportRegistrationFieldOption : Migration
{
    public override void Up()
    {
        Alter
            .Table("NexportRegistrationFieldOption")
            .AddColumn("DisplayOrder")
            .AsInt32().NotNullable().SetExistingRowsTo(0);
    }

    public override void Down()
    {
        Delete.Column("DisplayOrder").FromTable("NexportRegistrationFieldOption");
    }
}
