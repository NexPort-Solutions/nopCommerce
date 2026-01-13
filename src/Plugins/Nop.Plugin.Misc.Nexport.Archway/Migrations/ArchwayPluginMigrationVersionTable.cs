using FluentMigrator.Runner.VersionTableInfo;

namespace Nop.Plugin.Misc.Nexport.Archway.Migrations;

[VersionTableMetaData]
public class ArchwayPluginMigrationVersionTable: IVersionTableMetaData
{
    public object ApplicationContext { get; set; }

    public string SchemaName => "";

    public string TableName => "ArchwayPluginMigrationVersionInfo";

    public string ColumnName => "Version";

    public string UniqueIndexName => "UC_Version";

    public string AppliedOnColumnName => "AppliedOn";

    public bool CreateWithPrimaryKey => false;

    public string DescriptionColumnName => "Description";

    public virtual bool OwnsSchema => true;
}