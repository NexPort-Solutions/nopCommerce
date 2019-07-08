using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(5, "Add CreditHours to NexportProductMapping table")]
    public class M005_AddCreditHoursToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME)
                .AddColumn("CreditHours").AsDecimal().Nullable();
        }

        public override void Down()
        {
            Delete.Column("CreditHours");
        }
    }
}