using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations;

[Tags(NexportDefaults.PluginMigrationTag)]
[Migration(47, "Add previous enrollment expiration date to NexportRedemptionAssignmentApprovalRequest table")]
[SkipMigration]
public class M047_AddUtcPreviousEnrollmentExpirationToNexportRedemptionAssignmentApprovalRequest : Migration
{
    private const string TABLE_NAME = "NexportRedemptionAssignmentApprovalRequest";

    public override void Up()
    {
        Alter.Table(TABLE_NAME)
            .AddColumn("UtcPreviousEnrollmentExpirationDate").AsDateTime2().Nullable();
    }

    public override void Down()
    {
        Delete.Column("UtcPreviousEnrollmentExpirationDate").FromTable(TABLE_NAME);
    }
}