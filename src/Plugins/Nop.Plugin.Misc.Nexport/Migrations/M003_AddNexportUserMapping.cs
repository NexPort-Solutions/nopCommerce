using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(3, "Add MapNexportUser table")]
    public class M003_AddNexportUserMapping : Migration
    {
        private const string TABLE_NAME = "NexportUserMapping";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("NopUserId").AsInt32()
                .WithColumn("NexportUserId").AsGuid()
                .WithColumn("UtcDateSynchronize").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
