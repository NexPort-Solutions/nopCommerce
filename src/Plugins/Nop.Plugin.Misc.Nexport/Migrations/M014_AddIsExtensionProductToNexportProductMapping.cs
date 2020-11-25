using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(14, "Add IsExtensionProduct to NexportProductMapping table")]
    [SkipMigration]
    public class M014_AddIsExtensionProductToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = nameof(NexportProductMapping);

        public override void Up()
        {
            Alter.Table(TABLE_NAME)
                .AddColumn("IsExtensionProduct").AsBoolean().WithDefaultValue(false).SetExistingRowsTo(false);
        }

        public override void Down()
        {
            Delete.Column("IsExtensionProduct").FromTable(TABLE_NAME);
        }
    }
}