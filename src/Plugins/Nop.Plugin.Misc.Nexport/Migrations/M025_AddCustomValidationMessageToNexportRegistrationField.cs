using FluentMigrator;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(25, "Add ValidationMessage to Nexport registration field table")]
    public class M025_AddValidationMessageToNexportRegistrationField : Migration
    {
        public override void Up()
        {
            Alter
                .Table(nameof(NexportRegistrationField))
                .AddColumn("ValidationMessage").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("ValidationMessage")
                .FromTable(nameof(NexportRegistrationField));
        }
    }
}