using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(1, "Initial table creation for Nexport plugin")]
    [SkipMigration]
    public class M001_CreatePluginSchemas : Migration
    {
        private const string TABLE_NAME = nameof(NexportProductMapping);

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("NopProductId").AsInt32()
                .WithColumn("NexportProductName").AsString(255)
                .WithColumn("DisplayName").AsString(255).Nullable()
                .WithColumn("NexportCatalogId").AsGuid().NotNullable()
                .WithColumn("NexportSyllabusId").AsGuid().Nullable()
                .WithColumn("NexportCatalogSyllabusLinkId").AsGuid().Nullable()
                .WithColumn("NexportSubscriptionOrgId").AsGuid().Nullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("PublishingModel").AsInt32().Nullable()
                .WithColumn("PricingModel").AsInt32().Nullable()
                .WithColumn("AccessTimeLimit").AsInt32().Nullable()
                .WithColumn("UtcLastModifiedDate").AsDateTime2().Nullable()
                .WithColumn("UtcAvailableDate").AsDateTime2().Nullable()
                .WithColumn("UtcEndDate").AsDateTime2().Nullable()
                .WithColumn("IsSynchronized").AsBoolean()
                .WithColumn("UtcLastSynchronizationDate").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
