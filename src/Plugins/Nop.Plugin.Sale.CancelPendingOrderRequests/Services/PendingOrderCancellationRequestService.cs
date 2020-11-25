using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Html;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;
using Nop.Services.Common;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public class PendingOrderCancellationRequestService : IPendingOrderCancellationRequestService
    {
        private readonly EmailAccountSettings _emailAccountSettings;

        private readonly IRepository<PendingOrderCancellationRequest> _pendingOrderCancellationRequestRepository;
        private readonly IRepository<PendingOrderCancellationRequestReason> _pendingOrderCancellationRequestReasonRepository;

        private readonly IAddressService _addressService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreService _storeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger _logger;

        public PendingOrderCancellationRequestService(
            EmailAccountSettings emailAccountSettings,
            IRepository<PendingOrderCancellationRequest> pendingOrderCancellationRequestRepository,
            IRepository<PendingOrderCancellationRequestReason> pendingOrderCancellationRequestReasonRepository,
            IAddressService addressService,
            IWorkflowMessageService workflowMessageService,
            ICustomerService customerService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            IStoreService storeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IEventPublisher eventPublisher,
            ILogger logger)
        {
            _emailAccountSettings = emailAccountSettings;
            _pendingOrderCancellationRequestRepository = pendingOrderCancellationRequestRepository;
            _pendingOrderCancellationRequestReasonRepository = pendingOrderCancellationRequestReasonRepository;
            _addressService = addressService;
            _workflowMessageService = workflowMessageService;
            _customerService = customerService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _storeService = storeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        #region Utilities

        /// <summary>
        /// Ensure language is active
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Return a value language identifier</returns>
        private int EnsureLanguageIsActive(int languageId, int storeId)
        {
            //load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = _languageService.GetAllLanguages(storeId: storeId).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = _languageService.GetAllLanguages().FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }

        /// <summary>
        /// Get active message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>List of message templates</returns>
        private IList<MessageTemplate> GetActiveMessageTemplates(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = _messageTemplateService.GetMessageTemplatesByName(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>EmailAccount</returns>
        private EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = _localizationService.GetLocalized(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (_emailAccountService.GetEmailAccountById(emailAccountId) ??
                                _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)) ??
                               _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;
        }

        #endregion

        public IPagedList<PendingOrderCancellationRequest> SearchCancellationRequests(
            int storeId = 0, int customerId = 0,
            PendingOrderCancellationRequestStatus? requestStatus = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _pendingOrderCancellationRequestRepository.Table;
            if (storeId > 0)
                query = query.Where(request => storeId == request.StoreId);
            if (customerId > 0)
                query = query.Where(request => customerId == request.CustomerId);
            if (requestStatus.HasValue)
            {
                var returnStatusId = (int)requestStatus.Value;
                query = query.Where(request => (int)request.RequestStatus == returnStatusId);
            }

            if (createdFromUtc.HasValue)
                query = query.Where(request => createdFromUtc.Value <= request.UtcCreatedDate);
            if (createdToUtc.HasValue)
                query = query.Where(request => createdToUtc.Value >= request.UtcCreatedDate);

            query = query.OrderByDescending(request => request.UtcCreatedDate)
                .ThenByDescending(request => request.Id);

            var cancellationRequests =
                new PagedList<PendingOrderCancellationRequest>(query, pageIndex, pageSize, getOnlyTotalCount);

            return cancellationRequests;
        }

        public void InsertCancellationRequestReason(PendingOrderCancellationRequestReason cancellationRequestReason)
        {
            if (cancellationRequestReason == null)
                throw new ArgumentNullException(nameof(cancellationRequestReason));

            _pendingOrderCancellationRequestReasonRepository.Insert(cancellationRequestReason);

            _eventPublisher.EntityInserted(cancellationRequestReason);
        }

        public void DeleteCancellationRequestReason(PendingOrderCancellationRequestReason cancellationRequestReason)
        {
            if (cancellationRequestReason == null)
                throw new ArgumentNullException(nameof(cancellationRequestReason));

            if (_pendingOrderCancellationRequestReasonRepository.Table.Count() == 1)
                throw new NopException("You cannot delete cancellation request reason. At least one cancellation request reason is required.");

            _pendingOrderCancellationRequestReasonRepository.Delete(cancellationRequestReason);

            _eventPublisher.EntityDeleted(cancellationRequestReason);
        }

        public void UpdateCancellationRequestReason(PendingOrderCancellationRequestReason cancellationRequestReason)
        {
            if (cancellationRequestReason == null)
                throw new ArgumentNullException(nameof(cancellationRequestReason));

            _pendingOrderCancellationRequestReasonRepository.Update(cancellationRequestReason);

            _eventPublisher.EntityUpdated(cancellationRequestReason);
        }

        public IList<PendingOrderCancellationRequestReason> GetAllCancellationRequestReasons()
        {
            var query = from reason in _pendingOrderCancellationRequestReasonRepository.Table
                        orderby reason.DisplayOrder, reason.Id
                        select reason;
            return query.ToList();
        }

        public PendingOrderCancellationRequestReason GetCancellationRequestReasonById(int reasonId)
        {
            return reasonId < 1 ? null : _pendingOrderCancellationRequestReasonRepository.GetById(reasonId);
        }

        public bool HasCancellationRequestForOrder(int orderId)
        {
            return orderId > 0 && _pendingOrderCancellationRequestRepository.Table.Any(
                r => r.OrderId == orderId &&
                     r.RequestStatus == PendingOrderCancellationRequestStatus.Received);
        }

        public void InsertCancellationRequest(PendingOrderCancellationRequest cancellationRequest)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            if (_pendingOrderCancellationRequestRepository.Table.Any(x => x.OrderId == cancellationRequest.OrderId))
                return;

            _pendingOrderCancellationRequestRepository.Insert(cancellationRequest);

            _eventPublisher.EntityInserted(cancellationRequest);
        }

        public void DeleteCancellationRequest(PendingOrderCancellationRequest cancellationRequest)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            _pendingOrderCancellationRequestRepository.Delete(cancellationRequest);

            _eventPublisher.EntityDeleted(cancellationRequest);
        }

        public void UpdateCancellationRequest(PendingOrderCancellationRequest cancellationRequest)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            _pendingOrderCancellationRequestRepository.Update(cancellationRequest);

            _eventPublisher.EntityUpdated(cancellationRequest);
        }

        public PendingOrderCancellationRequest GetCancellationRequestById(int requestId)
        {
            return requestId < 1 ? null : _pendingOrderCancellationRequestRepository.GetById(requestId);
        }

        public void AddCancellationRequestTokens(IList<Token> tokens, PendingOrderCancellationRequest cancellationRequest, Order order)
        {
            tokens.Add(new Token("CancellationRequest.Id", cancellationRequest.Id));
            tokens.Add(new Token("CancellationRequest.OrderId", order.Id));
            tokens.Add(new Token("CancellationRequest.Reason", cancellationRequest.ReasonForCancellation));
            tokens.Add(new Token("CancellationRequest.CustomerComment", HtmlHelper.FormatText(cancellationRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("CancellationRequest.StaffNotes", HtmlHelper.FormatText(cancellationRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("CancellationRequest.Status", _localizationService.GetLocalizedEnum(cancellationRequest.RequestStatus)));

            _eventPublisher.EntityTokensAdded(cancellationRequest, tokens);
        }

        public IList<int> SendNewCancellationRequestStoreOwnerNotification(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplates = GetActiveMessageTemplates(PluginDefaults.NEW_CANCELLATION_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = _customerService.GetCustomerById(cancellationRequest.CustomerId)
                           ?? throw new Exception($"Customer with Id {cancellationRequest.CustomerId} does not existed");

            var commonTokens = new List<Token>();
            _messageTokenProvider.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(commonTokens, customer);
            AddCancellationRequestTokens(commonTokens, cancellationRequest, order);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return _workflowMessageService.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public IList<int> SendNewCancellationRequestCustomerNotification(PendingOrderCancellationRequest cancellationRequest, Order order, int languageId)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplates = GetActiveMessageTemplates(PluginDefaults.NEW_CANCELLATION_REQUEST_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = _customerService.GetCustomerById(cancellationRequest.CustomerId)
                           ?? throw new Exception($"Customer with Id {cancellationRequest.CustomerId} does not existed");

            var commonTokens = new List<Token>();
            _messageTokenProvider.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(commonTokens, customer);
            AddCancellationRequestTokens(commonTokens, cancellationRequest, order);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = _customerService.IsGuest(customer) ?
                    billingAddress.Email :
                    customer.Email;
                var toName = _customerService.IsGuest(customer) ?
                    billingAddress.FirstName :
                    _customerService.GetCustomerFullName(customer);

                return _workflowMessageService.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public IList<int> SendCancellationRequestCustomerNotification(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId, string template)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplates = GetActiveMessageTemplates(template, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = _customerService.GetCustomerById(cancellationRequest.CustomerId)
                           ?? throw new Exception($"Customer with Id {cancellationRequest.CustomerId} does not existed");

            var commonTokens = new List<Token>();
            _messageTokenProvider.AddOrderTokens(commonTokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(commonTokens, customer);
            AddCancellationRequestTokens(commonTokens, cancellationRequest, order);

            return messageTemplates.Select(messageTemplate =>
            {
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                var toEmail = _customerService.IsGuest(customer) ?
                    billingAddress.Email :
                    customer.Email;
                var toName = _customerService.IsGuest(customer) ?
                    billingAddress.FirstName :
                    _customerService.GetCustomerFullName(customer);

                return _workflowMessageService.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToList();
        }

        public void VoidCancelledOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                throw new NopException("You can't void this order");

            if (order.PaymentStatus == PaymentStatus.Authorized)
            {
                if (_paymentService.SupportVoid(order.PaymentMethodSystemName))
                {
                    var request = new VoidPaymentRequest();
                    try
                    {
                        request.Order = order;
                        var result = _paymentService.Void(request);

                        if (result.Success)
                        {
                            order.PaymentStatus = result.NewPaymentStatus;
                            _orderService.UpdateOrder(order);

                            _orderService.InsertOrderNote(new OrderNote
                            {
                                OrderId = order.Id,
                                Note = "Order has been voided",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error: {ex.Message}", ex);
                    }
                }
                else
                {
                    order.PaymentStatusId = (int)PaymentStatus.Voided;
                    _orderService.UpdateOrder(order);

                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order has been voided",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
            }
            else if (order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatusId = (int)PaymentStatus.Voided;
                _orderService.UpdateOrder(order);

                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "Order has been voided",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }
        }
    }
}