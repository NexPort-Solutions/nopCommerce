using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Drawing.Charts;
using LinqToDB;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Caching;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Org.BouncyCastle.Asn1;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportPluginService
    {
        private readonly EmailAccountSettings _emailAccountSettings;

        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IScheduleTaskRunner _taskRunner;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<Setting> _settingRepository;
        private readonly IPluginLocalizationService _pluginLocalizationService;
        private readonly ILogger _logger;

        private static readonly Dictionary<string, string> _localeResources = new()
        {
            {"Plugins.Misc.Nexport.Url", "Server url"},
            {"Plugins.Misc.Nexport.Url.Hint", "The Nexport server url"},
            {"Plugins.Misc.Nexport.Username", "Username"},
            {"Plugins.Misc.Nexport.Username.Hint", "Nexport login username"},
            {"Plugins.Misc.Nexport.Password", "Password"},
            {"Plugins.Misc.Nexport.Password.Hint", "Nexport login credential"},
            {"Plugins.Misc.Nexport.AutoRenewToken", "Automatically renew token"},
            {"Plugins.Misc.Nexport.AutoRenewToken.Hint", "Renew access token automatically before expiring date. Does not apply if \"Token never expired\" option is selected."},
            {"Plugins.Misc.Nexport.TokenNeverExpired", "Token never expired"},
            {"Plugins.Misc.Nexport.TokenNeverExpired.Hint", "The API access token will never expired"},
            {"Plugins.Misc.Nexport.CustomTokenExpirationDate", "Token expiration date on"},
            {"Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint", "Date that the API access token will be expired. " +
                 "If not specify, the token will be default to be expired after 30 days unless \"Automatically renew token\" or \"Token never expired\" option is selected."},
            {"Plugins.Misc.Nexport.Token", "Access token"},
            {"Plugins.Misc.Nexport.UtcExpirationDate", "Token expiration date"},
            {"Plugins.Misc.Nexport.UtcExpirationDate.Hint", "The API access token expiration date"},
            {"Plugins.Misc.Nexport.FindRootOrganization", "Find Nexport organization"},
            {"Plugins.Misc.Nexport.RootOrganizationId", "Root organization Id"},
            {"Plugins.Misc.Nexport.RootOrganizationId.Hint", "The organization that products will be synchronized with"},
            {"Plugins.Misc.Nexport.MerchantAccountId", "Merchant account Id"},
            {"Plugins.Misc.Nexport.MerchantAccountId.Hint", "The merchant account that will be used to process order redemptions"},
            {"Plugins.Misc.Nexport.NexportSubscriptionOrgId", "Nexport subscription organization Id"},
            {"Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint", "The Nexport subscription organization that will be used for the store"},
            {"Plugins.Misc.Nexport.SaleModel", "Sale model"},
            {"Plugins.Misc.Nexport.SaleModel.Hint", "The Nexport sale model for the store"},
            {"Plugins.Misc.Nexport.SaleModel.Retail", "Retail"},
            {"Plugins.Misc.Nexport.SaleModel.Wholesale", "Wholesale"},
            {"Plugins.Misc.Nexport.AllowRepurchaseFailedCourses", "Allow repurchasing failed courses"},
            {"Plugins.Misc.Nexport.AllowRepurchaseFailedCourses.Hint", "Allowing users to purchase products that associated with courses in Nexport that they have failed previously"},
            {"Plugins.Misc.Nexport.AllowRepurchasePassedCourses", "Allow repurchasing passed courses"},
            {"Plugins.Misc.Nexport.AllowRepurchasePassedCourses.Hint", "Allowing users to purchase products that associated with courses in Nexport that they have passed previously"},
            {"Plugins.Misc.Nexport.HideSectionCEUsInProductPage", "Hide Nexport section CEUs"},
            {"Plugins.Misc.Nexport.HideSectionCEUsInProductPage.Hint", "Hide Nexport section CEUs information in the product page"},
            {"Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts", "Hide Add to cart for ineligible product"},
            {"Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts.Hint", "Hide Add to cart button for any product that customers are not allowed to purchase"},

            {"Plugins.Misc.Nexport.NexportProductName", "Product name"},
            {"Plugins.Misc.Nexport.NexportProductName.Hint", "The name of the product in Nexport"},
            {"Plugins.Misc.Nexport.DisplayName", "Display name"},
            {"Plugins.Misc.Nexport.DisplayName.Hint", "The display name in Nop for the Nexport product"},
            {"Plugins.Misc.Nexport.CatalogSyllabusLinkId", "Catalog syllabus link Id"},
            {"Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint", "The Id of the catalog syllabus linking"},
            {"Plugins.Misc.Nexport.CatalogId", "Catalog Id"},
            {"Plugins.Misc.Nexport.CatalogId.Hint", "The Id of the Nexport catalog"},
            {"Plugins.Misc.Nexport.SyllabusId", "Syllabus Id"},
            {"Plugins.Misc.Nexport.SyllabusId.Hint", "The Id of the Nexport syllabus"},
            {"Plugins.Misc.Nexport.SubscriptionOrgId", "Subscription organization Id"},
            {"Plugins.Misc.Nexport.SubscriptionOrgId.Hint", "The Id of the subscription organization in Nexport"},
            {"Plugins.Misc.Nexport.SubscriptionOrgName", "Subscription organization name"},
            {"Plugins.Misc.Nexport.SubscriptionOrgName.Hint", "The name of the subscription organization in Nexport"},
            {"Plugins.Misc.Nexport.SubscriptionOrgShortName", "Subscription organization short name"},
            {"Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint", "The short name of the subscription organization in Nexport"},
            {"Plugins.Misc.Nexport.Type", "Type"},
            {"Plugins.Misc.Nexport.Type.Hint", "The type of the Nexport product"},
            {"Plugins.Misc.Nexport.PublishingModel", "Publishing model"},
            {"Plugins.Misc.Nexport.PublishingModel.Hint", "The publishing model in Nexport"},
            {"Plugins.Misc.Nexport.PricingModel", "Pricing model"},
            {"Plugins.Misc.Nexport.PricingModel.Hint", "The pricing model in Nexport"},
            {"Plugins.Misc.Nexport.ModifiedDate", "Modified date"},
            {"Plugins.Misc.Nexport.ModifiedDate.Hint", "The last modified date"},
            {"Plugins.Misc.Nexport.AvailableDate", "Available date"},
            {"Plugins.Misc.Nexport.AvailableDate.Hint", "The available date of the Nexport product"},
            {"Plugins.Misc.Nexport.EndDate", "End date"},
            {"Plugins.Misc.Nexport.EndDate.Hint", "The ending date of the Nexport product"},
            {"Plugins.Misc.Nexport.CreditHours", "Credit hours"},
            {"Plugins.Misc.Nexport.CreditHours.Hint", "The credit hours of the Nexport product"},
            {"Plugins.Misc.Nexport.UniqueName", "Unique name"},
            {"Plugins.Misc.Nexport.UniqueName.Hint",
                "The unique name of the Nexport product. This is only applicable for Nexport syllabus items."},
            {"Plugins.Misc.Nexport.SectionCeus", "Section CEUs"},
            {"Plugins.Misc.Nexport.SectionCeus.Hint", "The CEUs for the Nexport section"},
            {"Plugins.Misc.Nexport.SectionNumber", "Section number"},
            {"Plugins.Misc.Nexport.SectionNumber.Hint", "The Nexport section number"},
            {"Plugins.Misc.Nexport.IsSynchronized", "Synchronized?"},
            {"Plugins.Misc.Nexport.IsSynchronized.Hint", "Indication of synchronization from Nexport"},
            {"Plugins.Misc.Nexport.LastSynchronizationDate", "Last synchronization date"},
            {"Plugins.Misc.Nexport.LastSynchronizationDate.Hint", "The date that the last synchronization occurred"},
            {"Plugins.Misc.Nexport.AutoRedeem", "Auto redeeming purchase"},
            {"Plugins.Misc.Nexport.AutoRedeem.Hint", "Allows customers to redeem the product right after purchasing"},
            {"Plugins.Misc.Nexport.StoreMapping", "Store"},
            {"Plugins.Misc.Nexport.StoreMapping.Hint",
                "The current store that the product is mapped to. If it is empty, this mapping is the default mapping."},
            {"Plugins.Misc.Nexport.IsExtensionProduct", "Extension only product"},
            {"Plugins.Misc.Nexport.IsExtensionProduct.Hint", "Allows the product to become extension only product"},
            {"Plugins.Misc.Nexport.ProductAccessExpirationOption", "Access expiration option"},
            {"Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint",
                "Specify the access expiration in Nexport. If specified, access expiration date will take priority compared to access expiration time limit."},
            {"Plugins.Misc.Nexport.UtcAccessExpirationDate", "Access expiration date"},
            {"Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint",
                "The access expiration date (in Coordinated Universal Time - UTC) in Nexport. The product access expiration date will be based on this date instead."},
            {"Plugins.Misc.Nexport.AccessTimeLimit", "Access expiration time limit"},
            {"Plugins.Misc.Nexport.AccessTimeLimit.Hint",
                "The access time limit in Nexport. The product access expiration date will be based on the date of the product redemption plus the access time limit."},
            {"Plugins.Misc.Nexport.GroupId", "Group Id"},
            {"Plugins.Misc.Nexport.GroupId.Hint", "The Id of the group in Nexport"},
            {"Plugins.Misc.Nexport.GroupName", "Group name"},
            {"Plugins.Misc.Nexport.GroupName.Hint", "The name of the group in Nexport"},
            {"Plugins.Misc.Nexport.GroupShortName", "Group short name"},
            {"Plugins.Misc.Nexport.GroupShortName.Hint", "The short name of the group in Nexport"},
            {"Plugins.Misc.Nexport.AllowExtension", "Allow extension"},
            {"Plugins.Misc.Nexport.AllowExtension.Hint", "Allows customers to purchase the product as extension product"},
            {"Plugins.Misc.Nexport.RenewalWindow", "Renewal window"},
            {"Plugins.Misc.Nexport.RenewalWindow.Hint",
                "The time period that customers can purchase the product to extend the expiration on the Nexport product. Customers can only make purchase if the current date is within the period from the current Nexport product expiration date minus the time window."},
            {"Plugins.Misc.Nexport.RenewalDuration", "Renewal duration"},
            {"Plugins.Misc.Nexport.RenewalDuration.Hint", "The time period that the enrollment can be extended"},
            {"Plugins.Misc.Nexport.RenewalCompletionThreshold", "Enrollment completion threshold"},
            {"Plugins.Misc.Nexport.RenewalCompletionThreshold.Hint",
                "When this completion threshold is met, the enrollment will either be reset if the completion percentage is below the threshold or be extended automatically based on the Auto approval method. If approval method is Manual, the action will be deferred to the selection from administrators."},
            {"Plugins.Misc.Nexport.RenewalApprovalMethod", "Approval method"},
            {"Plugins.Misc.Nexport.RenewalApprovalMethod.Hint",
                "When approval method is set to manual, the administrator will be able to choose the choice between extending or restarting the enrollment when the completion percentage exceeds the completion threshold."},
            {"Plugins.Misc.Nexport.ExtensionPurchaseLimit", "Extension purchase limit"},
            {"Plugins.Misc.Nexport.ExtensionPurchaseLimit.Hint", "Limit how many times the customers can purchase the extension."},
            {"Plugins.Misc.Nexport.DuplicateSourceStoreMapping", "Mapping duplication source"},
            {"Plugins.Misc.Nexport.DuplicateSourceStoreMapping.Hint", "The store that contains the product mapping which will be duplicated from."},
            {"Plugins.Misc.Nexport.DuplicateDestinationStoreMappings", "Mapping duplication destinations"},
            {"Plugins.Misc.Nexport.DuplicateDestinationStoreMappings.Hint", "The list of stores that the product mapping will be duplicated to."},

            {"Plugins.Misc.Nexport.Training", "Nexport Training"},

            {"Account.Fields.Nexport.UserId", "Nexport user Id"},
            {"Account.Fields.Nexport.UserId.Hint", "The user Id in Nexport"},
            {"Account.Fields.Nexport.OwnerOrgId", "Nexport owner organization Id"},
            {"Account.Fields.Nexport.OwnerOrgId.Hint", "The Id of the owner organization for the user"},
            {"Account.Fields.Nexport.OwnerOrgShortName", "Nexport owner organization short name"},
            {"Account.Fields.Nexport.OwnerOrgShortName.Hint", "The short name of the owner organization for the user"},
            {"Account.Fields.Nexport.UserFullName", "Nexport user full name"},
            {"Account.Fields.Nexport.UserFullName.Hint", "The full name of the user in Nexport"},
            {"Account.Fields.Nexport.Email", "Nexport email"},
            {"Account.Fields.Nexport.Email.Hint", "The internal email of the user in Nexport"},
            {"Account.Login.Fields.EmailOrUsername", "Email/Username"},

            {"Plugins.Misc.Nexport.Navigation.UserInfo", "Nexport user info"},
            {"Plugins.Misc.Nexport.Navigation.MyTrainings", "My trainings"},

            {"Plugins.Misc.Nexport.Order.ViewRedemption", "Launch this training"},
            {"Plugins.Misc.Nexport.Order.Redeem", "Redeem"},

            {"Plugins.Misc.Nexport.SupplementalInfo.Question.Text", "Question text"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.Text.Hint", "The question text"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.Description", "Description"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.Description.Hint", "The description of the question"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.Type", "Type"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.Type.Hint", "The type of the question"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive", "Active?"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive.Hint", "Allows the question to be active"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Option.Text", "Option text"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Option.Text.Hint", "The question option text"},
            {"Plugins.Misc.Nexport.SupplementalInfo.Customer.Edit", "Modify"},

            {"Plugins.Misc.Nexport.Navigation.SupplementalInfoAnswers", "Supplemental info answers"},
            {"Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.PageTitle", "Supplemental info answers"},
            {"Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.Edit.PageTitle", "Modify your answer(s)"},

            {"Admin.Configuration.Settings.CustomerUser.Nexport.RegistrationFields", "Nexport registration fields"},
            {"Admin.Customers.Nexport.RegistrationFields.Description", "You can create and manage the registration fields and its categories available during registration below."},
            {"Admin.Customers.Nexport.RegistrationField.Categories", "Registration field categories"},
            {"Admin.Customers.Nexport.RegistrationField.Categories.Description", "You can create and manage the categories for registration fields below."},
            {"Admin.Customers.Nexport.RegistrationField.Fields", "Registration fields"},
            {"Admin.Customers.Nexport.RegistrationField.Fields.Description", "You can create and manage the registration fields below."},
            {"Admin.Customers.Nexport.RegistrationField.Fields.AddNew", "Add a new registration field"},
            {"Admin.Customers.Nexport.RegistrationField.Fields.EditFieldDetails", "Edit registration field details"},
            {"Admin.Customers.Nexport.RegistrationField.Fields.BackToList", "back to registration field list"},
            {"Admin.Customers.Nexport.RegistrationField.Fields.Added", "The new registration field has been added successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Fields.Updated", "The registration field has been updated successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Fields.Deleted", "The registration field has been deleted successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit", "The selected custom render cannot be used for this registration field under the selected store(s)."},
            {"Admin.Customers.Nexport.RegistrationField.Categories.AddNew", "Add a new registration field category"},
            {"Admin.Customers.Nexport.RegistrationField.Categories.EditCategoryDetails", "Edit registration field category details"},
            {"Admin.Customers.Nexport.RegistrationField.Categories.BackToList", "back to registration field category list"},
            {"Admin.Customers.Nexport.RegistrationField.Categories.Added", "The new registration field category has been added successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Categories.Updated", "The registration field category has been updated successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Categories.Deleted", "The registration field category has been deleted successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Options.SaveBeforeEdit", "You need to save the registration field before you can add its options. Only \"Select Dropdown\" and \"Select Checkbox\" field have additional field options."},
            {"Admin.Customers.Nexport.RegistrationField.Options.AddNew", "Add a new option value"},
            {"Admin.Customers.Nexport.RegistrationField.Options.EditOptionValueDetails", "Edit option value"},
            {"Admin.Customers.Nexport.RegistrationField.Options.Added", "The new registration field option value has been added successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Options.Updated", "The registration field option value has been updated successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Options.Deleted", "The registration field option value has been deleted successfully."},
            {"Admin.Customers.Nexport.RegistrationField.Customs.SaveBeforeEdit", "You need to save the registration field before you can select its custom render."},
            {"Plugins.Misc.Nexport.RegistrationField.Category.Title", "Title"},
            {"Plugins.Misc.Nexport.RegistrationField.Category.Title.Hint", "The name of the registration field category"},
            {"Plugins.Misc.Nexport.RegistrationField.Category.Description", "Description"},
            {"Plugins.Misc.Nexport.RegistrationField.Category.Description.Hint", "Description of the registration field category"},
            {"Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder", "Display order"},
            {"Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder.Hint", "The registration field category display order. 1 represents the first item in the list."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Name", "Name"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Name.Hint", "The name of the registration field."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Type", "Type"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Type.Hint", "Choose how to display your registration field."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey", "Nexport custom profile field key"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey.Hint", "The custom profile field key in Nexport that will be used to synchronize the field."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Required", "Required"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Required.Hint", "When the registration field is required, the customer must choose appropriate value before they can continues."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Active", "Active"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Active.Hint", "If the registration field is not active, it will not be displayed."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Category", "Field category"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Category.Hint", "The category that the registration field will be displayed within."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Validation", "Validate before submitting"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Validation.Hint", "Use custom validation to validate the registration field before submitting it."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex", "Custom validation"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex.Hint", "Regular expression that will be used to validate the field before submitting."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage", "Validation message"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage.Hint", "The message that will be displayed when the custom validation has failed."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings", "Stores"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Stores", "Stores"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Stores.Hint", "Option to limit display the registration field to a certain store"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder", "Display order"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder.Hint", "The registration field display order. 1 represents the first item in the list."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.CustomRender", "Custom field render"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.CustomRender.Hint", "Choose which custom display will be used to render this registration field."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection", "Allow multiple option selections"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection.Hint", "Allow custom to select multiple options instead of single selection."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder", "Display options in ascending order"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder.Hint", "Display the options in ascending order based on their values."},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Info", "Registration field info"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.OptionValues", "Registration field option values"},
            {"Plugins.Misc.Nexport.RegistrationField.Field.Custom", "Registration field custom option"},
            {"Plugins.Misc.Nexport.RegistrationField.Option.Value", "Option value"},
            {"Plugins.Misc.Nexport.RegistrationField.Option.Value.Hint", "The value of the option"},
            {"Plugins.Misc.Nexport.RegistrationField.Option.Value.Required", "The value for option is required"},

            {"Plugins.Misc.Nexport.Category.Options", "Additional options"},
            {"Plugins.Misc.Nexport.Category.LimitSingleProductPurchase", "Limit single product per checkout"},
            {"Plugins.Misc.Nexport.Category.LimitSingleProductPurchase.Hint", "Allowing customers to purchase single product within the category per checkout."},
            {"Plugins.Misc.Nexport.Category.AutoSwapProductPurchase", "Auto swapping between individual product"},
            {"Plugins.Misc.Nexport.Category.AutoSwapProductPurchase.Hint", "When customers select a different product within the category, the current product in the shopping cart will be replaced automatically with that new product item."},
            {"Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment", "Allow customers with existing enrollments to purchase additional products within this category"},
            {"Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment.Hint",
                "Allowing customers that currently have enrollments in Nexport to purchase additional products besides the products that associated with the enrollments in this category."},

            {"Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport", "Error occurred during the transferring to Nexport. You might not have an active subscription in Nexport Campus. Please contact customer service for further assistance."},
            {"Plugins.Misc.Nexport.Errors.FailedToRedeemForUser", "Failed to redeem Nexport invoice item {0} for user {1}"},
            {"Plugins.Misc.Nexport.Errors.RedemptionProcessFailure", "Error occurred during the redemption process for the order item invoice {0}"},
            {"Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved", "This item cannot be purchased at this time and will be removed at checkout!"},
            {"Plugins.Misc.Nexport.Errors.DuplicatedProduct", "Cannot add duplicated product to the cart due to store restriction."},
            {"Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed", "Cannot purchase more than one for this product due to store restriction."},
            {"Plugins.Misc.Nexport.Errors.SingleProductInCatalog", "You can only purchase single product within the same catalog."},
            {"Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart", "Cannot purchase more than one for some products in this shopping cart due to store restriction."},
            {"Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase", "Cannot purchase this product."},
            {"Plugins.Misc.Nexport.Errors.ModifiedLocaleResources",
                "There are modified resource values within the <b>Nexport plugin</b> that conflicts with the default value. Review them <a href=\"javascript:OpenWindow(\'{0}\', 800, 500, true)\">here</a>."}
        };

        public static Dictionary<string, string> GetLocaleResource()
        {
            return _localeResources;
        }

        public NexportPluginService(
            EmailAccountSettings emailAccountSettings,
            IScheduleTaskService scheduleTaskService,
            IScheduleTaskRunner taskRunner,
            ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IMessageTemplateService messageTemplateService,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<Setting> settingRepository,
            IPluginLocalizationService pluginLocalizationService,
            ILogger logger)
        {
            _emailAccountSettings = emailAccountSettings;
            _scheduleTaskService = scheduleTaskService;
            _taskRunner = taskRunner;
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _messageTemplateService = messageTemplateService;
            _activityLogTypeRepository = activityLogTypeRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _settingRepository = settingRepository;
            _pluginLocalizationService = pluginLocalizationService;
            _logger = logger;
        }

        public async Task InstallScheduledTaskAsync()
        {
            try
            {
                if (await _settingService.GetSettingAsync(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey) == null)
                {
                    await _settingService.SetSettingAsync(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey,
                        NexportDefaults.NexportOrderProcessingTaskBatchSize);
                }

                if (await _scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportOrderProcessingTaskType) == null)
                {
                    var orderProcessingTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportOrderProcessingTaskInterval,
                        Name = NexportDefaults.NexportOrderProcessingTaskName,
                        Type = NexportDefaults.NexportOrderProcessingTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(orderProcessingTask);

                    await _taskRunner.ExecuteAsync(orderProcessingTask, true);
                }

                if (_settingService.GetSettingAsync(NexportDefaults.NexportSynchronizationTaskBatchSizeSettingKey) == null)
                {
                    await _settingService.SetSettingAsync(NexportDefaults.NexportSynchronizationTaskBatchSizeSettingKey,
                        NexportDefaults.NexportSynchronizationTaskBatchSize);
                }

                if (_scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportSynchronizationTaskType) == null)
                {
                    var synchronizationTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportSynchronizationTaskInterval,
                        Name = NexportDefaults.NexportSynchronizationTaskName,
                        Type = NexportDefaults.NexportSynchronizationTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(synchronizationTask);

                    await _taskRunner.ExecuteAsync(synchronizationTask, true);
                }

                if (await _settingService.GetSettingAsync(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey) == null)
                {
                    await _settingService.SetSettingAsync(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey,
                        NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSize);
                }

                if (await _scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportOrderInvoiceRedemptionTaskType) == null)
                {
                    var invoiceRedemptionTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportOrderInvoiceRedemptionTaskInterval,
                        Name = NexportDefaults.NexportOrderInvoiceRedemptionTaskName,
                        Type = NexportDefaults.NexportOrderInvoiceRedemptionTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(invoiceRedemptionTask);

                    await _taskRunner.ExecuteAsync(invoiceRedemptionTask, true);
                }

                if (await _settingService.GetSettingAsync(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSizeSettingKey) == null)
                {
                    await _settingService.SetSettingAsync(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSizeSettingKey,
                        NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSize);
                }

                if (await _scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskType) == null)
                {
                    var supplementalInfoAnswerProcessingTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskInterval,
                        Name = NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskName,
                        Type = NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(supplementalInfoAnswerProcessingTask);

                    await _taskRunner.ExecuteAsync(supplementalInfoAnswerProcessingTask, true);
                }

                if (_scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportGroupMembershipRemovalTaskType) == null)
                {
                    var groupMembershipRemovalTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportGroupMembershipRemovalTaskInterval,
                        Name = NexportDefaults.NexportGroupMembershipRemovalTaskName,
                        Type = NexportDefaults.NexportGroupMembershipRemovalTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(groupMembershipRemovalTask);

                    await _taskRunner.ExecuteAsync(groupMembershipRemovalTask, true);
                }

                if (await _scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportRegistrationFieldSynchronizationTaskType) == null)
                {
                    var registrationFieldSynchronizationTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportRegistrationFieldSynchronizationTaskInterval,
                        Name = NexportDefaults.NexportRegistrationFieldSynchronizationTaskName,
                        Type = NexportDefaults.NexportRegistrationFieldSynchronizationTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(registrationFieldSynchronizationTask);

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
            var tasks = await _scheduleTaskService.GetAllTasksAsync();

            foreach (var task in tasks)
            {
                if (task.Type.Equals(NexportDefaults.NexportOrderProcessingTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportSynchronizationTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportOrderInvoiceRedemptionTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportGroupMembershipRemovalTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportRegistrationFieldSynchronizationTaskType))
                {
                    await _scheduleTaskService.DeleteTaskAsync(task);
                }
            }
        }

        public async Task AddActivityLogTypesAsync()
        {
            var customerActivityLogTypes = (await _customerActivityService.GetAllActivityTypesAsync())
                .Where(x => x.SystemKeyword.Contains("Nexport")).ToArray();

            if (!customerActivityLogTypes.Any(x =>
                x.SystemKeyword.Equals(NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Processing Nexport supplemental info group associations",
                    SystemKeyword = NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                x.SystemKeyword.Equals(NexportDefaults.EDIT_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Edit customer Nexport supplemental info answer",
                    SystemKeyword = NexportDefaults.EDIT_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                x.SystemKeyword.Equals(NexportDefaults.DELETE_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Delete customer Nexport supplemental info answer",
                    SystemKeyword = NexportDefaults.DELETE_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }
        }

        public async Task DeleteActivityLogTypesAsync()
        {
            var customerActivityLogTypes = (await _customerActivityService.GetAllActivityTypesAsync())
                .Where(x => x.SystemKeyword.Contains("Nexport"));
            foreach (var type in customerActivityLogTypes)
            {
                await _activityLogTypeRepository.DeleteAsync(type);
            }
        }

        public async Task AddMessageTemplatesAsync()
        {
            var messageTemplates = await _messageTemplateService.GetAllMessageTemplatesAsync(0);
            if (!messageTemplates.Any(x =>
                x.Name.Equals(NexportDefaults.NEXPORT_ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = NexportDefaults.NEXPORT_ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "New order approval request",
                    Body = $"<p>{Environment.NewLine}Order #%NexportOrderApproval.OrderId% requires an approval before the enrollment(s) can be redeemed for the students.{Environment.NewLine}<br />{Environment.NewLine}Please click <a href=\"%NexportOrderApproval.AdminViewOrderUrl%\">here</a> to view the order and take action.",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }
        }

        public async Task DeleteMessageTemplatesAsync()
        {
            var messageTemplates = (await _messageTemplateService.GetAllMessageTemplatesAsync(0))
                .Where(x => x.Name.Contains("Nexport"));
            foreach (var messageTemplate in messageTemplates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);
            }
        }

        public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int languageId = 1)
        {
            // if this returns back false, then it didn't exist and we have added it to the table
            if (await _pluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue))
            {
                // this is if we have multiple modified values
                var nexportSetting =
                    await _settingService.GetSettingAsync("Plugin.Misc.Nexport.HasModifiedLocaleResources");

                if (nexportSetting == null)
                {
                    await _settingService.SetSettingAsync<bool>("Plugin.Misc.Nexport.HasModifiedLocaleResources", true);
                }
            }
        }

        public async Task AddOrUpdateResourcesAsync()
        {
            foreach (var localeResource in _localeResources)
            {
                await AddOrUpdateResourceAsync(localeResource.Key, localeResource.Value);
            }
        }

        public async Task DeleteResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync(_localeResources.Keys.ToList());
        }

        public async Task InstallPermissionProviderAsync()
        {
            var permissionProviders = new List<Type> { typeof(NexportPermissionProvider) };
            foreach (var providerType in permissionProviders)
            {
                var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                await _permissionService.InstallPermissionsAsync(provider);
            }
        }

        public async Task UninstallPermissionProviderAsync()
        {
            var permissionProviders = new List<Type> { typeof(NexportPermissionProvider) };
            foreach (var providerType in permissionProviders)
            {
                var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                await _permissionService.UninstallPermissionsAsync(provider);
            }
        }
    }
}
