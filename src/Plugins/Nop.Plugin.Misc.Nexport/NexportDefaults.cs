using Nop.Plugin.Misc.Nexport.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport
{
    public class NexportDefaults
    {
        public const string ProviderSystemName = "ExternalAuth.Nexport";

        /// <summary>
        /// Name of the view component to display plugin in public store
        /// </summary>
        public const string ViewComponentName = "NexportAuthentication";

        /// <summary>
        /// Name of the Nexport redemption processing schedule task
        /// </summary>
        public static string NexportOrderProcessingTaskName => "Processing Nexport orders";

        /// <summary>
        /// Type of the Nexport redemption processing schedule task
        /// </summary>
        public static string NexportOrderProcessingTask => $"{typeof(NexportOrderProcessingTask).Namespace}.{nameof(NexportOrderProcessingTask)}";

        /// <summary>
        /// Nexport redemption processing interval (in seconds)
        /// </summary>
        public static int NexportOrderProcessingTaskInterval => 5;

        /// <summary>
        /// Nexport redemption processing default batch size
        /// </summary>
        public static int NexportOrderProcessingTaskBatchSize => 100;

        /// <summary>
        /// Nexport redemption processing batch size setting key
        /// </summary>
        public static string NexportOrderProcessingTaskBatchSizeSettingKey => "nexportsettings.tasks.orderprocessing.batchsize";

        /// <summary>
        /// Name of the Nexport synchronization schedule task
        /// </summary>
        public static string NexportSynchronizationTaskName => "Synchronize with Nexport";

        /// <summary>
        /// Type of the Nexport synchronization schedule task
        /// </summary>
        public static string NexportSynchronizationTask => $"{typeof(NexportSynchronizationTask).Namespace}.{nameof(NexportSynchronizationTask)}";

        /// <summary>
        /// Nexport synchronization task interval (in seconds)
        /// </summary>
        public static int NexportSynchronizationTaskInterval => 20 * 60;

        /// <summary>
        /// Nexport synchronization default batch size
        /// </summary>
        public static int NexportSynchronizationTaskBatchSize => 100;

        /// <summary>
        /// Nexport synchronization batch size setting key
        /// </summary>
        public static string NexportSynchronizationTaskBatchSizeSettingKey => "nexportsettings.tasks.productsynchronization.batchsize";

        /// <summary>
        /// Name of the Nexport order invoice redemption schedule task
        /// </summary>
        public static string NexportOrderInvoiceRedemptionTaskName => "Redeem Nexport invoice item";

        /// <summary>
        /// Type of the Nexport order invoice redemption schedule task
        /// </summary>
        public static string NexportOrderInvoiceRedemptionTask => $"{typeof(NexportInvoiceRedemptionTask).Namespace}.{nameof(NexportInvoiceRedemptionTask)}";

        /// <summary>
        /// Nexport order invoice redemption task interval (in seconds)
        /// </summary>
        public static int NexportOrderInvoiceRedemptionTaskInterval => 5;

        /// <summary>
        /// Nexport order invoice redemption default batch size
        /// </summary>
        public static int NexportOrderInvoiceRedemptionTaskBatchSize => 100;

        /// <summary>
        /// Nexport order invoice redemption batch size setting key
        /// </summary>
        public static string NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey => "nexportsettings.tasks.orderinvoiceredemption.batchsize";

        /// <summary>
        /// Assembly version key for provisioning
        /// </summary>
        public const string ASSEMBLY_VERSION_KEY = "nexportsettings.plugins.nexport.version";

        public const string NEXPORT_SUBSCRIPTION_ORGANIZATION_ID_SETTING_KEY = "NexportSubscriptionOrganizationId";

        public const string NEXPORT_STORE_SALE_MODEL_SETTING_KEY = "NexportStoreSaleModel";

        public const string ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY = "AllowRepurchaseFailedCoursesFromNexport";

        public const string ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY = "AllowRepurchasePassedCoursesFromNexport";
    }
}
