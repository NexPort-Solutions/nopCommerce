using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(24, "Add DisplayOrder to NexportRegistrationFieldOption table")]
    [SkipMigration]
    public class M024_AddDisplayOrderToNexportRegistrationFieldOption : Migration
    {
        public override void Up()
        {
            Alter
                .Table(nameof(NexportRegistrationFieldOption))
                .AddColumn("DisplayOrder")
                .AsInt32().NotNullable().SetExistingRowsTo(0);
        }

        public override void Down()
        {
            Delete.Column("DisplayOrder").FromTable(nameof(NexportRegistrationFieldOption));
        }
    }
}