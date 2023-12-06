using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(33, "Add properties to NexportFundingPool table")]
[SkipMigration]
public class M0033_AddNexportFundingPoolCodeAndDescription: Migration
{
    private const string TABLE_NAME = "NexportFundingPool";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("Description")
                .AsString()
                .Nullable()
                .SetExistingRowsTo(null)
            .AddColumn("Code")
                .AsString()
                .NotNullable()
                .SetExistingRowsTo("example code from migration")
                .Unique();
    }

    public override void Down()
    {
        Delete.Column("Description").FromTable(TABLE_NAME);
        Delete.Column("Code").FromTable(TABLE_NAME);
    }
}
