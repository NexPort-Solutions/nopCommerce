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
        public static int NexportOrderProcessingTaskInterval => 30;

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
        /// Assembly version key for provisioning
        /// </summary>
        public const string AssemblyVersionKey = "nexportsettings.plugins.nexport.version";
    }
}
