using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;

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
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;

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
            ILogger logger,
            ICustomerService customerService)
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
            _logger = logger;
            _customerService = customerService;
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

                if (await _settingService.GetSettingAsync(NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSizeSettingKey) == null)
                {
                    await _settingService.SetSettingAsync(NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSizeSettingKey,
                        NexportDefaults.NexportOrderInvoiceResetRedemptionTaskBatchSize);
                }

                if (await _scheduleTaskService.GetTaskByTypeAsync(NexportDefaults.NexportOrderInvoiceResetRedemptionTaskType) == null)
                {
                    var invoiceResetRedemptionTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportOrderInvoiceResetRedemptionTaskInterval,
                        Name = NexportDefaults.NexportOrderInvoiceResetRedemptionTaskName,
                        Type = NexportDefaults.NexportOrderInvoiceResetRedemptionTaskType
                    };

                    await _scheduleTaskService.InsertTaskAsync(invoiceResetRedemptionTask);

                    await _taskRunner.ExecuteAsync(invoiceResetRedemptionTask, true);
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
                    task.Type.Equals(NexportDefaults.NexportOrderInvoiceResetRedemptionTaskType) ||
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

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.DELETE_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Delete Nexport product mapping",
                    SystemKeyword = NexportDefaults.DELETE_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.EDIT_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Edit Nexport product mapping",
                    SystemKeyword = NexportDefaults.EDIT_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.MODIFY_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Modify Nexport product mapping",
                    SystemKeyword = NexportDefaults.MODIFY_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.DUPLICATE_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Duplicate Nexport product mapping",
                    SystemKeyword = NexportDefaults.DUPLICATE_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.DELETE_NEXPORT_GROUP_MEMBERSHIP_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Duplicate Nexport product mapping group membership",
                    SystemKeyword = NexportDefaults.DELETE_NEXPORT_GROUP_MEMBERSHIP_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.INSERT_SUPPLEMENTAL_INFO_QUESTION_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Insert Nexport product mapping supplemental info question ",
                    SystemKeyword = NexportDefaults.INSERT_SUPPLEMENTAL_INFO_QUESTION_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.DELETE_SUPPLEMENTAL_INFO_QUESTION_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Delete Nexport product mapping supplemental info question",
                    SystemKeyword = NexportDefaults.DELETE_SUPPLEMENTAL_INFO_QUESTION_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!customerActivityLogTypes.Any(x =>
                    x.SystemKeyword.Equals(NexportDefaults.DELETE_SUPPLEMENTAL_INFO_QUESTION_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType
                {
                    Name = "Purchase product for customer",
                    SystemKeyword = NexportDefaults.NEXPORT_PURCHASE_PRODUCT_FOR_CUSTOMER,
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
            //TODO @js - add real template for customer manual redemption here
            //if (!messageTemplates.Any(x =>
            //        x.Name.Equals(NexportDefaults.NEXPORT_MANUAL_REDEMPTION_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE)))
            //{
            //    await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
            //    {
            //        Name = NexportDefaults.NEXPORT_MANUAL_REDEMPTION_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE,
            //        Subject = "New Redemption",
            //        Body = $"This is a placeholder template for manual redemption. Just here to be able to test that emailing is working. I will add the real template later",
            //        IsActive = true,
            //        EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
            //    });
            //}

            if (!messageTemplates.Any(x =>
                    x.Name.Equals(NexportDefaults.REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = NexportDefaults.REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "Redemption assigned",
                    Body = $"<p>{Environment.NewLine}"
                           + $"An item has been redeemed to your account.<br />{Environment.NewLine}"
                           + $"<br />{Environment.NewLine}"
                           + $"Please click <a href=\"%Redemption.AcceptRedemptionUrl%\">here</a> to accept it.</p>{Environment.NewLine}",
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

        public async Task AddOrUpdateResourcesAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Url", "Server url");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Url.Hint", "The Nexport server url");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Username", "Username");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Username.Hint", "Nexport login username");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Password", "Password");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Password.Hint", "Nexport login credential");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AutoRenewToken", "Automatically renew token");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AutoRenewToken.Hint", "Renew access token automatically before expiring date. Does not apply if \"Token never expired\" option is selected.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.TokenNeverExpired", "Token never expired");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.TokenNeverExpired.Hint", "The API access token will never expired");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CustomTokenExpirationDate", "Token expiration date on");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint", "Date that the API access token will be expired. " +
                 "If not specify, the token will be default to be expired after 30 days unless \"Automatically renew token\" or \"Token never expired\" option is selected.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Token", "Access token");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.UtcExpirationDate", "Token expiration date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.UtcExpirationDate.Hint", "The API access token expiration date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.FindRootOrganization", "Find Nexport organization");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RootOrganizationId", "Root organization Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RootOrganizationId.Hint", "The organization that products will be synchronized with");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.MerchantAccountId", "Merchant account Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.MerchantAccountId.Hint", "The merchant account that will be used to process order redemptions");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.NexportSubscriptionOrgId", "Nexport subscription organization Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint", "The Nexport subscription organization that will be used for the store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel", "Sale model");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel.Hint", "The Nexport sale model for the store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel.Retail", "Retail");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel.Wholesale", "Wholesale");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses", "Allow repurchasing failed courses");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses.Hint", "Allowing users to purchase products that associated with courses in Nexport that they have failed previously");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchasePassedCourses", "Allow repurchasing passed courses");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchasePassedCourses.Hint", "Allowing users to purchase products that associated with courses in Nexport that they have passed previously");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.HideSectionCEUsInProductPage", "Hide Nexport section CEUs");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.HideSectionCEUsInProductPage.Hint", "Hide Nexport section CEUs information in the product page");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts", "Hide Add to cart for ineligible product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts.Hint", "Hide Add to cart button for any product that customers are not allowed to purchase");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.NexportProductName", "Product name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.NexportProductName.Hint", "The name of the product in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DisplayName", "Display name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DisplayName.Hint", "The display name in Nop for the Nexport product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CatalogSyllabusLinkId", "Catalog syllabus link Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint", "The Id of the catalog syllabus linking");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CatalogId", "Catalog Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CatalogId.Hint", "The Id of the Nexport catalog");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SyllabusId", "Syllabus Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SyllabusId.Hint", "The Id of the Nexport syllabus");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgId", "Subscription organization Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgId.Hint", "The Id of the subscription organization in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgName", "Subscription organization name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgName.Hint", "The name of the subscription organization in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgShortName", "Subscription organization short name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint", "The short name of the subscription organization in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Type", "Type");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Type.Hint", "The type of the Nexport product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.PublishingModel", "Publishing model");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.PublishingModel.Hint", "The publishing model in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.PricingModel", "Pricing model");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.PricingModel.Hint", "The pricing model in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ModifiedDate", "Modified date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ModifiedDate.Hint", "The last modified date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AvailableDate", "Available date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AvailableDate.Hint", "The available date of the Nexport product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.EndDate", "End date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.EndDate.Hint", "The ending date of the Nexport product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CreditHours", "Credit hours");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CreditHours.Hint", "The credit hours of the Nexport product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.UniqueName", "Unique name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.UniqueName.Hint",
                "The unique name of the Nexport product. This is only applicable for Nexport syllabus items.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SectionCeus", "Section CEUs");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SectionCeus.Hint", "The CEUs for the Nexport section");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SectionNumber", "Section number");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SectionNumber.Hint", "The Nexport section number");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.IsSynchronized", "Synchronized?");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.IsSynchronized.Hint", "Indication of synchronization from Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.LastSynchronizationDate", "Last synchronization date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.LastSynchronizationDate.Hint", "The date that the last synchronization occurred");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AutoRedeem", "Auto redeeming purchase");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AutoRedeem.Hint", "Allows customers to redeem the product right after purchasing");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.StoreMapping", "Store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.StoreMapping.Hint",
                "The current store that the product is mapped to. If it is empty, this mapping is the default mapping.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.IsExtensionProduct", "Extension only product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.IsExtensionProduct.Hint", "Allows the product to become extension only product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductAccessExpirationOption", "Access expiration option");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint",
                "Specify the access expiration in Nexport. If specified, access expiration date will take priority compared to access expiration time limit.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.UtcAccessExpirationDate", "Access expiration date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint",
                "The access expiration date (in Coordinated Universal Time - UTC) in Nexport. The product access expiration date will be based on this date instead.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AccessTimeLimit", "Access expiration time limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AccessTimeLimit.Hint",
                "The access time limit in Nexport. The product access expiration date will be based on the date of the product redemption plus the access time limit.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.GroupId", "Group Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.GroupId.Hint", "The Id of the group in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.GroupName", "Group name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.GroupName.Hint", "The name of the group in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.GroupShortName", "Group short name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.GroupShortName.Hint", "The short name of the group in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AllowExtension", "Allow extension");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AllowExtension.Hint", "Allows customers to purchase the product as extension product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalWindow", "Renewal window");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalWindow.Hint",
                "The time period that customers can purchase the product to extend the expiration on the Nexport product. Customers can only make purchase if the current date is within the period from the current Nexport product expiration date minus the time window.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalDuration", "Renewal duration");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalDuration.Hint", "The time period that the enrollment can be extended");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalCompletionThreshold", "Enrollment completion threshold");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalCompletionThreshold.Hint",
                "When this completion threshold is met, the enrollment will either be reset if the completion percentage is below the threshold or be extended automatically based on the Auto approval method. If approval method is Manual, the action will be deferred to the selection from administrators.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalApprovalMethod", "Approval method");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RenewalApprovalMethod.Hint",
                "When approval method is set to manual, the administrator will be able to choose the choice between extending or restarting the enrollment when the completion percentage exceeds the completion threshold.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ExtensionPurchaseLimit", "Extension purchase limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ExtensionPurchaseLimit.Hint", "Limit how many times the customers can purchase the extension.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateSourceStoreMapping", "Mapping duplication source");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateSourceStoreMapping.Hint", "The store that contains the product mapping which will be duplicated from.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings", "Mapping duplication destinations");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings.Hint", "The list of stores that the product mapping will be duplicated to.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Training", "Nexport Training");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.UserId", "Nexport user Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.UserId.Hint", "The user Id in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgId", "Nexport owner organization Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgId.Hint", "The Id of the owner organization for the user");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgShortName", "Nexport owner organization short name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgShortName.Hint", "The short name of the owner organization for the user");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.UserFullName", "Nexport user full name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.UserFullName.Hint", "The full name of the user in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.Email", "Nexport email");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Nexport.Email.Hint", "The internal email of the user in Nexport");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Login.Fields.EmailOrUsername", "Email/Username");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.UserInfo", "Nexport user info");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.MyTrainings", "My trainings");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Order.ViewRedemption", "Launch this training");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Order.Redeem", "Redeem");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Text", "Question text");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Text.Hint", "The question text");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Description", "Description");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Description.Hint", "The description of the question");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Type", "Type");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Type.Hint", "The type of the question");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive", "Active?");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive.Hint", "Allows the question to be active");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Option.Text", "Option text");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Option.Text.Hint", "The question option text");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Customer.Edit", "Modify");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.SupplementalInfoAnswers", "Supplemental info answers");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.PageTitle", "Supplemental info answers");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.Edit.PageTitle", "Modify your answer(s)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.Nexport.RegistrationFields", "Nexport registration fields");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationFields.Description", "You can create and manage the registration fields and its categories available during registration below.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories", "Registration field categories");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Description", "You can create and manage the categories for registration fields below.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields", "Registration fields");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Description", "You can create and manage the registration fields below.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.AddNew", "Add a new registration field");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.EditFieldDetails", "Edit registration field details");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.BackToList", "back to registration field list");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Added", "The new registration field has been added successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Updated", "The registration field has been updated successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Deleted", "The registration field has been deleted successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit", "The selected custom render cannot be used for this registration field under the selected store(s).");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.AddNew", "Add a new registration field category");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.EditCategoryDetails", "Edit registration field category details");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.BackToList", "back to registration field category list");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Added", "The new registration field category has been added successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Updated", "The registration field category has been updated successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Deleted", "The registration field category has been deleted successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.SaveBeforeEdit", "You need to save the registration field before you can add its options. Only \"Select Dropdown\" and \"Select Checkbox\" field have additional field options.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.AddNew", "Add a new option value");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.EditOptionValueDetails", "Edit option value");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Added", "The new registration field option value has been added successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Updated", "The registration field option value has been updated successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Deleted", "The registration field option value has been deleted successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Customs.SaveBeforeEdit", "You need to save the registration field before you can select its custom render.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title", "Title");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title.Hint", "The name of the registration field category");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title.Required", "Registration field category is required to have a title.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Description", "Description");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Description.Hint", "Description of the registration field category");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder", "Display order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder.Hint", "The registration field category display order. 1 represents the first item in the list.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Name", "Name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Name.Hint", "The name of the registration field.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Type", "Type");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Type.Hint", "Choose how to display your registration field.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey", "Nexport custom profile field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey.Hint", "The custom profile field key in Nexport that will be used to synchronize the field.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Required", "Required");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Required.Hint", "When the registration field is required, the customer must choose appropriate value before they can continues.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Active", "Active");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Active.Hint", "If the registration field is not active, it will not be displayed.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Category", "Field category");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Category.Hint", "The category that the registration field will be displayed within.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Validation", "Validate before submitting");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Validation.Hint", "Use custom validation to validate the registration field before submitting it.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex", "Custom validation");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex.Hint", "Regular expression that will be used to validate the field before submitting.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage", "Validation message");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage.Hint", "The message that will be displayed when the custom validation has failed.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings", "Stores");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Stores", "Stores");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Stores.Hint", "Option to limit display the registration field to a certain store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder", "Display order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder.Hint", "The registration field display order. 1 represents the first item in the list.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender", "Custom field render");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender.Hint", "Choose which custom display will be used to render this registration field.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection", "Allow multiple option selections");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection.Hint", "Allow custom to select multiple options instead of single selection.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder", "Display options in ascending order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder.Hint", "Display the options in ascending order based on their values.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Info", "Registration field info");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.OptionValues", "Registration field option values");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Custom", "Registration field custom option");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value", "Option value");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value.Hint", "The value of the option");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value.Required", "The value for option is required");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.Options", "Additional options");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase", "Limit single product per checkout");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase.Hint", "Allowing customers to purchase single product within the category per checkout.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase", "Auto swapping between individual product");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase.Hint", "When customers select a different product within the category, the current product in the shopping cart will be replaced automatically with that new product item.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment", "Allow customers with existing enrollments to purchase additional products within this category");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment.Hint",
                "Allowing customers that currently have enrollments in Nexport to purchase additional products besides the products that associated with the enrollments in this category.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport", "Error occurred during the transferring to Nexport. You might not have an active subscription in Nexport Campus. Please contact customer service for further assistance.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser", "Failed to redeem Nexport invoice item {0} for user {1}");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure", "Error occurred during the redemption process for the order item invoice {0}");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved", "This item cannot be purchased at this time and will be removed at checkout!");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.DuplicatedProduct", "Cannot add duplicated product to the cart due to store restriction.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed", "Cannot purchase more than one for this product due to store restriction.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.SingleProductInCatalog", "You can only purchase single product within the same catalog.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart", "Cannot purchase more than one for some products in this shopping cart due to store restriction.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase", "Cannot purchase this product.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductName", "Product Name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductName.Hint", "Filter product mapping list by nexport product name.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductType", "Type");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductType.Hint", "Filter product mapping list by nexport product type.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId", "Store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId.Hint", "Filter registration fields list by store.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchStoreName", "Store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchStoreName.Hint", "Filter product mapping list by store.");
            
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreName", "Name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreName.Hint", "Filter stores list by name.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreUrl", "URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreUrl.Hint", "Filter stores list by URL.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName", "Name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName.Hint", "Filter registration fields list by name.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Wholesale.Go", "Go to created order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Wholesale.RedeemBy", "Redeem-By Date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited", "Unlimited redemption");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Organization", "Organization");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Order.GroupId", "Purchasing For Group/Org:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.Groups", "Nexport wholesale purchases");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Groups", "Nexport Groups");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Products", "Products");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions", "Assignments");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem", "Assign");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectTraining", "Training:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectCustomer", "Customer:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.AssignmentType", "AssignmentType:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.AssignmentType.Option1", "By Email");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.AssignmentType.Option2", "Instantly");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Modify", "Modify Assignment");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Errors.MixedRedemptionTypeNotAllowedInShoppingCart", "Cannot add this product to the other products in the cart due to restriction on the product mapping.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.AssignWhenRedeemed", "Assign When Redeemed");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Unassign.Confirmation", "Are you sure you want to unassign?");
        }

        public async Task DeleteResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Url");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Url.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Username");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Username.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Password");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Password.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AutoRenewToken");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AutoRenewToken.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.TokenNeverExpired");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.TokenNeverExpired.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CustomTokenExpirationDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Token");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.UtcExpirationDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.UtcExpirationDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.FindRootOrganization");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RootOrganizationId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RootOrganizationId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.MerchantAccountId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.MerchantAccountId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.NexportSubscriptionOrgId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel.Retail");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SaleModel.Wholesale");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchasePassedCourses");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AllowRepurchasePassedCourses.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.HideSectionCEUsInProductPage");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.HideSectionCEUsInProductPage.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.NexportProductName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.NexportProductName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DisplayName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DisplayName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CatalogSyllabusLinkId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CatalogId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CatalogId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SyllabusId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SyllabusId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgShortName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Type");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Type.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.PublishingModel");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.PublishingModel.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.PricingModel");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.PricingModel.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ModifiedDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ModifiedDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AvailableDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AvailableDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.EndDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.EndDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CreditHours");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CreditHours.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.UniqueName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.UniqueName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SectionCeus");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SectionCeus.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SectionNumber");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SectionNumber.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.IsSynchronized");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.IsSynchronized.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.LastSynchronizationDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.LastSynchronizationDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AutoRedeem");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AutoRedeem.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.StoreMapping");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.StoreMapping.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.IsExtensionProduct");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.IsExtensionProduct.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductAccessExpirationOption");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.UtcAccessExpirationDate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AccessTimeLimit");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AccessTimeLimit.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.GroupId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.GroupId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.GroupName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.GroupName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.GroupShortName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.GroupShortName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AllowExtension");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AllowExtension.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalWindow");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalWindow.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalDuration");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalDuration.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalCompletionThreshold");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalCompletionThreshold.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalApprovalMethod");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RenewalApprovalMethod.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ExtensionPurchaseLimit");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ExtensionPurchaseLimit.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateSourceStoreMapping");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateSourceStoreMapping.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Training");

            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.UserId");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.UserId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgId");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgShortName");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.OwnerOrgShortName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.UserFullName");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.UserFullName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.Email");
            await _localizationService.DeleteLocaleResourceAsync("Account.Fields.Nexport.Email.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Account.Login.Fields.EmailOrUsername");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.UserInfo");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.MyTrainings");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Order.ViewRedemption");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Order.Redeem");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Text");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Text.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Description");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Description.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Type");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.Type.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Option.Text");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Option.Text.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.SupplementalInfo.Customer.Edit");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.SupplementalInfoAnswers");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.PageTitle");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.MyAccount.SupplementalInfoAnswers.Edit.PageTitle");

            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.Nexport.RegistrationFields");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationFields.Description");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Description");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Description");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.AddNew");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.EditFieldDetails");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.BackToList");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Added");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Updated");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Deleted");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.AddNew");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.EditCategoryDetails");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.BackToList");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Added");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Updated");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Deleted");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.SaveBeforeEdit");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.AddNew");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.EditOptionValueDetails");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Added");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Updated");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Deleted");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Customers.Nexport.RegistrationField.Customs.SaveBeforeEdit");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title.Required");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Description");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Description.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Name");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Name.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Type");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Type.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Required");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Required.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Active");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Active.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Category");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Category.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Validation");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Validation.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.ValidationMessage.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Stores");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Stores.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.CustomRender.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.AllowMultipleSelection.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOptionValueByAscendingOrder.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Info");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.OptionValues");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Field.Custom");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.Options");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.ProductItemWilBeRemoved");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.DuplicatedProduct");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowed");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.SingleProductInCatalog");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.OverMaximumQuantityAllowedInShoppingCart");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.ProductNotEligibleForPurchase");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductType");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductType.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchStoreName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.ProductMapping.SearchStoreName.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreUrl");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Stores.SearchStoreUrl.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Wholesale.Go");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Wholesale.RedeemBy");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Organization");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Order.GroupId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Navigation.Groups");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Groups");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Products");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Modify");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Errors.MixedRedemptionTypeNotAllowedInShoppingCart");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.AssignWhenRedeemed");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Unassign.Confirmation");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectTraining");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectCustomer");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.AssignmentType");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.AssignmentType.Option1");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.AssignmentType.Option2");
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
