using FluentMigrator.Runner.VersionTableInfo;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Migrations;

[VersionTableMetaData]
public class CancelPendingOrderRequestsPluginMigrationVersionTable : IVersionTableMetaData
{
    public object ApplicationContext { get; set; }

    public string SchemaName => "";

    public string TableName => "CancelPendingOrderRequestsPluginMigrationVersionInfo";

    public string ColumnName => "Version";

    public string UniqueIndexName => "UC_Version";

    public string AppliedOnColumnName => "AppliedOn";

    public bool CreateWithPrimaryKey => false;

    public string DescriptionColumnName => "Description";

    public bool OwnsSchema => true;
}