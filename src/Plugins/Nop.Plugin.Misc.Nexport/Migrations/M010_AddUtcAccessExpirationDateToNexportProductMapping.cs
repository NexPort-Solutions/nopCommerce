using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(10, "Add UtcAccessExpirationDate to NexportProductMapping table")]
    public class M010_AddUtcAccessExpirationDateToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME).AddColumn("UtcAccessExpirationDate").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete.Column("UtcAccessExpirationDate").FromTable(TABLE_NAME);
        }
    }
}