using FluentMigrator.Runner.VersionTableInfo;

namespace Nop.Plugin.Misc.Nexport.Migrations;

public class NexportPluginMigrationVersionTable : IVersionTableMetaData
{
    public object ApplicationContext { get; set; }

    public string SchemaName => "";

    public string TableName => "NexportPluginMigrationVersionInfo";

    public string ColumnName => "Version";

    public string UniqueIndexName => "UC_Version";

    public string AppliedOnColumnName => "AppliedOn";

    public bool CreateWithPrimaryKey => false;

    public string DescriptionColumnName => "Description";

    public bool OwnsSchema => true;
}