using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public class CancelPendingOrderRequestsPluginService
    {
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginLocalizationService _pluginLocalizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        
        private static readonly Dictionary<string, string> _localeResources = new()
        {
            {"CancellationRequests.ActiveNotice", "Note: You have an active cancellation request for this order"},
            {"CancellationRequests.WhyCancelling", "Why are you want to cancel this order?"},
            {"CancellationRequests.CancelReason", "Cancel reason"},
            {"CancellationRequests.Comments", "Comments"},
            {"CancellationRequests.Submit", "Submit Cancellation Request"},
            {"CancellationRequests.Submitted", "Your cancellation request has been submitted successfully."},
            {"Admin.CancellationRequests", "Cancellation requests"},
            {"Admin.CancellationRequests.EditCancellationRequestDetails", "Edit cancellation request details"},
            {"Admin.CancellationRequests.BackToList", "back to cancellation request list"},
            {"Admin.CancellationRequests.SearchCancellationRequestStatus.All", "All"},
            {"Admin.CancellationRequests.NotifyCustomer", "Notify customer about status change"},
            {"Admin.CancellationRequests.Deleted", "The cancellation request has been deleted successfully."},
            {"Admin.CancellationRequests.Updated", "The cancellation request has been updated successfully."},
            {"Admin.CancellationRequests.CannotModified", "This cancellation request has been processed and cannot be modified."},
            {"Admin.PendingOrderCancellationRequests", "Pending order cancellation requests"},
            {"Admin.PendingOrderCancellationRequests.Description", "The cancellation request feature enables customers to request cancellation for any pending order. Here you can find all submitted cancellation requests."},
            {"Admin.PendingOrderCancellationRequests.SearchStartDate", "Start date"},
            {"Admin.PendingOrderCancellationRequests.SearchStartDate.Hint", "The start date for the search"},
            {"Admin.PendingOrderCancellationRequests.SearchEndDate", "End date"},
            {"Admin.PendingOrderCancellationRequests.SearchEndDate.Hint", "The end date for the search"},
            {"Admin.PendingOrderCancellationRequests.RequestStatus", "Request status"},
            {"Admin.PendingOrderCancellationRequests.RequestStatus.Hint", "Search by a specific cancellation request status e.g. Received."},
            {"Admin.PendingOrderCancellationRequests.Fields.OrderId", "Order Id"},
            {"Admin.PendingOrderCancellationRequests.Fields.OrderId.Hint", "The Id of the order"},
            {"Admin.PendingOrderCancellationRequests.Fields.Customer", "Customer"},
            {"Admin.PendingOrderCancellationRequests.Fields.Customer.Hint", "The customer that requested the cancellation"},
            {"Admin.PendingOrderCancellationRequests.Fields.CustomerComments", "Customer comments"},
            {"Admin.PendingOrderCancellationRequests.Fields.CustomerComments.Hint", "The comments of the customer when requesting the cancellation"},
            {"Admin.PendingOrderCancellationRequests.Fields.RequestStatus", "Request status"},
            {"Admin.PendingOrderCancellationRequests.Fields.RequestStatus.Hint", "The status of the request"},
            {"Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation", "Cancellation reason"},
            {"Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation.Hint", "The reason for cancellation"},
            {"Admin.PendingOrderCancellationRequests.Fields.StaffNotes", "Staff notes"},
            {"Admin.PendingOrderCancellationRequests.Fields.StaffNotes.Hint", "The notes from staff member"},
            {"Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate", "Created on"},
            {"Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate.Hint", "The date/time the request was made"},
            {"Admin.PendingOrderCancellationRequests.Errors.ModifiedLocaleResources",
                "There are modified resource values within the <b>Cancel pending order requests plugin</b> that conflicts with the default value. Review them <a href=\"javascript:OpenWindow(\'{0}\', 800, 500, true)\">here</a>."},
            {"ActivityLog.EditCancellationRequest", "Edited a cancellation request (ID = {0})"},
            {"ActivityLog.DeleteCancellationRequest", "Deleted a cancellation request (ID = {0})"},

            {"Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.StoreOwnerNotification", "This message template is used when a new cancellation request is created. The message is received by a store owner."},
            {"Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.CustomerNotification", "This message template is used to notify a customer about a new cancellation request submitted from his/her account."},
            {"Admin.ContentManagement.MessageTemplates.Description.CancellationRequestAccepted.CustomerNotification", "This message template is used to notify a customer that the cancellation request has been accepted."},
            {"Admin.ContentManagement.MessageTemplates.Description.CancellationRequestRejected.CustomerNotification", "This message template is used to notify a customer that the cancellation request has been rejected."},

            {"Admin.Configuration.Settings.Order.CancellationRequestSettings", "Cancellation request settings"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.Name", "Name"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.Name.Hint", "The cancellation request reason name"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder", "Display order"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder.Hint", "The cancellation request reason display order. 1 represents the first item in the list."},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons", "Cancellation request reasons"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.Hint", "List of reasons a customer will be able to choose when submitting a cancellation request."},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.AddNew", "Add new cancellation request reason"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.BackToList", "back to cancellation request reason list"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.EditDetails", "Edit cancellation request reason details"},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.Added", "The new cancellation request reason has been added successfully."},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.Deleted", "The cancellation request reason has been deleted successfully."},
            {"Admin.Configuration.Settings.Order.CancellationRequestReasons.Updated", "The cancellation request reason has been updated successfully."}
        };

        public static Dictionary<string, string> GetLocaleResource()
        {
            return _localeResources;
        }

        public CancelPendingOrderRequestsPluginService(
            IRepository<ActivityLogType> activityLogTypeRepository,
            EmailAccountSettings emailAccountSettings,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPluginLocalizationService pluginLocalizationService,
            IMessageTemplateService messageTemplateService,
            ISettingService settingService,
            ILogger logger)
        {
            _activityLogTypeRepository = activityLogTypeRepository;
            _emailAccountSettings = emailAccountSettings;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _pluginLocalizationService = pluginLocalizationService;
            _messageTemplateService = messageTemplateService;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task AddActivityLogTypesAsync()
        {
            var customerActivityLogTypes = (await _customerActivityService.GetAllActivityTypesAsync())
                .Where(x => x.SystemKeyword.Contains("CancellationRequest"));
            var activityLogTypes = customerActivityLogTypes.ToArray();

            if (!activityLogTypes.Any(x =>
                x.SystemKeyword.Equals(PluginDefaults.EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType()
                {
                    Name = "Edit a cancellation request",
                    SystemKeyword = PluginDefaults.EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!activityLogTypes.Any(x =>
                x.SystemKeyword.Equals(PluginDefaults.DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE)))
            {
                await _activityLogTypeRepository.InsertAsync(new ActivityLogType()
                {
                    Name = "Delete a cancellation request",
                    SystemKeyword = PluginDefaults.DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }
        }

        public async Task DeleteActivityLogTypesAsync()
        {
            var customerActivityLogTypes = (await _customerActivityService.GetAllActivityTypesAsync())
                .Where(x => x.SystemKeyword.Contains("CancellationRequest"));
            foreach (var type in customerActivityLogTypes)
            {
                await _activityLogTypeRepository.DeleteAsync(type);
            }
        }

        public async Task AddMessageTemplatesAsync()
        {
            var messageTemplates = await _messageTemplateService.GetAllMessageTemplatesAsync(0);
            if (!messageTemplates.Any(x =>
                x.Name.Equals(PluginDefaults.NEW_CANCELLATION_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = PluginDefaults.NEW_CANCELLATION_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "%Store.Name%. New cancellation request.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Customer.FullName% has just submitted a cancellation return request. Details are below:{Environment.NewLine}<br />{Environment.NewLine}Request ID: %CancellationRequest.Id%{Environment.NewLine}<br />{Environment.NewLine}Order ID: %CancellationRequest.OrderId%{Environment.NewLine}<br />{Environment.NewLine}Reason for cancel: %CancellationRequest.Reason%{Environment.NewLine}<br />{Environment.NewLine}Customer comments:{Environment.NewLine}<br />{Environment.NewLine}%CancellationRequest.CustomerComment%</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }

            if (!messageTemplates.Any(x =>
                x.Name.Equals(PluginDefaults.NEW_CANCELLATION_REQUEST_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = PluginDefaults.NEW_CANCELLATION_REQUEST_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "%Store.Name%. New cancellation request.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%!{Environment.NewLine}<br />{Environment.NewLine}You have just submitted a new cancellation request. Details are below:{Environment.NewLine}<br />{Environment.NewLine}Request ID: %CancellationRequest.Id%{Environment.NewLine}<br />{Environment.NewLine}Order ID: %CancellationRequest.OrderId%{Environment.NewLine}<br />{Environment.NewLine}Reason for return: %CancellationRequest.Reason%{Environment.NewLine}<br />{Environment.NewLine}Customer comments:{Environment.NewLine}<br />{Environment.NewLine}%CancellationRequest.CustomerComment%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }

            if (!messageTemplates.Any(x =>
                x.Name.Equals(PluginDefaults.CANCELLATION_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = PluginDefaults.CANCELLATION_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "%Store.Name%. Cancellation request status.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}Your cancellation request #%CancellationRequest.Id% for order #%CancellationRequest.OrderId% has been accepted. The order will be cancelled shortly and you will receive additional email regarding about the order cancellation.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }

            if (!messageTemplates.Any(x =>
                x.Name.Equals(PluginDefaults.CANCELLATION_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = PluginDefaults.CANCELLATION_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "%Store.Name%. Cancellation request status.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}Your cancellation request #%CancellationRequest.Id% for order #%CancellationRequest.OrderId% has been rejected. Please contact Customer Service for further details.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }
        }

        public async Task DeleteMessageTemplatesAsync()
        {
            var messageTemplates = (await _messageTemplateService.GetAllMessageTemplatesAsync(0))
                .Where(x => x.Name.Contains("CancellationRequest"));
            foreach (var messageTemplate in messageTemplates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);
            }
        }

        public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int languageId = 1)
        {
            // if this returns back false, then it didn't exist
            if (await _pluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue))
            {
                // this is if we have multiple modified values
                var cancelPendingOrderRequestsSetting =
                    await _settingService.GetSettingAsync("Plugin.Sale.CancelPendingOrderRequests.HasModifiedLocaleResources");
              
                if (cancelPendingOrderRequestsSetting == null)
                {
                    await _settingService.SetSettingAsync<bool>("Plugin.Sale.CancelPendingOrderRequests.HasModifiedLocaleResources", true);
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
    }
}
