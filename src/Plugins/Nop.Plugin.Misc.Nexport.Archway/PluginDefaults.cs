using Nop.Core.Caching;

namespace Nop.Plugin.Misc.Nexport.Archway;

public static class PluginDefaults
{
    public const string SystemName = "Misc.Nexport.Archway";

    /// <summary>
    /// Assembly version key for provisioning
    /// </summary>
    public const string ASSEMBLY_VERSION_KEY = "plugin.misc.nexport.archway.version";
    public const string PLUGIN_MIGRATION_TAG = "ArchwayPluginMigration";
    public const string HTML_FIELD_PREFIX = "Archway";

    public static string UploadPath => "files\\archwaystoredata";
    public static CacheKey ArchwayStoreRecordAllNoPaginationCacheKey { get; } = new("Nop.nexport.archway.storerecord.all");
    public static CacheKey ArchwayStoreEmployeePositionAllNoPaginationCacheKey { get; } = new("Nop.nexport.archway.employeeposition.all");

    public const string CUSTOM_ENROLLMENT_ROUTE_SETTING_KEY = "nexport.archway.enrollment.route";
    public const string CUSTOM_ENROLLMENT_ROUTE_CONTROL_SETTING_KEY = "nexport.archway.enrollment.route.enabled";
}
