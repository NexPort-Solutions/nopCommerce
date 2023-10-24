using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data;

public class PluginDataMappingNameCompatibility : INameCompatibility
{
    public const string GROUP_MEMBERSHIP_REMOVAL_QUEUE = "NexportGroupMembershipRemovalQueue";
    public const string ORDER_INVOICE_ITEM = "NexportOrderInvoiceItem";
    public const string ORDER_INVOICE_REDEMPTION_QUEUE = "NexportOrderInvoiceRedemptionQueue";
    public const string ORDER_PROCESSING_QUEUE = "NexportOrderProcessingQueue";
    public const string PRODUCT_GROUP_MEMBERSHIP_MAPPING = "NexportProductGroupMembershipMapping";
    public const string PRODUCT_MAPPING = "NexportProductMapping";
    public const string REGISTRATION_FIELD = "NexportRegistrationField";
    public const string REGISTRATION_FIELD_ANSWER = "NexportRegistrationFieldAnswer";
    public const string REGISTRATION_FIELD_CATEGORY = "NexportRegistrationFieldCategory";
    public const string REGISTRATION_FIELD_OPTION = "NexportRegistrationFieldOption";
    public const string REGISTRATION_FIELD_STORE_MAPPING = "NexportRegistrationFieldStoreMapping";
    public const string REGISTRATION_FIELD_SYNCHRONIZATION_QUEUE = "NexportRegistrationFieldSynchronizationQueue";
    public const string REQUIRED_SUPPLEMENTAL_INFO = "NexportRequiredSupplementalInfo";
    public const string SUPPLEMENTAL_INFO_ANSWER = "NexportSupplementalInfoAnswer";
    public const string SUPPLEMENTAL_INFO_ANSWER_MEMBERSHIP = "NexportSupplementalInfoAnswerMembership";
    public const string SUPPLEMENTAL_INFO_ANSWER_PROCESSING_QUEUE = "NexportSupplementalInfoAnswerProcessingQueue";
    public const string SUPPLEMENTAL_INFO_OPTION = "NexportSupplementalInfoOption";
    public const string SUPPLEMENTAL_INFO_OPTION_GROUP_ASSOCIATION = "NexportSupplementalInfoOptionGroupAssociation";
    public const string SUPPLEMENTAL_INFO_QUESTION = "NexportSupplementalInfoQuestion";
    public const string SUPPLEMENTAL_INFO_QUESTION_MAPPING = "NexportSupplementalInfoQuestionMapping";
    public const string USER_MAPPING = "NexportUserMapping";

    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(GroupMembershipRemovalQueueItem), GROUP_MEMBERSHIP_REMOVAL_QUEUE },
        { typeof(OrderInvoiceItem), ORDER_INVOICE_ITEM },
        { typeof(OrderInvoiceRedemptionQueueItem), ORDER_INVOICE_REDEMPTION_QUEUE },
        { typeof(OrderProcessingQueueItem), ORDER_PROCESSING_QUEUE },
        { typeof(ProductGroupMembershipMapping), PRODUCT_GROUP_MEMBERSHIP_MAPPING },
        { typeof(ProductMapping), PRODUCT_MAPPING },
        { typeof(Domain.RegistrationField.RegistrationField), REGISTRATION_FIELD },
        { typeof(Domain.RegistrationField.Answer), REGISTRATION_FIELD_ANSWER },
        { typeof(Domain.RegistrationField.Category), REGISTRATION_FIELD_CATEGORY },
        { typeof(Domain.RegistrationField.Option), REGISTRATION_FIELD_OPTION },
        { typeof(Domain.RegistrationField.StoreMapping), REGISTRATION_FIELD_STORE_MAPPING },
        { typeof(Domain.RegistrationField.SynchronizationQueueItem), REGISTRATION_FIELD_SYNCHRONIZATION_QUEUE },
        { typeof(Domain.SupplementalInfo.RequiredSupplementalInfo), REQUIRED_SUPPLEMENTAL_INFO },
        { typeof(Domain.SupplementalInfo.Answer), SUPPLEMENTAL_INFO_ANSWER },
        { typeof(Domain.SupplementalInfo.AnswerMembership), SUPPLEMENTAL_INFO_ANSWER_MEMBERSHIP },
        { typeof(Domain.SupplementalInfo.AnswerProcessingQueueItem), SUPPLEMENTAL_INFO_ANSWER_PROCESSING_QUEUE },
        { typeof(Domain.SupplementalInfo.Option), SUPPLEMENTAL_INFO_OPTION },
        { typeof(Domain.SupplementalInfo.OptionGroupAssociation), SUPPLEMENTAL_INFO_OPTION_GROUP_ASSOCIATION },
        { typeof(Domain.SupplementalInfo.Question), SUPPLEMENTAL_INFO_QUESTION },
        { typeof(Domain.SupplementalInfo.QuestionMapping), SUPPLEMENTAL_INFO_QUESTION_MAPPING },
        { typeof(UserMapping), USER_MAPPING },
    };

    public Dictionary<(Type, string), string> ColumnName => new();
}
