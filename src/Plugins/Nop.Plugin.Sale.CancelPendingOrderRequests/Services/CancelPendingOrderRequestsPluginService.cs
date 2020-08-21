using System;
using System.Linq;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public class CancelPendingOrderRequestsPluginService
    {
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ILogger _logger;

        public CancelPendingOrderRequestsPluginService(
            EmailAccountSettings emailAccountSettings,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            ILogger logger)
        {
            _emailAccountSettings = emailAccountSettings;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
            _logger = logger;
        }

        public void AddActivityLogTypes()
        {
            var customerActivityLogTypes = _customerActivityService.GetAllActivityTypes()
                .Where(x => x.SystemKeyword.Contains("CancellationRequest"));
            var activityLogTypes = customerActivityLogTypes.ToArray();

            if (!activityLogTypes.Any(x =>
                x.SystemKeyword.Equals(PluginDefaults.EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE)))
            {
                _customerActivityService.InsertActivityType(new ActivityLogType()
                {
                    Name = "Edit a cancellation request",
                    SystemKeyword = PluginDefaults.EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }

            if (!activityLogTypes.Any(x =>
                x.SystemKeyword.Equals(PluginDefaults.DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE)))
            {
                _customerActivityService.InsertActivityType(new ActivityLogType()
                {
                    Name = "Delete a cancellation request",
                    SystemKeyword = PluginDefaults.DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                    Enabled = true
                });
            }
        }

        public void DeleteActivityLogTypes()
        {
            var customerActivityLogTypes = _customerActivityService.GetAllActivityTypes()
                .Where(x => x.SystemKeyword.Contains("CancellationRequest"));
            foreach (var type in customerActivityLogTypes)
            {
                _customerActivityService.DeleteActivityType(type);
            }
        }

        public void AddMessageTemplates()
        {
            var messageTemplates = _messageTemplateService.GetAllMessageTemplates(0);
            if (!messageTemplates.Any(x =>
                x.Name.Equals(PluginDefaults.NEW_CANCELLATION_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE)))
            {
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate
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
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate
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
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate
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
                _messageTemplateService.InsertMessageTemplate(new MessageTemplate
                {
                    Name = PluginDefaults.CANCELLATION_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE,
                    Subject = "%Store.Name%. Cancellation request status.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />{Environment.NewLine}Your cancellation request #%CancellationRequest.Id% for order #%CancellationRequest.OrderId% has been rejected. Please contact Customer Service for further details.{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = _emailAccountSettings.DefaultEmailAccountId
                });
            }
        }

        public void DeleteMessageTemplates()
        {
            var messageTemplates = _messageTemplateService.GetAllMessageTemplates(0)
                .Where(x => x.Name.Contains("CancellationRequest"));
            foreach (var messageTemplate in messageTemplates)
            {
                _messageTemplateService.DeleteMessageTemplate(messageTemplate);
            }
        }

        public void AddOrUpdateResources()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("CancellationRequests.ActiveNotice", "Note: You have an active cancellation request for this order");
            _localizationService.AddOrUpdatePluginLocaleResource("CancellationRequests.WhyCancelling", "Why are you want to cancel this order?");
            _localizationService.AddOrUpdatePluginLocaleResource("CancellationRequests.CancelReason", "Cancel reason");
            _localizationService.AddOrUpdatePluginLocaleResource("CancellationRequests.Comments", "Comments");
            _localizationService.AddOrUpdatePluginLocaleResource("CancellationRequests.Submit", "Submit Cancellation Request");
            _localizationService.AddOrUpdatePluginLocaleResource("CancellationRequests.Submitted", "Your cancellation request has been submitted successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests", "Cancellation requests");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.EditCancellationRequestDetails", "Edit cancellation request details");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.BackToList", "back to cancellation request list");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.SearchCancellationRequestStatus.All", "All");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.NotifyCustomer", "Notify customer about status change");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.Deleted", "The cancellation request has been deleted successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.Updated", "The cancellation request has been updated successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.CancellationRequests.CannotModified", "This cancellation request has been processed and cannot be modified.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests", "Pending order cancellation requests");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Description", "The cancellation request feature enables customers to request cancellation for any pending order. Here you can find all submitted cancellation requests.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchStartDate", "Start date");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchStartDate.Hint", "The start date for the search");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchEndDate", "End date");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchEndDate.Hint", "The end date for the search");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.RequestStatus", "Request status");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.RequestStatus.Hint", "Search by a specific cancellation request status e.g. Received.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.OrderId", "Order Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.OrderId.Hint", "The Id of the order");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.Customer", "Customer");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.Customer.Hint", "The customer that requested the cancellation");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.CustomerComments", "Customer comments");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.CustomerComments.Hint", "The comments of the customer when requesting the cancellation");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.RequestStatus", "Request status");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.RequestStatus.Hint", "The status of the request");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation", "Cancellation reason");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation.Hint", "The reason for cancellation");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.StaffNotes", "Staff notes");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.StaffNotes.Hint", "The notes from staff member");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate", "Created on");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate.Hint", "The date/time the request was made");
            _localizationService.AddOrUpdatePluginLocaleResource("ActivityLog.EditCancellationRequest", "Edited a cancellation request (ID = {0})");
            _localizationService.AddOrUpdatePluginLocaleResource("ActivityLog.DeleteCancellationRequest", "Deleted a cancellation request (ID = {0})");

            _localizationService.AddOrUpdatePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.StoreOwnerNotification", "This message template is used when a new cancellation request is created. The message is received by a store owner.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.CustomerNotification", "This message template is used to notify a customer about a new cancellation request submitted from his/her account.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestAccepted.CustomerNotification", "This message template is used to notify a customer that the cancellation request has been accepted.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestRejected.CustomerNotification", "This message template is used to notify a customer that the cancellation request has been rejected.");

            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestSettings", "Cancellation request settings");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name", "Name");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name.Hint", "The cancellation request reason name");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder", "Display order");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder.Hint", "The cancellation request reason display order. 1 represents the first item in the list.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons", "Cancellation request reasons");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Hint", "List of reasons a customer will be able to choose when submitting a cancellation request.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.AddNew", "Add new cancellation request reason");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.BackToList", "back to cancellation request reason list");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.EditDetails", "Edit cancellation request reason details");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Added", "The new cancellation request reason has been added successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Deleted", "The cancellation request reason has been deleted successfully.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Updated", "The cancellation request reason has been updated successfully.");
        }

        public void DeleteResources()
        {
            _localizationService.DeletePluginLocaleResource("CancellationRequests.ActiveNotice");
            _localizationService.DeletePluginLocaleResource("CancellationRequests.WhyCancelling");
            _localizationService.DeletePluginLocaleResource("CancellationRequests.CancelReason");
            _localizationService.DeletePluginLocaleResource("CancellationRequests.Comments");
            _localizationService.DeletePluginLocaleResource("CancellationRequests.Submit");
            _localizationService.DeletePluginLocaleResource("CancellationRequests.Submitted");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.EditCancellationRequestDetails");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.BackToList");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.SearchCancellationRequestStatus.All");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.NotifyCustomer");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.Deleted");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.Updated");
            _localizationService.DeletePluginLocaleResource("Admin.CancellationRequests.CannotModified");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Description");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchStartDate");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchStartDate.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchEndDate");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.SearchEndDate.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.RequestStatus");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.RequestStatus.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.OrderId");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.OrderId.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.Customer");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.Customer.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.CustomerComments");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.CustomerComments.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.RequestStatus");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.RequestStatus.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.StaffNotes");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.StaffNotes.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate");
            _localizationService.DeletePluginLocaleResource("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate.Hint");
            _localizationService.DeletePluginLocaleResource("ActivityLog.EditCancellationRequest");
            _localizationService.DeletePluginLocaleResource("ActivityLog.DeleteCancellationRequest");

            _localizationService.DeletePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.StoreOwnerNotification");
            _localizationService.DeletePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.NewCancellationRequest.CustomerNotification");
            _localizationService.DeletePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestAccepted.CustomerNotification");
            _localizationService.DeletePluginLocaleResource("Admin.ContentManagement.MessageTemplates.Description.CancellationRequestRejected.CustomerNotification");

            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestSettings");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.AddNew");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.BackToList");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.EditDetails");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Added");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Deleted");
            _localizationService.DeletePluginLocaleResource("Admin.Configuration.Settings.Order.CancellationRequestReasons.Updated");
        }
    }
}
