using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(6, "Add SectionCeus to NexportProductMapping table")]
    [SkipMigration]
    public class M006_AddSectionCeusToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME)
                .AddColumn("SectionCeus").AsString(64).Nullable();
        }

        public override void Down()
        {
            Delete.Column("SectionCeus").FromTable(TABLE_NAME);
        }
    }
}