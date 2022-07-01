using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Events;
using Nop.Data;
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
using Nop.Services.Html;

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
        private readonly IHtmlFormatter _htmlFormatter;
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
            IHtmlFormatter htmlFormatter,
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
            _htmlFormatter = htmlFormatter;
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
        protected async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
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
        private async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

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
        private async Task<EmailAccount> GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = await (_emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ??
                                _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }

        #endregion

        public async Task<IPagedList<PendingOrderCancellationRequest>> SearchCancellationRequestsAsync(int storeId = 0,
            int customerId = 0,
            PendingOrderCancellationRequestStatus? requestStatus = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
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

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertCancellationRequestReasonAsync(PendingOrderCancellationRequestReason cancellationRequestReason)
        {
            if (cancellationRequestReason == null)
                throw new ArgumentNullException(nameof(cancellationRequestReason));

            await _pendingOrderCancellationRequestReasonRepository.InsertAsync(cancellationRequestReason);
        }

        public async Task DeleteCancellationRequestReasonAsync(PendingOrderCancellationRequestReason cancellationRequestReason)
        {
            if (cancellationRequestReason == null)
                throw new ArgumentNullException(nameof(cancellationRequestReason));

            if (_pendingOrderCancellationRequestReasonRepository.Table.Count() == 1)
                throw new NopException("You cannot delete cancellation request reason. At least one cancellation request reason is required.");

            await _pendingOrderCancellationRequestReasonRepository.DeleteAsync(cancellationRequestReason);
        }

        public async Task UpdateCancellationRequestReasonAsync(PendingOrderCancellationRequestReason cancellationRequestReason)
        {
            if (cancellationRequestReason == null)
                throw new ArgumentNullException(nameof(cancellationRequestReason));

            await _pendingOrderCancellationRequestReasonRepository.UpdateAsync(cancellationRequestReason);
        }

        public async Task<IList<PendingOrderCancellationRequestReason>> GetAllCancellationRequestReasonsAsync()
        {
            var query =
                _pendingOrderCancellationRequestReasonRepository
                    .Table
                    .OrderBy(reason => reason.DisplayOrder)
                    .ThenBy(reason => reason.Id);
            return await query.ToListAsync();
        }

        public async Task<PendingOrderCancellationRequestReason> GetCancellationRequestReasonByIdAsync(int reasonId)
        {
            return reasonId < 1
                ? null
                : await _pendingOrderCancellationRequestReasonRepository.GetByIdAsync(reasonId);
        }

        public async Task<bool> HasCancellationRequestForOrderAsync(int orderId)
        {
            return orderId > 0 && await _pendingOrderCancellationRequestRepository.Table.AnyAsync(
                r => r.OrderId == orderId &&
                     r.RequestStatus == PendingOrderCancellationRequestStatus.Received);
        }

        public async Task InsertCancellationRequestAsync(PendingOrderCancellationRequest cancellationRequest)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            if (_pendingOrderCancellationRequestRepository.Table.Any(x => x.OrderId == cancellationRequest.OrderId))
                return;

            await _pendingOrderCancellationRequestRepository.InsertAsync(cancellationRequest);
        }

        public async Task DeleteCancellationRequestAsync(PendingOrderCancellationRequest cancellationRequest)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            await _pendingOrderCancellationRequestRepository.DeleteAsync(cancellationRequest);
        }

        public async Task UpdateCancellationRequestAsync(PendingOrderCancellationRequest cancellationRequest)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            await _pendingOrderCancellationRequestRepository.UpdateAsync(cancellationRequest);
        }

        public async Task<PendingOrderCancellationRequest> GetCancellationRequestByIdAsync(int requestId)
        {
            return requestId < 1
                ? null
                : await _pendingOrderCancellationRequestRepository.GetByIdAsync(requestId);
        }

        public async Task AddCancellationRequestTokensAsync(IList<Token> tokens, PendingOrderCancellationRequest cancellationRequest, Order order)
        {
            tokens.Add(new Token("CancellationRequest.Id", cancellationRequest.Id));
            tokens.Add(new Token("CancellationRequest.OrderId", order.Id));
            tokens.Add(new Token("CancellationRequest.Reason", cancellationRequest.ReasonForCancellation));
            tokens.Add(new Token("CancellationRequest.CustomerComment",
                _htmlFormatter.FormatText(cancellationRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("CancellationRequest.StaffNotes",
                _htmlFormatter.FormatText(cancellationRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("CancellationRequest.Status", await _localizationService.GetLocalizedEnumAsync(cancellationRequest.RequestStatus)));

        }

        public async Task<IList<int>> SendNewCancellationRequestStoreOwnerNotificationAsync(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(PluginDefaults.NEW_CANCELLATION_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(cancellationRequest.CustomerId)
                           ?? throw new Exception($"Customer with Id {cancellationRequest.CustomerId} does not existed");

            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await AddCancellationRequestTokensAsync(commonTokens, cancellationRequest, order);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await _workflowMessageService
                    .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task<IList<int>> SendNewCancellationRequestCustomerNotificationAsync(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(PluginDefaults.NEW_CANCELLATION_REQUEST_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(cancellationRequest.CustomerId)
                           ?? throw new Exception($"Customer with Id {cancellationRequest.CustomerId} does not existed");

            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await AddCancellationRequestTokensAsync(commonTokens, cancellationRequest, order);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var customerIsGuest = await _customerService.IsGuestAsync(customer);
                var toEmail = customerIsGuest
                    ? billingAddress.Email
                    : customer.Email;
                var toName = customerIsGuest
                    ? billingAddress.FirstName
                    : await _customerService.GetCustomerFullNameAsync(customer);

                return await _workflowMessageService
                    .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task<IList<int>> SendCancellationRequestCustomerNotificationAsync(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId, string template)
        {
            if (cancellationRequest == null)
                throw new ArgumentNullException(nameof(cancellationRequest));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(template, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(cancellationRequest.CustomerId)
                           ?? throw new Exception($"Customer with Id {cancellationRequest.CustomerId} does not existed");

            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await AddCancellationRequestTokensAsync(commonTokens, cancellationRequest, order);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var customerIsGuest = await _customerService.IsGuestAsync(customer);
                var toEmail = customerIsGuest
                    ? billingAddress.Email
                    : customer.Email;
                var toName = customerIsGuest
                    ? billingAddress.FirstName
                    : await _customerService.GetCustomerFullNameAsync(customer);

                return await _workflowMessageService
                    .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task VoidCancelledOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                throw new NopException("You can't void this order");

            if (order.PaymentStatus == PaymentStatus.Authorized)
            {
                if (await _paymentService.SupportVoidAsync(order.PaymentMethodSystemName))
                {
                    var request = new VoidPaymentRequest();
                    try
                    {
                        request.Order = order;
                        var result = await _paymentService.VoidAsync(request);

                        if (result.Success)
                        {
                            order.PaymentStatus = result.NewPaymentStatus;
                            await _orderService.UpdateOrderAsync(order);

                            await _orderService.InsertOrderNoteAsync(new OrderNote
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
                        await _logger.ErrorAsync($"Error: {ex.Message}", ex);
                    }
                }
                else
                {
                    order.PaymentStatusId = (int)PaymentStatus.Voided;
                    await _orderService.UpdateOrderAsync(order);

                    await _orderService.InsertOrderNoteAsync(new OrderNote
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
                await _orderService.UpdateOrderAsync(order);

                await _orderService.InsertOrderNoteAsync(new OrderNote
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