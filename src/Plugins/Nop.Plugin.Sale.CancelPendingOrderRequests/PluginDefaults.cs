namespace Nop.Plugin.Sale.CancelPendingOrderRequests
{
    public class PluginDefaults
    {
        public const string SystemName = "Sale.CancelPendingOrderRequests";

        /// <summary>
        /// Assembly version key for provisioning
        /// </summary>
        public const string ASSEMBLY_VERSION_KEY = "plugin.sale.cancelpendingorderrequests.version";

        /// <summary>
        /// Key for cancellation request reasons
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        public static string CancellationRequestReasonsModelKey => "Nop.sale.cancellationrequestreasons-{0}";

        public static string CancellationRequestReasonsPrefixCacheKey => "Nop.sale.cancellationrequestreasons";

        public const string EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE = "EditCancellationRequest";

        public const string DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE = "DeleteCancellationRequest";

        public const string NEW_CANCELLATION_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE =
            "NewCancellationRequest.StoreOwnerNotification";

        public const string NEW_CANCELLATION_REQUEST_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE =
            "NewCancellationRequest.CustomerNotification";

        public const string CANCELLATION_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE =
            "CancellationRequestAccepted.CustomerNotification";

        public const string CANCELLATION_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE =
            "CancellationRequestRejected.CustomerNotification";
    }
}