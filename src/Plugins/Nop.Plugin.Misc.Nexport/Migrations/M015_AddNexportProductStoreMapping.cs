using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(15, "Add NexportProductStoreMapping table")]
    public class M015_AddNexportProductStoreMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductStoreMapping";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("NexportProductMappingId").AsInt32().NotNullable()
                .WithColumn("StoreId").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
