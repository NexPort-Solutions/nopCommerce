namespace Nop.Plugin.Misc.Nexport.Archway
{
    public class PluginDefaults
    {
        public const string SystemName = "Misc.Nexport.Archway";

        /// <summary>
        /// Assembly version key for provisioning
        /// </summary>
        public const string ASSEMBLY_VERSION_KEY = "plugin.misc.nexport.archway.version";

        public const string PluginMigrationTag = "ArchwayPluginMigration";

        public const string HtmlFieldPrefix = "Archway";

        public static string UploadPath => "files\\archwaystoredata";

        public static string ArchwayStoreRecordAllNoPaginationCacheKey => "Nop.nexport.archway.storerecord.all";

        public static string ArchwayStoreEmployeePositionAllNoPaginationCacheKey => "Nop.nexport.archway.employeeposition.all";

        public static string CustomEnrollmentRouteSettingKey = "nexport.archway.enrollment.route";

        public static string CustomEnrollmentRouteControlSettingKey = "nexport.archway.enrollment.route.enabled";
    }
}
