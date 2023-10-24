using Nop.Plugin.Misc.Nexport.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport;

public static class Defaults
{
    public const int ZERO_SECONDS = 0;
    public const string HOMEPAGE = "Homepage";
    public const string GROUP_FOR_ORDER = "GroupForOrder";
    public const string GROUP_FOR_CUSTOMER = "GroupForCustomer";
    public const string PLUGIN_MIGRATION_TAG = "PluginMigration";
    public const string NEXPORT_PLUGIN_VIEW_BASE_PATH = "~/Plugins/Misc.Nexport/Views/";
    public const string NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH = "~/Plugins/Misc.Nexport/Areas/Admin/Views/";

    /// <summary>
    /// Name of the NexPort redemption processing schedule task
    /// </summary>
    public static string OrderProcessingTaskName => "Processing NexPort orders";

    /// <summary>
    /// Type of the NexPort redemption processing schedule task
    /// </summary>
    public static string OrderProcessingTaskType => $"{typeof(OrderProcessingTask).Namespace}.{nameof(OrderProcessingTask)}";

    /// <summary>
    /// NexPort redemption processing interval (in seconds)
    /// </summary>
    public static int OrderProcessingTaskInterval => 5;

    /// <summary>
    /// NexPort redemption processing default batch size
    /// </summary>
    public static int OrderProcessingTaskBatchSize => 100;

    /// <summary>
    /// NexPort redemption processing batch size setting key
    /// </summary>
    public static string OrderProcessingTaskBatchSizeSettingKey => "nexportsettings.tasks.orderprocessing.batchsize";

    /// <summary>
    /// Name of the NexPort synchronization schedule task
    /// </summary>
    public static string SynchronizationTaskName => "Synchronize with Nexport";

    /// <summary>
    /// Type of the NexPort synchronization schedule task
    /// </summary>
    public static string SynchronizationTaskType => $"{typeof(SynchronizationTask).Namespace}.{nameof(SynchronizationTask)}";

    /// <summary>
    /// NexPort synchronization task interval (in seconds)
    /// </summary>
    public static int SynchronizationTaskInterval => 20 * 60;

    /// <summary>
    /// NexPort synchronization default batch size
    /// </summary>
    public static int SynchronizationTaskBatchSize => 100;

    /// <summary>
    /// NexPort synchronization batch size setting key
    /// </summary>
    public static string SynchronizationTaskBatchSizeSettingKey => "nexportsettings.tasks.productsynchronization.batchsize";

    /// <summary>
    /// Name of the NexPort order invoice redemption schedule task
    /// </summary>
    public static string OrderInvoiceRedemptionTaskName => "Redeem NexPort invoice item";

    /// <summary>
    /// Type of the NexPort order invoice redemption schedule task
    /// </summary>
    public static string OrderInvoiceRedemptionTaskType => $"{typeof(InvoiceRedemptionTask).Namespace}.{nameof(InvoiceRedemptionTask)}";

    /// <summary>
    /// NexPort order invoice redemption task interval (in seconds)
    /// </summary>
    public static int OrderInvoiceRedemptionTaskInterval => 5;

    /// <summary>
    /// NexPort order invoice redemption default batch size
    /// </summary>
    public static int OrderInvoiceRedemptionTaskBatchSize => 100;

    /// <summary>
    /// NexPort order invoice redemption batch size setting key
    /// </summary>
    public static string OrderInvoiceRedemptionTaskBatchSizeSettingKey => "nexportsettings.tasks.orderinvoiceredemption.batchsize";

    /// <summary>
    /// Name of the NexPort supplemental info answer processing schedule task
    /// </summary>
    public static string SupplementalInfoAnswerProcessingTaskName => "Processing NexPort supplemental info answers";

    /// <summary>
    /// Type of the NexPort supplemental info answer processing schedule task
    /// </summary>
    public static string SupplementalInfoAnswerProcessingTaskType => $"{typeof(SupplementalInfoAnswerProcessingTask).Namespace}.{nameof(SupplementalInfoAnswerProcessingTask)}";

    /// <summary>
    /// NexPort supplemental info answer processing interval (in seconds)
    /// </summary>
    public static int SupplementalInfoAnswerProcessingTaskInterval => 5;

    /// <summary>
    /// NexPort supplemental info answer processing default task batch size
    /// </summary>
    public static int SupplementalInfoAnswerProcessingTaskBatchSize => 100;

    /// <summary>
    /// NexPort supplemental info answer processing batch size setting key
    /// </summary>
    public static string SupplementalInfoAnswerProcessingTaskBatchSizeSettingKey => "nexportsettings.tasks.supplementalinfoanswerprocessing.batchsize";

    /// <summary>
    /// Name of the NexPort group membership removal schedule task
    /// </summary>
    public static string GroupMembershipRemovalTaskName => "Processing the removal of NexPort group membership";

    /// <summary>
    /// Type of the NexPort group membership removal schedule task
    /// </summary>
    public static string GroupMembershipRemovalTaskType => $"{typeof(GroupMembershipRemovalTask).Namespace}.{nameof(GroupMembershipRemovalTask)}";

    /// <summary>
    /// NexPort group membership removal interval (in seconds)
    /// </summary>
    public static int GroupMembershipRemovalTaskInterval => 5;

    /// <summary>
    /// NexPort group membership removal default task batch size
    /// </summary>
    public static int GroupMembershipRemovalTaskBatchSize => 100;

    /// <summary>
    /// NexPort group membership removal batch size setting key
    /// </summary>
    public static string GroupMembershipRemovalTaskBatchSizeSettingKey => "nexportsettings.tasks.groupmembershipremoval.batchsize";

    /// <summary>
    /// Name of the NexPort registration field synchronization schedule task
    /// </summary>
    public static string RegistrationFieldSynchronizationTaskName => "Synchronize registration fields with Nexport";

    /// <summary>
    /// Type of the NexPort registration field synchronization schedule task
    /// </summary>
    public static string RegistrationFieldSynchronizationTaskType => $"{typeof(RegistrationFieldSynchronizationTask).Namespace}.{nameof(RegistrationFieldSynchronizationTask)}";

    /// <summary>
    /// NexPort registration field synchronization task interval (in seconds)
    /// </summary>
    public static int RegistrationFieldSynchronizationTaskInterval => 5;

    /// <summary>
    /// NexPort registration field  synchronization default batch size
    /// </summary>
    public static int RegistrationFieldSynchronizationTaskBatchSize => 100;

    /// <summary>
    /// NexPort registration field synchronization batch size setting key
    /// </summary>
    public static string RegistrationFieldSynchronizationTaskBatchSizeSettingKey => "nexportsettings.tasks.registrationfieldsynchronization.batchsize";

    /// <summary>
    /// Assembly version key for provisioning
    /// </summary>
    public const string ASSEMBLY_VERSION_KEY = "nexportsettings.plugins.nexport.version";
    public const string SUBSCRIPTION_ORGANIZATION_ID_SETTING_KEY = "SubscriptionOrganizationId";
    public const string STORE_SALE_MODEL_SETTING_KEY = "StoreSaleModel";
    public const string HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY = "HideSectionCEUsInProductPage";
    public const string HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY = "HideAddToCartForIneligibleProducts";
    public const string ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY = "AllowRepurchaseFailedCoursesFromNexport";
    public const string ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY = "AllowRepurchasePassedCoursesFromNexport";
    public const string LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY = "LimitSingleProductPurchaseInCategory";
    public const string AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY = "AutoSwapProductPurchaseInCategory";
    public const string ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT = "AllowProductPurchaseInCategoryDuringEnrollment";
    public const string ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE = "Nexport.OrderManualApproval.StoreOwnerNotification";
    public const string REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE = "Nexport.Redemption.StudentNotification";

    public static readonly string RegistrationFieldsZone = "nexport_registration_fields";
    public static readonly string CustomRegistrationFieldZone = "admin_nexport_custom_registration_field";
    public static readonly string RegistrationFieldPrefix = "CustomProfile";

    public const string PURCHASE_PRODUCT_FOR_CUSTOMER = "Nexport.PurchaseForCustomer";
}
