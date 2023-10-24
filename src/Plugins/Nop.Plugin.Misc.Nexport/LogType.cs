using static Nop.Plugin.Misc.Nexport.SystemNames;

namespace Nop.Plugin.Misc.Nexport;

public static class LogType
{
    public const string EDIT_CUSTOMER_SUPPLEMENTAL_INFO_ANSWER = $"{NEXPORT}.EditCustomerSupplementalInfoAnswer";
    public const string DELETE_CUSTOMER_SUPPLEMENTAL_INFO_ANSWER = $"{NEXPORT}.DeleteCustomerSupplementalInfoAnswer";
    public const string PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS = $"{NEXPORT}.ProcessingSupplementalInfoGroupAssociations";
    public const string DELETE_PRODUCT_MAPPING = $"{NEXPORT}.Delete";
    public const string EDIT_PRODUCT_MAPPING = $"{NEXPORT}.EditProductMapping";
    public const string MODIFY_PRODUCT_MAPPING = $"{NEXPORT}.ModifyProductMapping";
    public const string DUPLICATE_PRODUCT_MAPPING = $"{NEXPORT}.DuplicateProductMapping";
    public const string DELETE_GROUP_MEMBERSHIP_MAPPING = $"{NEXPORT}.DeleteGroupMembershipMapping";
    public const string INSERT_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING = $"{NEXPORT}.InsertSupplementalInfoQuestion";
    public const string DELETE_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING = $"{NEXPORT}.DeleteSupplementalInfoQuestion";
}
