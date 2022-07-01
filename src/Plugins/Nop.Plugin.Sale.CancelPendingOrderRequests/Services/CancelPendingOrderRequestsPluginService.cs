using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Data;
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
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ILogger _logger;

        public CancelPendingOrderRequestsPluginService(
            IRepository<ActivityLogType> activityLogTypeRepository,
            EmailAccountSettings emailAccountSettings,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            ILogger logger)
        {
            _activityLogTypeRepository = activityLogTypeRepository;
            _emailAccountSettings = emailAccountSettings;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
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

        public async Task AddOrUpdateResourcesAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("CancellationRequests.ActiveNotice", "Note: You have an active cancellation request for this order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("CancellationRequests.WhyCancelling", "Why are you want to cancel this order?");
            await _localizationService.AddOrUpdateLocaleResourceAsync("CancellationRequests.CancelReason", "Cancel reason");
            await _localizationService.AddOrUpdateLocaleResourceAsync("CancellationRequests.Comments", "Comments");
            await _localizationService.AddOrUpdateLocaleResourceAsync("CancellationRequests.Submit", "Submit Cancellation Request");
            await _localizationService.AddOrUpdateLocaleResourceAsync("CancellationRequests.Submitted", "Your cancellation request has been submitted successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests", "Cancellation requests");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.EditCancellationRequestDetails", "Edit cancellation request details");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.BackToList", "back to cancellation request list");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.SearchCancellationRequestStatus.All", "All");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.NotifyCustomer", "Notify customer about status change");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.Deleted", "The cancellation request has been deleted successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.Updated", "The cancellation request has been updated successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.CancellationRequests.CannotModified", "This cancellation request has been processed and cannot be modified.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests", "Pending order cancellation requests");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Description", "The cancellation request feature enables customers to request cancellation for any pending order. Here you can find all submitted cancellation requests.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchStartDate", "Start date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchStartDate.Hint", "The start date for the search");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchEndDate", "End date");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchEndDate.Hint", "The end date for the search");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.RequestStatus", "Request status");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.RequestStatus.Hint", "Search by a specific cancellation request status e.g. Received.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.OrderId", "Order Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.OrderId.Hint", "The Id of the order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.Customer", "Customer");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.Customer.Hint", "The customer that requested the cancellation");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.CustomerComments", "Customer comments");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.CustomerComments.Hint", "The comments of the customer when requesting the cancellation");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.RequestStatus", "Request status");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.RequestStatus.Hint", "The status of the request");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation", "Cancellation reason");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation.Hint", "The reason for cancellation");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.StaffNotes", "Staff notes");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.StaffNotes.Hint", "The notes from staff member");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate", "Created on");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate.Hint", "The date/time the request was made");
            await _localizationService.AddOrUpdateLocaleResourceAsync("ActivityLog.EditCancellationRequest", "Edited a cancellation request (ID = {0})");
            await _localizationService.AddOrUpdateLocaleResourceAsync("ActivityLog.DeleteCancellationRequest", "Deleted a cancellation request (ID = {0})");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.StoreOwnerNotification", "This message template is used when a new cancellation request is created. The message is received by a store owner.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.CustomerNotification", "This message template is used to notify a customer about a new cancellation request submitted from his/her account.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestAccepted.CustomerNotification", "This message template is used to notify a customer that the cancellation request has been accepted.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestRejected.CustomerNotification", "This message template is used to notify a customer that the cancellation request has been rejected.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestSettings", "Cancellation request settings");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name", "Name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name.Hint", "The cancellation request reason name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder", "Display order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder.Hint", "The cancellation request reason display order. 1 represents the first item in the list.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons", "Cancellation request reasons");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Hint", "List of reasons a customer will be able to choose when submitting a cancellation request.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.AddNew", "Add new cancellation request reason");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.BackToList", "back to cancellation request reason list");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.EditDetails", "Edit cancellation request reason details");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Added", "The new cancellation request reason has been added successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Deleted", "The cancellation request reason has been deleted successfully.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Updated", "The cancellation request reason has been updated successfully.");
        }

        public async Task DeleteResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourceAsync("CancellationRequests.ActiveNotice");
            await _localizationService.DeleteLocaleResourceAsync("CancellationRequests.WhyCancelling");
            await _localizationService.DeleteLocaleResourceAsync("CancellationRequests.CancelReason");
            await _localizationService.DeleteLocaleResourceAsync("CancellationRequests.Comments");
            await _localizationService.DeleteLocaleResourceAsync("CancellationRequests.Submit");
            await _localizationService.DeleteLocaleResourceAsync("CancellationRequests.Submitted");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.EditCancellationRequestDetails");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.BackToList");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.SearchCancellationRequestStatus.All");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.NotifyCustomer");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.Deleted");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.Updated");
            await _localizationService.DeleteLocaleResourceAsync("Admin.CancellationRequests.CannotModified");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Description");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchStartDate");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchStartDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchEndDate");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.SearchEndDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.RequestStatus");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.RequestStatus.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.OrderId");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.OrderId.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.Customer");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.Customer.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.CustomerComments");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.CustomerComments.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.RequestStatus");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.RequestStatus.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.StaffNotes");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.StaffNotes.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate");
            await _localizationService.DeleteLocaleResourceAsync("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate.Hint");
            await _localizationService.DeleteLocaleResourceAsync("ActivityLog.EditCancellationRequest");
            await _localizationService.DeleteLocaleResourceAsync("ActivityLog.DeleteCancellationRequest");

            await _localizationService.DeleteLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.StoreOwnerNotification");
            await _localizationService.DeleteLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.CustomerNotification");
            await _localizationService.DeleteLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestAccepted.CustomerNotification");
            await _localizationService.DeleteLocaleResourceAsync("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestRejected.CustomerNotification");

            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestSettings");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.AddNew");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.BackToList");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.EditDetails");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Added");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Deleted");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Updated");
        }
    }
}
