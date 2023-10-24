using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using static Nop.Plugin.Misc.Nexport.Defaults;
using static Nop.Plugin.Misc.Nexport.LogType;

namespace Nop.Plugin.Misc.Nexport.Services;

public class PluginService
{
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IPermissionService _permission;
    private readonly IScheduleTaskService _scheduleTask;
    private readonly IScheduleTaskRunner _taskRunner;
    private readonly ISettingService _setting;
    private readonly ILocalizationService _localization;
    private readonly ICustomerActivityService _customerActivity;
    private readonly IMessageTemplateService _messageTemplate;
    private readonly IRepository<ActivityLogType> _activityLogTypes;
    private readonly ILogger _logger;

#pragma warning disable RCS0056
    private static readonly Dictionary<string, string> s_localeResources = new()
        {
            { "Plugins.Misc.Nexport.Url", "Server URL" },
            { "Plugins.Misc.Nexport.Url.Hint", "The NexPort server URL" },
            { "Plugins.Misc.Nexport.Username", "Username" },
            { "Plugins.Misc.Nexport.Username.Hint", "NexPort login username" },
            { "Plugins.Misc.Nexport.Password", "Password" },
            { "Plugins.Misc.Nexport.Password.Hint", "NexPort login credential" },
            { "Plugins.Misc.Nexport.AutoRenewToken", "Automatically renew token" },
            { "Plugins.Misc.Nexport.AutoRenewToken.Hint", """Renew access token automatically before expiring date. Does not apply if "Token never expired" option is selected.""" },
            { "Plugins.Misc.Nexport.TokenNeverExpired", "Token never expired" },
            { "Plugins.Misc.Nexport.TokenNeverExpired.Hint", "The API access token will never expired" },
            { "Plugins.Misc.Nexport.CustomTokenExpirationDate", "Token expiration date on" },
            { "Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint", """Date that the API access token will be expired. If not specify, the token will be default to be expired after 30 days unless "Automatically renew token" or "Token never expired" option is selected.""" },
            { "Plugins.Misc.Nexport.Token", "Access token" },
            { "Plugins.Misc.Nexport.UtcExpirationDate", "Token expiration date" },
            { "Plugins.Misc.Nexport.UtcExpirationDate.Hint", "The API access token expiration date" },
            { "Plugins.Misc.Nexport.FindRootOrganization", "Find NexPort organization" },
            { "Plugins.Misc.Nexport.RootOrganizationId", "Root organization GroupGuid" },
            { "Plugins.Misc.Nexport.RootOrganizationId.Hint", "The organization that products will be synchronized with" },
            { "Plugins.Misc.Nexport.MerchantAccountId", "Merchant account GroupGuid" },
            { "Plugins.Misc.Nexport.MerchantAccountId.Hint", "The merchant account that will be used to process order redemptions" },
            { "Plugins.Misc.Nexport.SubscriptionOrgId", "NexPort subscription organization GroupGuid" },
            { "Plugins.Misc.Nexport.SubscriptionOrgId.Hint", "The NexPort subscription organization that will be used for the store" },
            { "Plugins.Misc.Nexport.SaleModel", "Sale model" },
            { "Plugins.Misc.Nexport.SaleModel.Hint", "The NexPort sale model for the store" },
            { "Plugins.Misc.Nexport.SaleModel.Retail", "Retail" },
            { "Plugins.Misc.Nexport.SaleModel.Wholesale", "Wholesale" },
            { "Plugins.Misc.Nexport.AllowRepurchaseFailedCourses", "Allow repurchasing failed courses" },
            { "Plugins.Misc.Nexport.AllowRepurchaseFailedCourses.Hint", "Allowing users to purchase products that associated with courses in NexPort that they have failed previously" },
            { "Plugins.Misc.Nexport.AllowRepurchasePassedCourses", "Allow repurchasing passed courses" },
            { "Plugins.Misc.Nexport.AllowRepurchasePassedCourses.Hint", "Allowing users to purchase products that associated with courses in NexPort that they have passed previously" },
            { "Plugins.Misc.Nexport.HideSectionCEUsInProductPage", "Hide NexPort section CEUs" },
            { "Plugins.Misc.Nexport.HideSectionCEUsInProductPage.Hint", "Hide NexPort section CEUs information in the product page" },
            { "Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts", "Hide Add to cart for ineligible product" },
            { "Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts.Hint", "Hide Add to cart button for any product that customers are not allowed to purchase" },
            { "Plugins.Misc.Nexport.ProductName", "Product name" },
            { "Plugins.Misc.Nexport.ProductName.Hint", "The name of the product in Nexport" },
            { "Plugins.Misc.Nexport.DisplayName", "Display name" },
            { "Plugins.Misc.Nexport.DisplayName.Hint", "The display name in Nop for the NexPort product" },
            { "Plugins.Misc.Nexport.CatalogSyllabusLinkId", "Catalog syllabus link GroupGuid" },
            { "Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint", "The GroupGuid of the catalog syllabus linking" },
            { "Plugins.Misc.Nexport.CatalogId", "Catalog GroupGuid" },
            { "Plugins.Misc.Nexport.CatalogId.Hint", "The GroupGuid of the NexPort catalog" },
            { "Plugins.Misc.Nexport.SyllabusId", "Syllabus GroupGuid" },
            { "Plugins.Misc.Nexport.SyllabusId.Hint", "The GroupGuid of the NexPort syllabus" },
            { "Plugins.Misc.Nexport.SubscriptionOrgId", "Subscription organization GroupGuid" },
            { "Plugins.Misc.Nexport.SubscriptionOrgId.Hint", "The GroupGuid of the subscription organization in Nexport" },
            { "Plugins.Misc.Nexport.SubscriptionOrgName", "Subscription organization name" },
            { "Plugins.Misc.Nexport.SubscriptionOrgName.Hint", "The name of the subscription organization in Nexport" },
            { "Plugins.Misc.Nexport.SubscriptionOrgShortName", "Subscription organization short name" },
            { "Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint", "The short name of the subscription organization in Nexport" },
            { "Plugins.Misc.Nexport.Type", "Type" },
            { "Plugins.Misc.Nexport.Type.Hint", "The type of the NexPort product" },
            { "Plugins.Misc.Nexport.PublishingModel", "Publishing model" },
            { "Plugins.Misc.Nexport.PublishingModel.Hint", "The publishing model in Nexport" },
            { "Plugins.Misc.Nexport.PricingModel", "Pricing model" },
            { "Plugins.Misc.Nexport.PricingModel.Hint", "The pricing model in Nexport" },
            { "Plugins.Misc.Nexport.ModifiedDate", "Modified date" },
            { "Plugins.Misc.Nexport.ModifiedDate.Hint", "The last modified date" },
            { "Plugins.Misc.Nexport.AvailableDate", "Available date" },
            { "Plugins.Misc.Nexport.AvailableDate.Hint", "The available date of the NexPort product" },
            { "Plugins.Misc.Nexport.EndDate", "End date" },
            { "Plugins.Misc.Nexport.EndDate.Hint", "The ending date of the NexPort product" },
            { "Plugins.Misc.Nexport.CreditHours", "Credit hours" },
            { "Plugins.Misc.Nexport.CreditHours.Hint", "The credit hours of the NexPort product" },
            { "Plugins.Misc.Nexport.UniqueName", "Unique name" },
            { "Plugins.Misc.Nexport.UniqueName.Hint", "The unique name of the NexPort product. This is only applicable for NexPort syllabus items." },
            { "Plugins.Misc.Nexport.SectionCeus", "Section CEUs" },
            { "Plugins.Misc.Nexport.SectionCeus.Hint", "The CEUs for the NexPort section" },
            { "Plugins.Misc.Nexport.SectionNumber", "Section number" },
            { "Plugins.Misc.Nexport.SectionNumber.Hint", "The NexPort section number" },
            { "Plugins.Misc.Nexport.IsSynchronized", "Synchronized?" },
            { "Plugins.Misc.Nexport.IsSynchronized.Hint", "Indication of synchronization from Nexport" },
            { "Plugins.Misc.Nexport.LastSynchronizationDate", "Last synchronization date" },
            { "Plugins.Misc.Nexport.LastSynchronizationDate.Hint", "The date that the last synchronization occurred" },
            { "Plugins.Misc.Nexport.AutoRedeem", "Auto redeeming purchase" },
            { "Plugins.Misc.Nexport.AutoRedeem.Hint", "Allows customers to redeem the product right after purchasing" },
            { "Plugins.Misc.Nexport.StoreMapping", "Store" },
            { "Plugins.Misc.Nexport.StoreMapping.Hint", "The current store that the product is mapped to. If it is empty, this mapping is the default mapping." },
            { "Plugins.Misc.Nexport.IsExtensionProduct", "Extension only product" },
            { "Plugins.Misc.Nexport.IsExtensionProduct.Hint", "Allows the product to become extension only product" },
            { "Plugins.Misc.Nexport.ProductAccessExpirationOption", "Access expiration option" },
            { "Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint", "Specify the access expiration in Nexport. If specified, access expiration date will take priority compared to access expiration time limit." },
            { "Plugins.Misc.Nexport.UtcAccessExpirationDate", "Access expiration date" },
            { "Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint", "The access expiration date (in Coordinated Universal Time - UTC) in Nexport. The product access expiration date will be based on this date instead." },
            { "Plugins.Misc.Nexport.AccessTimeLimit", "Access expiration time limit" },
            { "Plugins.Misc.Nexport.AccessTimeLimit.Hint", "The access time limit in Nexport. The product access expiration date will be based on the date of the product redemption plus the access time limit." },
            { "Plugins.Misc.Nexport.GroupId", "Group GroupGuid" },
            { "Plugins.Misc.Nexport.GroupId.Hint", "The GroupGuid of the group in Nexport" },
            { "Plugins.Misc.Nexport.GroupName", "Group name" },
            { "Plugins.Misc.Nexport.GroupName.Hint", "The name of the group in Nexport" },
            { "Plugins.Misc.Nexport.GroupShortName", "Group short name" },
            { "Plugins.Misc.Nexport.GroupShortName.Hint", "The short name of the group in Nexport" },
            { "Plugins.Misc.Nexport.AllowExtension", "Allow extension" },
            { "Plugins.Misc.Nexport.AllowExtension.Hint", "Allows customers to purchase the product as extension product" },
            { "Plugins.Misc.Nexport.RenewalWindow", "Renewal window" },
            { "Plugins.Misc.Nexport.RenewalWindow.Hint", "The time period that customers can purchase the product to extend the expiration on the NexPort product. Customers can only make purchase if the current date is within the period from the current NexPort product expiration date minus the time window." },
            { "Plugins.Misc.Nexport.RenewalDuration", "Renewal duration" },
            { "Plugins.Misc.Nexport.RenewalDuration.Hint", "The time period that the enrollment can be extended" },
            { "Plugins.Misc.Nexport.RenewalCompletionThreshold", "Enrollment completion threshold" },
            { "Plugins.Misc.Nexport.RenewalCompletionThreshold.Hint", "When this completion threshold is met, the enrollment will either be reset if the completion percentage is below the threshold or be extended automatically based on the Auto approval method. If approval method is Manual, the action will be deferred to the selection from administrators." },
            { "Plugins.Misc.Nexport.RenewalApprovalMethod", "Approval method" },
            { "Plugins.Misc.Nexport.RenewalApprovalMethod.Hint", "When approval method is set to manual, the administrator will be able to choose the choice between extending or restarting the enrollment when the completion percentage exceeds the completion threshold." },
            { "Plugins.Misc.Nexport.ExtensionPurchaseLimit", "Extension purchase limit" },
            { "Plugins.Misc.Nexport.ExtensionPurchaseLimit.Hint", "Limit how many times the customers can purchase the extension." },
            { "Plugins.Misc.Nexport.DuplicateSourceStoreMapping", "Mapping duplication source" },
            { "Plugins.Misc.Nexport.DuplicateSourceStoreMapping.Hint", "The store that contains the product mapping which will be duplicated from." },
            { "Plugins.Misc.Nexport.DuplicateDestinationStoreMappings", "Mapping duplication destinations" },
            { "Plugins.Misc.Nexport.DuplicateDestinationStoreMappings.Hint", "The list of stores that the product mapping will be duplicated to." },
            { "Plugins.Misc.Nexport.Training", "NexPort Training" },
            { "Account.Fields.Nexport.UserId", "NexPort user GroupGuid" },
            { "Account.Fields.Nexport.UserId.Hint", "The user GroupGuid in NexPort" },
            { "Account.Fields.Nexport.OwnerOrgId", "NexPort owner organization GroupGuid" },
            { "Account.Fields.Nexport.OwnerOrgId.Hint", "The GroupGuid of the owner organization for the user" },
            { "Account.Fields.Nexport.OwnerOrgShortName", "NexPort owner organization short name" },
            { "Account.Fields.Nexport.OwnerOrgShortName.Hint", "The short name of the owner organization for the user" },
            { "Account.Fields.Nexport.UserFullName", "NexPort user full name" },
            { "Account.Fields.Nexport.UserFullName.Hint", "The full name of the user in NexPort" },
            { "Account.Fields.Nexport.Email", "NexPort email" },
            { "Account.Fields.Nexport.Email.Hint", "The internal email of the user in NexPort" },
            { "Account.Login.Fields.EmailOrUsername", "Email/Username" },
            { "Plugins.Misc.Nexport.Navigation.UserInfo", "NexPort user info" },
            { "Plugins.Misc.Nexport.Navigation.MyTrainings", "My trainings" },
            { "Plugins.Misc.Nexport.Order.ViewRedemption", "Launch this training" },
            { "Plugins.Misc.Nexport.Order.Redeem", "Redeem" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.NONE", "Question text" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.NONE.Hint", "The question text" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.Description", "Description" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.Description.Hint", "The description of the question" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.Type", "Type" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.Type.Hint", "The type of the question" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive", "Active?" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive.Hint", "Allows the question to be active" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Option.NONE", "Option text" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Option.NONE.Hint", "The question option text" },
            { "Plugins.Misc.Nexport.SupplementalInfo.Customer.Edit", "Modify" },
            { "Plugins.Misc.Nexport.Navigation.SupplementalInfoAnswers", "Supplemental info answers" },
            { "Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.PageTitle", "Supplemental info answers" },
            { "Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.Edit.PageTitle", "Modify your answer(s)" },
            { "Admin.Configuration.Settings.CustomerUser.Nexport.RegistrationFields", "NexPort registration fields" },
            { "Admin.Customers.Nexport.RegistrationFields.Description", "You can create and manage the registration fields and its categories available during registration below." },
            { "Admin.Customers.Nexport.RegistrationField.Categories", "Registration field categories" },
            { "Admin.Customers.Nexport.RegistrationField.Categories.Description", "You can create and manage the categories for registration fields below." },
            { "Admin.Customers.Nexport.RegistrationField.Fields", "Registration fields" },
            { "Admin.Customers.Nexport.RegistrationField.Fields.Description", "You can create and manage the registration fields below." },
            { "Admin.Customers.Nexport.RegistrationField.Fields.AddNew", "Add a new registration field" },
            { "Admin.Customers.Nexport.RegistrationField.Fields.EditFieldDetails", "Edit registration field details" },
            { "Admin.Customers.Nexport.RegistrationField.Fields.BackToList", "back to registration field list" },
            { "Admin.Customers.Nexport.RegistrationField.Fields.Added", "The new registration field has been added successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Fields.Updated", "The registration field has been updated successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Fields.Deleted", "The registration field has been deleted successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit", "The selected custom render cannot be used for this registration field under the selected store(s)." },
            { "Admin.Customers.Nexport.RegistrationField.Categories.AddNew", "Add a new registration field category" },
            { "Admin.Customers.Nexport.RegistrationField.Categories.EditCategoryDetails", "Edit registration field category details" },
            { "Admin.Customers.Nexport.RegistrationField.Categories.BackToList", "back to registration field category list" },
            { "Admin.Customers.Nexport.RegistrationField.Categories.Added", "The new registration field category has been added successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Categories.Updated", "The registration field category has been updated successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Categories.Deleted", "The registration field category has been deleted successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Options.SaveBeforeEdit", """You need to save the registration field before you can add its options. Only "Select Dropdown" and "Select Checkbox" field have additional field options.""" },
            { "Admin.Customers.Nexport.RegistrationField.Options.AddNew", "Add a new option value" },
            { "Admin.Customers.Nexport.RegistrationField.Options.EditOptionValueDetails", "Edit option value" },
            { "Admin.Customers.Nexport.RegistrationField.Options.Added", "The new registration field option value has been added successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Options.Updated", "The registration field option value has been updated successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Options.Deleted", "The registration field option value has been deleted successfully." },
            { "Admin.Customers.Nexport.RegistrationField.Customs.SaveBeforeEdit", "You need to save the registration field before you can select its custom render." },
            { "Plugins.Misc.Nexport.RegistrationField.Category.Title", "Title" },
            { "Plugins.Misc.Nexport.RegistrationField.Category.Title.Hint", "The name of the registration field category" },
            { "Plugins.Misc.Nexport.RegistrationField.Category.Title.Required", "Registration field category is required to have a title." },
            { "Plugins.Misc.Nexport.RegistrationField.Category.Description", "Description" },
            { "Plugins.Misc.Nexport.RegistrationField.Category.Description.Hint", "Description of the registration field category" },
            { "Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder", "Display order" },
            { "Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder.Hint", "The registration field category display order. 1 represents the first item in the list." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Name", "Name" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Name.Hint", "The name of the registration field." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Type", "Type" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Type.Hint", "Choose how to display your registration field." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey", "NexPort custom profile field key" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey.Hint", "The custom profile field key in NexPort that will be used to synchronize the field." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Required", "Required" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Required.Hint", "When the registration field is required, the customer must choose appropriate value before they can continues." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Active", "Active" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Active.Hint", "If the registration field is not active, it will not be displayed." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Category", "Field category" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Category.Hint", "The category that the registration field will be displayed within." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Validation", "Validate before submitting" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Validation.Hint", "Use custom validation to validate the registration field before submitting it." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex", "Custom validation" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex.Hint", "Regular expression that will be used to validate the field before submitting." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage", "Validation message" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage.Hint", "The message that will be displayed when the custom validation has failed." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings", "Stores" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Stores", "Stores" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Stores.Hint", "Option to limit display the registration field to a certain store" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder", "Display order" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder.Hint", "The registration field display order. 1 represents the first item in the list." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.CustomRender", "Custom field render" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.CustomRender.Hint", "Choose which custom display will be used to render this registration field." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection", "Allow multiple option selections" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection.Hint", "Allow custom to select multiple options instead of single selection." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder", "Display options in ascending order" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder.Hint", "Display the options in ascending order based on their values." },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Info", "Registration field info" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.OptionValues", "Registration field option values" },
            { "Plugins.Misc.Nexport.RegistrationField.Field.Custom", "Registration field custom option" },
            { "Plugins.Misc.Nexport.RegistrationField.Option.Value", "Option value" },
            { "Plugins.Misc.Nexport.RegistrationField.Option.Value.Hint", "The value of the option" },
            { "Plugins.Misc.Nexport.RegistrationField.Option.Value.Required", "The value for option is required" },
            { "Plugins.Misc.Nexport.Category.Options", "Additional options" },
            { "Plugins.Misc.Nexport.Category.LimitSingleProductPurchase", "Limit single product per checkout" },
            { "Plugins.Misc.Nexport.Category.LimitSingleProductPurchase.Hint", "Allowing customers to purchase single product within the category per checkout." },
            { "Plugins.Misc.Nexport.Category.AutoSwapProductPurchase", "Auto swapping between individual product" },
            { "Plugins.Misc.Nexport.Category.AutoSwapProductPurchase.Hint", "When customers select a different product within the category, the current product in the shopping cart will be replaced automatically with that new product item." },
            { "Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment", "Allow customers with existing enrollments to purchase additional products within this category" },
            { "Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment.Hint", "Allowing customers that currently have enrollments in NexPort to purchase additional products besides the products that associated with the enrollments in this category." },
            { "Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport", "Error occurred during the transferring to NexPort. You might not have an active subscription in NexPort Campus. Please contact customer nexportService for further assistance." },
            { "Plugins.Misc.Nexport.Errors.FailedToRedeemForUser", "Failed to redeem NexPort invoice item {0} for user {1}" },
            { "Plugins.Misc.Nexport.Errors.RedemptionProcessFailure", "Error occurred during the redemption process for the order item invoice {0}" },
            { "Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved", "This item cannot be purchased at this time and will be removed at checkout!" },
            { "Plugins.Misc.Nexport.Errors.DuplicatedProduct", "Cannot add duplicated product to the cart due to store restriction." },
            { "Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed", "Cannot purchase more than one for this product due to store restriction." },
            { "Plugins.Misc.Nexport.Errors.SingleProductInCatalog", "You can only purchase single product within the same catalog." },
            { "Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart", "Cannot purchase more than one for some products in this shopping cart due to store restriction." },
            { "Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase", "Cannot purchase this product." },
            { "Plugins.Misc.Nexport.ProductMapping.SearchProductName", "Product Name" },
            { "Plugins.Misc.Nexport.ProductMapping.SearchProductName.Hint", "Filter product mapping list by NexPort product name." },
            { "Plugins.Misc.Nexport.ProductMapping.SearchProductType", "Type" },
            { "Plugins.Misc.Nexport.ProductMapping.SearchProductType.Hint", "Filter product mapping list by NexPort product type." },
            { "Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId", "Store" },
            { "Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId.Hint", "Filter registration fields list by store." },
            { "Plugins.Misc.Nexport.ProductMapping.SearchStoreName", "Store" },
            { "Plugins.Misc.Nexport.ProductMapping.SearchStoreName.Hint", "Filter product mapping list by store." },
            { "Plugins.Misc.Nexport.Stores.SearchStoreName", "Name" },
            { "Plugins.Misc.Nexport.Stores.SearchStoreName.Hint", "Filter stores list by name." },
            { "Plugins.Misc.Nexport.Stores.SearchStoreUrl", "URL" },
            { "Plugins.Misc.Nexport.Stores.SearchStoreUrl.Hint", "Filter stores list by URL." },
            { "Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName", "Name" },
            { "Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName.Hint", "Filter registration fields list by name." },
            { "Plugins.Misc.Nexport.Wholesale.Go", "Go to created order" },
            { "Plugins.Misc.Nexport.Wholesale.RedeemBy", "Redeem-By Date" },
            { "Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited", "Unlimited redemption" },
            { "Plugins.Misc.Nexport.Organization", "Organization" },
            { "Plugins.Misc.Nexport.Order.GroupId", "Purchasing For Group/Org:" },
            { "Plugins.Misc.Nexport.Navigation.Groups", "My NexPort groups" },
            { "Plugins.Misc.Nexport.Groups", "NexPort Groups" },
            { "Plugins.Misc.Nexport.Group.Products", "Products" },
            { "Plugins.Misc.Nexport.Group.Product.Redemptions", "Redemptions" },
            { "Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem", "Redeem Product" },
            { "Plugins.Misc.Nexport.Group.Product.Redemptions.Modify", "Modify Product Redemption" },
            { "Plugins.Misc.Nexport.Wholesale.Purchasing.Agent", "Purchasing Agent" },
            { "Plugins.Misc.Nexport.Wholesale.Purchasing.Group", "Group Membership" },
            { "Plugins.Misc.Nexport.Wholesale.ApplyGroupMembershipWhenRedeemed", "Apply Group Membership When Redeemed" },
            { "Plugins.Misc.Nexport.Wholesale.Purchasing.Group", "Purchasing Group" },
            { "Plugins.Misc.Nexport.Wholesale.Create", "Create Wholesale Order" },
            { "Plugins.Misc.Nexport.Admin.Redemption.OrderInvoiceItemId", "Invoice Item" },
            { "Plugins.Misc.Nexport.Admin.Redemption.NopCustomerId", "Student" },
        };

    public PluginService(
        EmailAccountSettings emailAccountSettings,
        IScheduleTaskService scheduleTaskService,
        IScheduleTaskRunner taskRunner,
        ISettingService settingService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        ICustomerActivityService customerActivityService,
        IMessageTemplateService messageTemplateService,
        IRepository<ActivityLogType> activityLogTypeRepository,
        ILogger logger)
    {
        _emailAccountSettings = emailAccountSettings;
        _scheduleTask = scheduleTaskService;
        _taskRunner = taskRunner;
        _setting = settingService;
        _permission = permissionService;
        _localization = localizationService;
        _customerActivity = customerActivityService;
        _messageTemplate = messageTemplateService;
        _activityLogTypes = activityLogTypeRepository;
        _logger = logger;
    }

    public async Task InstallScheduledTaskAsync()
    {
        try
        {
            if (await _setting.GetSettingAsync(OrderProcessingTaskBatchSizeSettingKey) is null)
            {
                await _setting.SetSettingAsync(
                    OrderProcessingTaskBatchSizeSettingKey,
                    OrderProcessingTaskBatchSize);
            }
            if (await _scheduleTask.GetTaskByTypeAsync(OrderProcessingTaskType) is null)
            {
                var orderProcessingTask = new ScheduleTask
                {
                    Enabled = true,
                    Seconds = OrderProcessingTaskInterval,
                    Name = OrderProcessingTaskName,
                    Type = OrderProcessingTaskType,
                };
                await _scheduleTask.InsertTaskAsync(orderProcessingTask);
                await _taskRunner.ExecuteAsync(orderProcessingTask, true);
            }
            if (_setting.GetSettingAsync(SynchronizationTaskBatchSizeSettingKey) is null)
            {
                await _setting.SetSettingAsync(
                    SynchronizationTaskBatchSizeSettingKey,
                    SynchronizationTaskBatchSize);
            }
            if (_scheduleTask.GetTaskByTypeAsync(SynchronizationTaskType) is null)
            {
                var synchronizationTask = new ScheduleTask
                {
                    Enabled = true,
                    Seconds = SynchronizationTaskInterval,
                    Name = SynchronizationTaskName,
                    Type = SynchronizationTaskType,
                };
                await _scheduleTask.InsertTaskAsync(synchronizationTask);
                await _taskRunner.ExecuteAsync(synchronizationTask, true);
            }
            if (await _setting.GetSettingAsync(OrderInvoiceRedemptionTaskBatchSizeSettingKey) is null)
            {
                await _setting.SetSettingAsync(
                    OrderInvoiceRedemptionTaskBatchSizeSettingKey,
                    OrderInvoiceRedemptionTaskBatchSize);
            }
            if (await _scheduleTask.GetTaskByTypeAsync(OrderInvoiceRedemptionTaskType) is null)
            {
                var invoiceRedemptionTask = new ScheduleTask
                {
                    Enabled = true,
                    Seconds = OrderInvoiceRedemptionTaskInterval,
                    Name = OrderInvoiceRedemptionTaskName,
                    Type = OrderInvoiceRedemptionTaskType,
                };
                await _scheduleTask.InsertTaskAsync(invoiceRedemptionTask);
                await _taskRunner.ExecuteAsync(invoiceRedemptionTask, true);
            }
            if (await _setting.GetSettingAsync(SupplementalInfoAnswerProcessingTaskBatchSizeSettingKey) is null)
            {
                await _setting.SetSettingAsync(
                    SupplementalInfoAnswerProcessingTaskBatchSizeSettingKey,
                    SupplementalInfoAnswerProcessingTaskBatchSize);
            }
            if (await _scheduleTask.GetTaskByTypeAsync(SupplementalInfoAnswerProcessingTaskType) is null)
            {
                var supplementalInfoAnswerProcessingTask = new ScheduleTask
                {
                    Enabled = true,
                    Seconds = SupplementalInfoAnswerProcessingTaskInterval,
                    Name = SupplementalInfoAnswerProcessingTaskName,
                    Type = SupplementalInfoAnswerProcessingTaskType,
                };
                await _scheduleTask.InsertTaskAsync(supplementalInfoAnswerProcessingTask);
                await _taskRunner.ExecuteAsync(supplementalInfoAnswerProcessingTask, true);
            }
            if (_scheduleTask.GetTaskByTypeAsync(GroupMembershipRemovalTaskType) is null)
            {
                var groupMembershipRemovalTask = new ScheduleTask
                {
                    Enabled = true,
                    Seconds = GroupMembershipRemovalTaskInterval,
                    Name = GroupMembershipRemovalTaskName,
                    Type = GroupMembershipRemovalTaskType,
                };
                await _scheduleTask.InsertTaskAsync(groupMembershipRemovalTask);
                await _taskRunner.ExecuteAsync(groupMembershipRemovalTask, true);
            }
            if (await _scheduleTask.GetTaskByTypeAsync(RegistrationFieldSynchronizationTaskType) is null)
            {
                var registrationFieldSynchronizationTask = new ScheduleTask
                {
                    Enabled = true,
                    Seconds = RegistrationFieldSynchronizationTaskInterval,
                    Name = RegistrationFieldSynchronizationTaskName,
                    Type = RegistrationFieldSynchronizationTaskType,
                };
                await _scheduleTask.InsertTaskAsync(registrationFieldSynchronizationTask);
                await _taskRunner.ExecuteAsync(registrationFieldSynchronizationTask, true);
            }
        }
        catch (Exception e)
        {
            await _logger.ErrorAsync("Cannot install and run new task(s)", e);
        }
    }

    public async Task UninstallScheduledTaskAsync()
    {
        var tasks = await _scheduleTask.GetAllTasksAsync();
        foreach (var task in tasks)
        {
            if (task.Type.Equals(OrderProcessingTaskType, StringComparison.OrdinalIgnoreCase)
                || task.Type.Equals(SynchronizationTaskType, StringComparison.OrdinalIgnoreCase)
                || task.Type.Equals(OrderInvoiceRedemptionTaskType, StringComparison.OrdinalIgnoreCase)
                || task.Type.Equals(SupplementalInfoAnswerProcessingTaskType, StringComparison.OrdinalIgnoreCase)
                || task.Type.Equals(GroupMembershipRemovalTaskType, StringComparison.OrdinalIgnoreCase)
                || task.Type.Equals(RegistrationFieldSynchronizationTaskType, StringComparison.OrdinalIgnoreCase))
            {
                await _scheduleTask.DeleteTaskAsync(task);
            }
        }
    }

    public async Task AddActivityLogTypesAsync()
    {
        var customerActivityLogTypes = (await _customerActivity.GetAllActivityTypesAsync())
            .Where(activityLogType => activityLogType.SystemKeyword.Contains(nameof(Nexport), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Processing NexPort supplemental info group associations",
                SystemKeyword = PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(EDIT_CUSTOMER_SUPPLEMENTAL_INFO_ANSWER, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Edit customer NexPort supplemental info answer",
                SystemKeyword = EDIT_CUSTOMER_SUPPLEMENTAL_INFO_ANSWER,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(DELETE_CUSTOMER_SUPPLEMENTAL_INFO_ANSWER, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Delete customer NexPort supplemental info answer",
                SystemKeyword = DELETE_CUSTOMER_SUPPLEMENTAL_INFO_ANSWER,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(DELETE_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Delete NexPort product mapping",
                SystemKeyword = DELETE_PRODUCT_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(EDIT_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Edit NexPort product mapping",
                SystemKeyword = EDIT_PRODUCT_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(MODIFY_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Modify NexPort product mapping",
                SystemKeyword = MODIFY_PRODUCT_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(DUPLICATE_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Duplicate NexPort product mapping",
                SystemKeyword = DUPLICATE_PRODUCT_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(DELETE_GROUP_MEMBERSHIP_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Duplicate NexPort product mapping group membership",
                SystemKeyword = DELETE_GROUP_MEMBERSHIP_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(INSERT_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Insert NexPort product mapping supplemental info question ",
                SystemKeyword = INSERT_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(DELETE_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Delete NexPort product mapping supplemental info question",
                SystemKeyword = DELETE_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
        if (!customerActivityLogTypes.Any(activityLogType =>
            activityLogType.SystemKeyword.Equals(DELETE_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING, StringComparison.OrdinalIgnoreCase)))
        {
            var entity = new ActivityLogType
            {
                Name = "Purchase product for customer",
                SystemKeyword = PURCHASE_PRODUCT_FOR_CUSTOMER,
                Enabled = true,
            };
            await _activityLogTypes.InsertAsync(entity);
        }
    }

    public async Task DeleteActivityLogTypesAsync()
    {
        var customerActivityLogTypes = (await _customerActivity.GetAllActivityTypesAsync())
            .Where(activityLogType => activityLogType.SystemKeyword.Contains(nameof(Nexport), StringComparison.OrdinalIgnoreCase));
        foreach (var type in customerActivityLogTypes)
        {
            await _activityLogTypes.DeleteAsync(type);
        }
    }

    public async Task AddMessageTemplatesAsync()
    {
        var messageTemplates = await _messageTemplate.GetAllMessageTemplatesAsync(0);
        const string ownerTemplate = ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE;
        if (!messageTemplates.Any(messageTemplate => messageTemplate.Name.Equals(ownerTemplate, StringComparison.OrdinalIgnoreCase)))
        {
            var messageTemplate = new MessageTemplate
            {
                Name = ownerTemplate,
                Subject = "New order approval request",
                Body = $"<p>{Environment.NewLine}"
                    + $"Order #%OrderApproval.Id% requires an approval before the enrollment(s) can be redeemed for the students.{Environment.NewLine}"
                    + $"<br />{Environment.NewLine}"
                    + $"Please click <a href=\"%OrderApproval.AdminViewOrderUrl%\">here</a> to view the order and take action.</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = _emailAccountSettings.DefaultEmailAccountId,
            };
            await _messageTemplate.InsertMessageTemplateAsync(messageTemplate);
        }
        const string studentTemplate = REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE;
        if (!messageTemplates.Any(messageTemplate => messageTemplate.Name.Equals(studentTemplate, StringComparison.OrdinalIgnoreCase)))
        {
            var messageTemplate = new MessageTemplate
            {
                Name = studentTemplate,
                Subject = "Redemption assigned",
                Body = $"<p>{Environment.NewLine}"
                    + $"An item has been redeemed to your account.<br />{Environment.NewLine}"
                    + $"<br />{Environment.NewLine}"
                    + $"Please click <a href=\"%Redemption.AcceptRedemptionUrl%\">here</a> to accept it.</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = _emailAccountSettings.DefaultEmailAccountId,
            };
            await _messageTemplate.InsertMessageTemplateAsync(messageTemplate);
        }
    }

    public async Task DeleteMessageTemplatesAsync()
    {
        foreach (var messageTemplate in (await _messageTemplate.GetAllMessageTemplatesAsync(0))
            .Where(messageTemplate => messageTemplate.Name.StartsWith(nameof(Nexport), StringComparison.OrdinalIgnoreCase)))
        {
            await _messageTemplate.DeleteMessageTemplateAsync(messageTemplate);
        }
    }

    public Task AddOrUpdateResourcesAsync() => _localization.AddOrUpdateLocaleResourceAsync(s_localeResources);
    public Task DeleteResourcesAsync() => _localization.DeleteLocaleResourcesAsync(s_localeResources.Keys.ToList());
    public Task InstallPermissionProviderAsync() => _permission.InstallPermissionsAsync(new PermissionProvider());
    public Task UninstallPermissionProviderAsync() => _permission.UninstallPermissionsAsync(new PermissionProvider());
}
