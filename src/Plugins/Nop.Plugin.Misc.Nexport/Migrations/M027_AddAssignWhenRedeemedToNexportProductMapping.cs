using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(27, "Add AssignWhenRedeemed To NexportProductMapping")]
    [SkipMigration]
    public class M027_AddAssignWhenRedeemedToNexportProductMapping : Migration
    {

        public override void Up()
        {
            Alter
                .Table(nameof(NexportProductMapping))
                .AddColumn("AssignWhenRedeemed").AsBoolean().Nullable();

        }

        public override void Down()
        {
            Delete
                .Column("AssignWhenRedeemed")
                .FromTable(nameof(NexportProductMapping));
        }
    }
}