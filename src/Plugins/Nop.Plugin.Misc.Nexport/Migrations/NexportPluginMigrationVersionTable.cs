using FluentMigrator.Runner.VersionTableInfo;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [VersionTableMetaData]
    public class NexportPluginMigrationVersionTable : IVersionTableMetaData
    {
        public object ApplicationContext { get; set; }

        public virtual string SchemaName => "";

        public virtual string TableName => "NexportPluginMigrationVersionInfo";

        public virtual string ColumnName => "Version";

        public virtual string UniqueIndexName => "UC_Version";

        public virtual string AppliedOnColumnName => "AppliedOn";

        public virtual string DescriptionColumnName => "Description";

        public virtual bool OwnsSchema => true;
    }
}
