using FluentMigrator.Runner.VersionTableInfo;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Migrations
{
    [VersionTableMetaData]
    public class CancelPendingOrderRequestsPluginMigrationVersionTable : IVersionTableMetaData
    {
        public object ApplicationContext { get; set; }

        public virtual string SchemaName => "";

        public virtual string TableName => "CancelPendingOrderRequestsPluginMigrationVersionInfo";

        public virtual string ColumnName => "Version";

        public virtual string UniqueIndexName => "UC_Version";

        public virtual string AppliedOnColumnName => "AppliedOn";

        public virtual string DescriptionColumnName => "Description";

        public virtual bool OwnsSchema => true;
    }
}