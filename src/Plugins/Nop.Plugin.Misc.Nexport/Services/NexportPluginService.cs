using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportPluginService
    {
        private readonly EmailAccountSettings _emailAccountSettings;

        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ILogger _logger;

        public NexportPluginService(
            EmailAccountSettings emailAccountSettings,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IMessageTemplateService messageTemplateService,
            ILogger logger)
        {
            _emailAccountSettings = emailAccountSettings;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _messageTemplateService = messageTemplateService;
            _logger = logger;
        }

        public void InstallScheduledTask()
        {
            try
            {
                if (_settingService.GetSetting(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey) == null)
                {
                    _settingService.SetSetting(NexportDefaults.NexportOrderProcessingTaskBatchSizeSettingKey,
                        NexportDefaults.NexportOrderProcessingTaskBatchSize);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportOrderProcessingTaskType) == null)
                {
                    var orderProcessingTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportOrderProcessingTaskInterval,
                        Name = NexportDefaults.NexportOrderProcessingTaskName,
                        Type = NexportDefaults.NexportOrderProcessingTaskType
                    };

                    _scheduleTaskService.InsertTask(orderProcessingTask);

                    var task = new Task(orderProcessingTask);
                    task.Execute(true, false);
                }

                if (_settingService.GetSetting(NexportDefaults.NexportSynchronizationTaskBatchSizeSettingKey) == null)
                {
                    _settingService.SetSetting(NexportDefaults.NexportSynchronizationTaskBatchSizeSettingKey,
                        NexportDefaults.NexportSynchronizationTaskBatchSize);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportSynchronizationTaskType) == null)
                {
                    var synchronizationTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportSynchronizationTaskInterval,
                        Name = NexportDefaults.NexportSynchronizationTaskName,
                        Type = NexportDefaults.NexportSynchronizationTaskType
                    };

                    _scheduleTaskService.InsertTask(synchronizationTask);

                    var task = new Task(synchronizationTask);
                    task.Execute(true, false);
                }

                if (_settingService.GetSetting(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey) == null)
                {
                    _settingService.SetSetting(NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSizeSettingKey,
                        NexportDefaults.NexportOrderInvoiceRedemptionTaskBatchSize);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportOrderInvoiceRedemptionTaskType) == null)
                {
                    var invoiceRedemptionTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportOrderInvoiceRedemptionTaskInterval,
                        Name = NexportDefaults.NexportOrderInvoiceRedemptionTaskName,
                        Type = NexportDefaults.NexportOrderInvoiceRedemptionTaskType
                    };

                    _scheduleTaskService.InsertTask(invoiceRedemptionTask);

                    var task = new Task(invoiceRedemptionTask);
                    task.Execute(true, false);
                }

                if (_settingService.GetSetting(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSizeSettingKey) == null)
                {
                    _settingService.SetSetting(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSizeSettingKey,
                        NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSize);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskType) == null)
                {
                    var supplementalInfoAnswerProcessingTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskInterval,
                        Name = NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskName,
                        Type = NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskType
                    };

                    _scheduleTaskService.InsertTask(supplementalInfoAnswerProcessingTask);

                    var task = new Task(supplementalInfoAnswerProcessingTask);
                    task.Execute(true, false);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportGroupMembershipRemovalTaskType) == null)
                {
                    var groupMembershipRemovalTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportGroupMembershipRemovalTaskInterval,
                        Name = NexportDefaults.NexportGroupMembershipRemovalTaskName,
                        Type = NexportDefaults.NexportGroupMembershipRemovalTaskType
                    };

                    _scheduleTaskService.InsertTask(groupMembershipRemovalTask);

                    var task = new Task(groupMembershipRemovalTask);
                    task.Execute(true, false);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportRegistrationFieldSynchronizationTaskType) == null)
                {
                    var registrationFieldSynchronizationTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportRegistrationFieldSynchronizationTaskInterval,
                        Name = NexportDefaults.NexportRegistrationFieldSynchronizationTaskName,
                        Type = NexportDefaults.NexportRegistrationFieldSynchronizationTaskType
                    };

                    _scheduleTaskService.InsertTask(registrationFieldSynchronizationTask);

                    var task = new Task(registrationFieldSynchronizationTask);
                    task.Execute(true, false);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Cannot install and run new task(s)", e);
            }
        }

        public void UninstallScheduledTask()
        {
            var tasks = _scheduleTaskService.GetAllTasks();

            foreach (var task in tasks)
            {
                if (task.Type.Equals(NexportDefaults.NexportOrderProcessingTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportSynchronizationTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportOrderInvoiceRedemptionTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportGroupMembershipRemovalTaskType) ||
                    task.Type.Equals(NexportDefaults.NexportRegistrationFieldSynchronizationTaskType))
                {
                    _scheduleTaskService.DeleteTask(task);
                }
            }
        }

        public void AddActivityLogTypes()
        {
            var customerActivityLogTypes = _customerActivityService.GetAllActivityTypes()
                .Where(x => x.SystemKeyword.Contains("Nexport")).ToArray();

            if (!customerActivityLogTypes.Any(x =>
                x.SystemKeyword.Equals(NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE)))
            {
                _customerActivityService.InsertActivityType(new ActivityLogType
                {
                    Name = "Processing Nexport supplemental info group associations",
                    SystemKeyword = NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                x.SystemKeyword.Equals(NexportDefaults.EDIT_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE)))
            {
                _customerActivityService.InsertActivityType(new ActivityLogType
                {
                    Name = "Edit customer Nexport supplemental info answer",
                    SystemKeyword = NexportDefaults.EDIT_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                x.SystemKeyword.Equals(NexportDefaults.DELETE_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE)))
            {
                _customerActivityService.InsertActivityType(new ActivityLogType
                {
                    Name = "Delete customer Nexport supplemental info answer",
                    SystemKeyword = NexportDefaults.DELETE_CUSTOMER_NEXPORT_SUPPLEMENTAL_INFO_ANSWER_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }
        }

        public void DeleteActivityLogTypes()
        {
            var customerActivityLogTypes = _customerActivityService.GetAllActivityTypes()
                .Where(x => x.SystemKeyword.Contains("Nexport"));
            foreach (var type in customerActivityLogTypes)
            {
                _customerActivityService.DeleteActivityType(type);
            }
        }

        public void AddMessageTemplates()
        {
            var messageTemplates = _messageTemplateService.GetAllMessageTemplates(0);
            if (!messageTemplates.Any(x =>
                x.Name.Equals(NexportDefaults.NEXPORT_ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate
                {
                    Name = NexportDefaults.NEXPORT_ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "New order approval request",
                    Body = $"<p>{Environment.NewLine}Order #%NexportOrderApproval.OrderId% requires an approval before the enrollment(s) can be redeemed for the students.{Environment.NewLine}<br />{Environment.NewLine}Please click <a href=\"%NexportOrderApproval.AdminViewOrderUrl%\">here</a> to view the order and take action.",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }
        }

        public void DeleteMessageTemplates()
        {
            var messageTemplates = _messageTemplateService.GetAllMessageTemplates(0)
                .Where(x => x.Name.Contains("Nexport"));
            foreach (var messageTemplate in messageTemplates)
            {
                _messageTemplateService.DeleteMessageTemplate(messageTemplate);
            }
        }

        public void AddOrUpdateResources()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Url", "Server url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Url.Hint", "The Nexport server url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Username", "Username");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Username.Hint", "Nexport login username");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Password", "Password");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Password.Hint", "Nexport login credential");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRenewToken", "Automatically renew token");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRenewToken.Hint", "Renew access token automatically before expiring date. Does not apply if \"Token never expired\" option is selected.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.TokenNeverExpired", "Token never expired");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.TokenNeverExpired.Hint", "The API access token will never expired");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CustomTokenExpirationDate", "Token expiration date on");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint", "Date that the API access token will be expired. " +
                 "If not specify, the token will be default to be expired after 30 days unless \"Automatically renew token\" or \"Token never expired\" option is selected.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Token", "Access token");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate", "Token expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate.Hint", "The API access token expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.FindRootOrganization", "Find Nexport organization");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId", "Root organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId.Hint", "The organization that products will be synchronized with");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId", "Merchant account Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId.Hint", "The merchant account that will be used to process order redemptions");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId", "Nexport subscription organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint", "The Nexport subscription organization that will be used for the store");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SaleModel", "Sale model");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SaleModel.Hint", "The Nexport sale model for the store");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SaleModel.Retail", "Retail");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SaleModel.Wholesale", "Wholesale");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses", "Allow repurchasing failed courses");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses.Hint", "Allowing users to purchase products that associated with courses in Nexport that they have failed previously");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchasePassedCourses", "Allow repurchasing passed courses");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchasePassedCourses.Hint", "Allowing users to purchase products that associated with courses in Nexport that they have passed previously");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.HideSectionCEUsInProductPage", "Hide Nexport section CEUs");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.HideSectionCEUsInProductPage.Hint", "Hide Nexport section CEUs information in the product page");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts","Hide Add to cart for ineligible product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts.Hint", "Hide Add to cart button for any product that customers are not allowed to purchase");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName", "Product name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName.Hint", "The name of the product in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.DisplayName", "Display name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.DisplayName.Hint", "The display name in Nop for the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId", "Catalog syllabus link Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint", "The Id of the catalog syllabus linking");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogId", "Catalog Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogId.Hint", "The Id of the Nexport catalog");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId","Syllabus Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId.Hint","The Id of the Nexport syllabus");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId", "Subscription organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId.Hint", "The Id of the subscription organization in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName", "Subscription organization name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName.Hint", "The name of the subscription organization in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName", "Subscription organization short name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint", "The short name of the subscription organization in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Type", "Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Type.Hint", "The type of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel", "Publishing model");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel.Hint", "The publishing model in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PricingModel", "Pricing model");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PricingModel.Hint", "The pricing model in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate", "Modified date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate.Hint", "The last modified date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate", "Available date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate.Hint", "The available date of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.EndDate", "End date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.EndDate.Hint", "The ending date of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CreditHours", "Credit hours");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CreditHours.Hint", "The credit hours of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UniqueName", "Unique name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UniqueName.Hint",
                "The unique name of the Nexport product. This is only applicable for Nexport syllabus items.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus", "Section CEUs");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus.Hint", "The CEUs for the Nexport section");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SectionNumber", "Section number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SectionNumber.Hint", "The Nexport section number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized", "Synchronized?");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized.Hint", "Indication of synchronization from Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate", "Last synchronization date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate.Hint", "The date that the last synchronization occurred");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem", "Auto redeeming purchase");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem.Hint", "Allows customers to redeem the product right after purchasing");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.StoreMapping", "Store");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.StoreMapping.Hint",
                "The current store that the product is mapped to. If it is empty, this mapping is the default mapping.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.IsExtensionProduct", "Extension only product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.IsExtensionProduct.Hint", "Allows the product to become extension only product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption", "Access expiration option");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint",
                "Specify the access expiration in Nexport. If specified, access expiration date will take priority compared to access expiration time limit.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate", "Access expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint",
                "The access expiration date (in Coordinated Universal Time - UTC) in Nexport. The product access expiration date will be based on this date instead.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit", "Access expiration time limit");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit.Hint",
                "The access time limit in Nexport. The product access expiration date will be based on the date of the product redemption plus the access time limit.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupId", "Group Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupId.Hint", "The Id of the group in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupName", "Group name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupName.Hint", "The name of the group in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName", "Group short name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName.Hint", "The short name of the group in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AllowExtension", "Allow extension");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AllowExtension.Hint", "Allows customers to purchase the product as extension product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalWindow", "Renewal window");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalWindow.Hint",
                "The time period that customers can purchase the product to extend the expiration on the Nexport product. Customers can only make purchase if the current date is within the period from the current Nexport product expiration date minus the time window.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalDuration", "Renewal duration");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalDuration.Hint", "The time period that the enrollment can be extended");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalCompletionThreshold", "Enrollment completion threshold");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalCompletionThreshold.Hint",
                "When this completion threshold is met, the enrollment will either be reset if the completion percentage is below the threshold or be extended automatically based on the Auto approval method. If approval method is Manual, the action will be deferred to the selection from administrators.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalApprovalMethod", "Approval method");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RenewalApprovalMethod.Hint",
                "When approval method is set to manual, the administrator will be able to choose the choice between extending or restarting the enrollment when the completion percentage exceeds the completion threshold.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ExtensionPurchaseLimit", "Extension purchase limit");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ExtensionPurchaseLimit.Hint", "Limit how many times the customers can purchase the extension.");

            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.UserId", "Nexport user Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.UserId.Hint", "The user Id in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId", "Nexport owner organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId.Hint", "The Id of the owner organization for the user");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName", "Nexport owner organization short name");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName.Hint", "The short name of the owner organization for the user");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.Email", "Nexport email");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.Email.Hint", "The internal email of the user in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Login.Fields.EmailOrUsername", "Email/Username");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.UserInfo", "Nexport user info");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyTrainings", "My trainings");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Order.ViewRedemption", "Launch this training");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Order.Redeem", "Redeem");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Text", "Question text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Text.Hint", "The question text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Description", "Description");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Description.Hint", "The description of the question");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Type", "Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Type.Hint", "The type of the question");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive", "Active?");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive.Hint", "Allows the question to be active");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Option.Text", "Option text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Option.Text.Hint", "The question option text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Customer.Edit", "Modify");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.SupplementalInfoAnswers", "Supplemental info answers");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.PageTitle", "Supplemental info answers");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.Edit.PageTitle", "Modify your answer(s)");

            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.CustomerUser.Nexport.RegistrationFields", "Nexport registration fields");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationFields.Description", "You can create and manage the registration fields and its categories available during registration below.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories", "Registration field categories");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Description", "You can create and manage the categories for registration fields below.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields", "Registration fields");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Description", "You can create and manage the registration fields below.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.AddNew", "Add a new registration field");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.EditFieldDetails", "Edit registration field details");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.BackToList", "back to registration field list");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Added", "The new registration field has been added successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Updated", "The registration field has been updated successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Deleted", "The registration field has been deleted successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit", "The selected custom render cannot be used for this registration field under the selected store(s).");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.AddNew", "Add a new registration field category");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.EditCategoryDetails", "Edit registration field category details");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.BackToList", "back to registration field category list");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Added", "The new registration field category has been added successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Updated", "The registration field category has been updated successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Deleted", "The registration field category has been deleted successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.SaveBeforeEdit", "You need to save the registration field before you can add its options. Only \"Select Dropdown\" and \"Select Checkbox\" field have additional field options.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.AddNew", "Add a new option value");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.EditOptionValueDetails", "Edit option value");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.Added", "The new registration field option value has been added successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.Updated", "The registration field option value has been updated successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.Deleted", "The registration field option value has been deleted successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Customs.SaveBeforeEdit", "You need to save the registration field before you can select its custom render.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Title", "Title");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Title.Hint", "The name of the registration field category");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Description", "Description");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Description.Hint", "Description of the registration field category");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder", "Display order");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder.Hint", "The registration field category display order. 1 represents the first item in the list.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Name", "Name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Name.Hint", "The name of the registration field.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Type", "Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Type.Hint", "Choose how to display your registration field.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey", "Nexport custom profile field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey.Hint", "The custom profile field key in Nexport that will be used to synchronize the field.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Required", "Required");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Required.Hint", "When the registration field is required, the customer must choose appropriate value before they can continues.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Active", "Active");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Active.Hint", "If the registration field is not active, it will not be displayed.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Category", "Field category");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Category.Hint", "The category that the registration field will be displayed within.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Validation", "Validate before submitting");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Validation.Hint", "Use custom validation to validate the registration field before submitting it.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex", "Custom validation");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex.Hint", "Regular expression that will be used to validate the field before submitting.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings", "Stores");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Stores", "Stores");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Stores.Hint", "Option to limit display the registration field to a certain store");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder", "Display order");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder.Hint", "The registration field display order. 1 represents the first item in the list.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender", "Custom field render");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender.Hint", "Choose which custom display will be used to render this registration field.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection", "Allow multiple option selections");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection.Hint", "Allow custom to select multiple options instead of single selection.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder", "Display options in ascending order");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder.Hint", "Display the options in ascending order based on their values.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Info", "Registration field info");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.OptionValues", "Registration field option values");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Custom", "Registration field custom option");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Option.Value", "Option value");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Option.Value.Hint", "The value of the option");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Option.Value.Required", "The value for option is required");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.Options", "Additional options");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase", "Limit single product per checkout");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase.Hint", "Allowing customers to purchase single product within the category per checkout.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase", "Auto swapping between individual product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase.Hint", "When customers select a different product within the category, the current product in the shopping cart will be replaced automatically with that new product item.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment", "Allow customers with existing enrollments to purchase additional products within this category");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment.Hint",
                "Allowing customers that currently have enrollments in Nexport to purchase additional products besides the products that associated with the enrollments in this category.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport", "Error occurred during the transferring to Nexport. You might not have an active subscription in Nexport Campus. Please contact customer service for further assistance.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser", "Failed to redeem Nexport invoice item {0} for user {1}");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure", "Error occurred during the redemption process for the order item invoice {0}");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved", "This item cannot be purchased at this time and will be removed at checkout!");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.DuplicatedProduct", "Cannot add duplicated product to the cart due to store restriction.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed", "Cannot purchase more than one for this product due to store restriction.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.SingleProductInCatalog", "You can only purchase single product within the same catalog.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart", "Cannot purchase more than one for some products in this shopping cart due to store restriction.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase", "Cannot purchase this product.");
        }

        public void DeleteResources()
        {
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Url");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Url.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Username");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Username.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Password");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Password.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AutoRenewToken");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AutoRenewToken.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.TokenNeverExpired");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.TokenNeverExpired.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CustomTokenExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Token");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.FindRootOrganization");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SaleModel");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SaleModel.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SaleModel.Retail");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SaleModel.Wholesale");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchasePassedCourses");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AllowRepurchasePassedCourses.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.HideSectionCEUsInProductPage");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.HideSectionCEUsInProductPage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.DisplayName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.DisplayName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Type");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Type.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PricingModel");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PricingModel.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.EndDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.EndDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CreditHours");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CreditHours.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UniqueName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UniqueName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SectionNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SectionNumber.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.StoreMapping");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.StoreMapping.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.IsExtensionProduct");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.IsExtensionProduct.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AllowExtension");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AllowExtension.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalWindow");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalWindow.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalDuration");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalDuration.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalCompletionThreshold");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalCompletionThreshold.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalApprovalMethod");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RenewalApprovalMethod.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ExtensionPurchaseLimit");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ExtensionPurchaseLimit.Hint");

            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.UserId");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.UserId.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.Email");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.Email.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Login.Fields.EmailOrUsername");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.UserInfo");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyTrainings");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Order.ViewRedemption");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Order.Redeem");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Text");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Text.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Description");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Description.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Type");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.Type.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Option.Text");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Option.Text.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SupplementalInfo.Customer.Edit");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.SupplementalInfoAnswers");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.PageTitle");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.Edit.PageTitle");

            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.CustomerUser.Nexport.RegistrationFields");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationFields.Description");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Description");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Description");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.AddNew");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.EditFieldDetails");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.BackToList");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Added");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Updated");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.Deleted");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.AddNew");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.EditCategoryDetails");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.BackToList");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Added");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Updated");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Categories.Deleted");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.SaveBeforeEdit");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.AddNew");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.EditOptionValueDetails");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.Added");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.Updated");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Options.Deleted");
            _localizationService.DeletePluginLocaleResource("Admin.Customers.Nexport.RegistrationField.Customs.SaveBeforeEdit");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Title");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Title.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Description");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.Description.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Name");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Name.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Type");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Type.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Required");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Required.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Active");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Active.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Category");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Category.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Validation");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Validation.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Stores");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Stores.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Info");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.OptionValues");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Field.Custom");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Option.Value");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RegistrationField.Option.Value.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.Options");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.DuplicatedProduct");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.SingleProductInCatalog");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase");
        }

        public void InstallPermissionProvider()
        {
            var permissionProviders = new List<Type> { typeof(NexportPermissionProvider) };
            foreach (var providerType in permissionProviders)
            {
                var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                _permissionService.InstallPermissions(provider);
            }
        }

        public void UninstallPermissionProvider()
        {
            var permissionProviders = new List<Type> { typeof(NexportPermissionProvider) };
            foreach (var providerType in permissionProviders)
            {
                var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                _permissionService.UninstallPermissions(provider);
            }
        }
    }
}
