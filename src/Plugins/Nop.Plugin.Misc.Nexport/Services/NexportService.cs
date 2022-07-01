using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public partial class NexportService
    {
        #region Fields

        private readonly NexportApiService _nexportApiService;

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly NexportSettings _nexportSettings;

        private readonly IAddressService _addressService;
        private readonly IAclService _aclService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<NexportProductMapping> _nexportProductMappingRepository;
        private readonly IRepository<NexportProductGroupMembershipMapping> _nexportProductGroupMembershipMappingRepository;
        private readonly IRepository<NexportOrderProcessingQueueItem> _nexportOrderProcessingQueueRepository;
        private readonly IRepository<NexportOrderInvoiceItem> _nexportOrderInvoiceItemRepository;
        private readonly IRepository<NexportOrderInvoiceRedemptionQueueItem> _nexportOrderInvoiceRedemptionQueueRepository;
        private readonly IRepository<NexportUserMapping> _nexportUserMappingRepository;
        private readonly IRepository<NexportSupplementalInfoQuestion> _nexportSupplementalInfoQuestionRepository;
        private readonly IRepository<NexportSupplementalInfoOption> _nexportSupplementalInfoOptionRepository;
        private readonly IRepository<NexportSupplementalInfoQuestionMapping> _nexportSupplementalInfoQuestionMappingRepository;
        private readonly IRepository<NexportSupplementalInfoAnswer> _nexportSupplementalInfoAnswerRepository;
        private readonly IRepository<NexportSupplementalInfoOptionGroupAssociation> _nexportSupplementalInfoOptionGroupAssociationRepository;
        private readonly IRepository<NexportSupplementalInfoAnswerMembership> _nexportSupplementalInfoAnswerMembershipRepository;
        private readonly IRepository<NexportRequiredSupplementalInfo> _nexportRequiredSupplementalInfoRepository;
        private readonly IRepository<NexportSupplementalInfoAnswerProcessingQueueItem> _nexportSupplementalInfoAnswerProcessingQueueRepository;
        private readonly IRepository<NexportGroupMembershipRemovalQueueItem> _nexportGroupMembershipRemovalQueueRepository;
        private readonly IRepository<NexportRegistrationField> _nexportRegistrationFieldRepository;
        private readonly IRepository<NexportRegistrationFieldOption> _nexportRegistrationFieldOptionRepository;
        private readonly IRepository<NexportRegistrationFieldCategory> _nexportRegistrationFieldCategoryRepository;
        private readonly IRepository<NexportRegistrationFieldStoreMapping> _nexportRegistrationFieldStoreMappingRepository;
        private readonly IRepository<NexportRegistrationFieldAnswer> _nexportRegistrationFieldAnswerRepository;
        private readonly IRepository<NexportRegistrationFieldSynchronizationQueueItem> _nexportRegistrationFieldSynchronizationQueueRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IMessageTokenProvider _messageTokenProvider;

        private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomerRenderPluginManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public NexportService(
            NexportApiService nexportApiService,
            EmailAccountSettings emailAccountSettings,
            NexportSettings nexportSettings,
            IAddressService addressService,
            IAclService aclService,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IProductService productService,
            IRepository<AclRecord> aclRepository,
            IRepository<Product> productRepository,
            IRepository<NexportProductMapping> nexportProductMappingRepository,
            IRepository<NexportProductGroupMembershipMapping> nexportProductGroupMembershipMappingRepository,
            IRepository<NexportOrderProcessingQueueItem> nexportOrderProcessingQueueRepository,
            IRepository<NexportOrderInvoiceItem> nexportOrderInvoiceItemRepository,
            IRepository<NexportOrderInvoiceRedemptionQueueItem> nexportOrderInvoiceRedemptionQueueRepository,
            IRepository<NexportUserMapping> nexportUserMappingRepository,
            IRepository<NexportSupplementalInfoQuestion> nexportSupplementalInfoQuestionRepository,
            IRepository<NexportSupplementalInfoOption> nexportSupplementalInfoOptionRepository,
            IRepository<NexportSupplementalInfoQuestionMapping> nexportSupplementalInfoQuestionMappingRepository,
            IRepository<NexportSupplementalInfoAnswer> nexportSupplementalInfoAnswerRepository,
            IRepository<NexportSupplementalInfoOptionGroupAssociation> nexportSupplementalInfoOptionGroupAssociationRepository,
            IRepository<NexportSupplementalInfoAnswerMembership> nexportSupplementalInfoAnswerMembershipRepository,
            IRepository<NexportRequiredSupplementalInfo> nexportRequiredSupplementalInfoRepository,
            IRepository<NexportSupplementalInfoAnswerProcessingQueueItem> nexportSupplementalInfoAnswerProcessingQueueRepository,
            IRepository<NexportGroupMembershipRemovalQueueItem> nexportGroupMembershipRemovalQueueRepository,
            IRepository<NexportRegistrationField> nexportRegistrationFieldRepository,
            IRepository<NexportRegistrationFieldOption> nexportRegistrationFieldOptionRepository,
            IRepository<NexportRegistrationFieldCategory> nexportRegistrationFieldCategoryRepository,
            IRepository<NexportRegistrationFieldStoreMapping> nexportRegistrationFieldStoreMappingRepository,
            IRepository<NexportRegistrationFieldAnswer> nexportRegistrationFieldAnswerRepository,
            IRepository<NexportRegistrationFieldSynchronizationQueueItem> nexportRegistrationFieldSynchronizationQueueRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IOrderService orderService,
            ICategoryService categoryService,
            ISettingService settingService,
            IStoreService storeService,
            IShoppingCartService shoppingCartService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IGenericAttributeService genericAttributeService,
            INotificationService notificationService,
            ILanguageService languageService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IWorkflowMessageService workflowMessageService,
            ILocalizationService localizationService,
            IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomerRenderPluginManager,
            IDateTimeHelper dateTimeHelper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger logger)
        {
            _nexportApiService = nexportApiService;
            _emailAccountSettings = emailAccountSettings;
            _nexportSettings = nexportSettings;
            _addressService = addressService;
            _aclService = aclService;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _productService = productService;
            _aclRepository = aclRepository;
            _productRepository = productRepository;
            _nexportProductMappingRepository = nexportProductMappingRepository;
            _nexportProductGroupMembershipMappingRepository = nexportProductGroupMembershipMappingRepository;
            _nexportOrderProcessingQueueRepository = nexportOrderProcessingQueueRepository;
            _nexportOrderInvoiceItemRepository = nexportOrderInvoiceItemRepository;
            _nexportOrderInvoiceRedemptionQueueRepository = nexportOrderInvoiceRedemptionQueueRepository;
            _nexportUserMappingRepository = nexportUserMappingRepository;
            _nexportSupplementalInfoQuestionRepository = nexportSupplementalInfoQuestionRepository;
            _nexportSupplementalInfoOptionRepository = nexportSupplementalInfoOptionRepository;
            _nexportSupplementalInfoQuestionMappingRepository = nexportSupplementalInfoQuestionMappingRepository;
            _nexportSupplementalInfoAnswerRepository = nexportSupplementalInfoAnswerRepository;
            _nexportSupplementalInfoOptionGroupAssociationRepository = nexportSupplementalInfoOptionGroupAssociationRepository;
            _nexportSupplementalInfoAnswerMembershipRepository = nexportSupplementalInfoAnswerMembershipRepository;
            _nexportRequiredSupplementalInfoRepository = nexportRequiredSupplementalInfoRepository;
            _nexportSupplementalInfoAnswerProcessingQueueRepository = nexportSupplementalInfoAnswerProcessingQueueRepository;
            _nexportGroupMembershipRemovalQueueRepository = nexportGroupMembershipRemovalQueueRepository;
            _nexportRegistrationFieldRepository = nexportRegistrationFieldRepository;
            _nexportRegistrationFieldOptionRepository = nexportRegistrationFieldOptionRepository;
            _nexportRegistrationFieldCategoryRepository = nexportRegistrationFieldCategoryRepository;
            _nexportRegistrationFieldStoreMappingRepository = nexportRegistrationFieldStoreMappingRepository;
            _nexportRegistrationFieldAnswerRepository = nexportRegistrationFieldAnswerRepository;
            _nexportRegistrationFieldSynchronizationQueueRepository = nexportRegistrationFieldSynchronizationQueueRepository;
            _storeMappingRepository = storeMappingRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _orderService = orderService;
            _categoryService = categoryService;
            _settingService = settingService;
            _storeService = storeService;
            _shoppingCartService = shoppingCartService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _notificationService = notificationService;
            _languageService = languageService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _messageTokenProvider = messageTokenProvider;
            _workflowMessageService = workflowMessageService;
            _localizationService = localizationService;
            _registrationFieldCustomerRenderPluginManager = registrationFieldCustomerRenderPluginManager;
            _dateTimeHelper = dateTimeHelper;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _workContext = workContext;
            _storeContext = storeContext;
            _logger = logger;
        }

        #endregion

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
            messageTemplates = await messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToListAsync();

            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>EmailAccount</returns>
        private async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate,
                mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ??
                                await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }

        #endregion

        #region Workflow Message Services

        #region Message Token Builders

        private async Task AddNexportOrderApprovalTokensAsync(IList<Token> tokens, Order order)
        {
            tokens.Add(new Token("NexportOrderApproval.OrderId", order.Id));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.Action("Edit", "Order", new { id = order.Id }));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            var orderUrl = Uri.EscapeDataString(WebUtility.UrlDecode($"{store.Url}{AreaNames.Admin}/{url}"));

            tokens.Add(new Token("NexportOrderApproval.AdminViewOrderUrl", orderUrl, true));
        }

        #endregion

        public async Task<IList<int>> SendNewNexportOrderApprovalStoreOwnerNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId);
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(
                NexportDefaults.NEXPORT_ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await AddNexportOrderApprovalTokensAsync(commonTokens, order);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount,
                    languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

        public async Task GenerateNewNexportTokenAsync(ConfigurationModel model)
        {
            try
            {
                DateTime? tokenExpiration = null;

                if (!model.TokenNeverExpired)
                {
                    tokenExpiration = model.CustomTokenExpirationDate ?? DateTime.UtcNow.AddDays(30);
                }

                var response = _nexportApiService.AuthenticateNexportApi(model.Url, model.Username, model.Password, tokenExpiration).Response;

                _nexportSettings.AuthenticationToken = response.AccessToken.ToString();
                _nexportSettings.Url = model.Url;
                _nexportSettings.UtcExpirationDate = tokenExpiration;

                await _settingService.SaveSettingAsync(_nexportSettings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error occurred during Web API call Authenticate for username {model.Username}";
                await _logger.ErrorAsync($"{errorMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<AuthenticationTokenResponse>(exception.ErrorContent.ToString()!);
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        [CanBeNull]
        public async Task<GetUserResponse> AuthenticateUserAsync(string username, string password)
        {
            GetUserResponse result;
            try
            {
                var response = _nexportApiService.AuthenticateNexportUser(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    username, password);

                result = response.Response;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error occurred during Web API call AuthenticateUser for username {username}";
                await _logger.ErrorAsync($"{errorMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetUserResponse>(exception.ErrorContent.ToString()!);
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetUserResponse> ValidateUserAsync(string username)
        {
            try
            {
                var response = _nexportApiService.GetNexportUserByLogin(_nexportSettings.Url, _nexportSettings.AuthenticationToken, username);

                if (response.StatusCode == 409)
                    return null;

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message = $"Nexport plugin access does not have permission to look up the user with login {username}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get a user with login {username}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error occurred during Web API call GetUser for username {username}";
                await _logger.ErrorAsync($"{errorMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetUserResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        [CanBeNull]
        public async Task<CreateUserResponse> CreateNexportUserAsync(string login, string password,
            string firstName, string lastName, string email, Guid ownerOrgId, UserContactInfoRequest contactInfo = null)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentNullException(nameof(login), "Login cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be empty");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentNullException(nameof(firstName), "First name cannot be empty");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentNullException(nameof(lastName), "Last name cannot be empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be empty");

            try
            {
                var response = _nexportApiService.CreateNexportUser(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    login, password, firstName, lastName, email, ownerOrgId, contactInfo);

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message =
                        $"Nexport plugin access does not have permission to create new user with login {login} in the organization {ownerOrgId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 409)
                {
                    var message = response.Response.ApiErrorEntity.ErrorMessage.Contains("exists") ?
                        $"Cannot create new user with {login} in the organization {ownerOrgId} because of duplication" :
                        $"Cannot find any organization with the Id of {ownerOrgId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message =
                        $"Validation exception occurred when trying to create new user with login {login} in organization {ownerOrgId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error occurred during Web API call CreateUser for login {login} (Name: {firstName} {lastName})";
                await _logger.ErrorAsync($"{errorMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<CreateUserResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        [CanBeNull]
        public async Task<GetUserResponse> GetNexportUserAsync(Guid userId)
        {
            try
            {
                var response = _nexportApiService.GetNexportUserByUserId(_nexportSettings.Url, _nexportSettings.AuthenticationToken, userId);

                if (response.StatusCode == 409)
                    return null;

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message = $"Nexport plugin access does not have permission to look up the user with Id {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get a user with Id {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during GetUser api call for Nexport user {userId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetUserResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        [CanBeNull]
        public async Task<UserContactInfoResponse> GetNexportUserContactInfoAsync(Guid userId)
        {
            try
            {
                var response = _nexportApiService.GetNexportUserContactInfo(_nexportSettings.Url, _nexportSettings.AuthenticationToken, userId);

                if (response.StatusCode == 409)
                    return null;

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message = $"Nexport plugin access does not have permission to look up the contact info for user with Id {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get the contact info for user with Id {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during GetUserContactInformation api call for Nexport user {userId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<UserContactInfoResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        [CanBeNull]
        public async Task<EditUserResponse> UpdateNexportUserContactInfoAsync(Guid userId, UserContactInfoRequest updatedInfo)
        {
            if (updatedInfo == null)
                throw new ArgumentNullException(nameof(updatedInfo), "Updated information be empty");

            try
            {
                var response = _nexportApiService.EditNexportUserContactInfo(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    userId, updatedInfo);

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message = $"Nexport plugin access does not have permission to edit user {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 409)
                {
                    var message = $"Cannot find user with the Id of {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message =
                        $"Validation exception occurred when trying to edit user with the Id {userId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during EditUser api call for Nexport user {userId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<EditUserResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<List<DirectoryResponseItem>> SearchNexportDirectoryAsync(string searchTerm, int? page = null)
        {
            try
            {
                if (_nexportSettings.RootOrganizationId.HasValue)
                {
                    var response = _nexportApiService.SearchNexportDirectory(_nexportSettings.Url,
                        _nexportSettings.AuthenticationToken,
                        _nexportSettings.RootOrganizationId.Value, searchTerm, page);

                    return response.DirectoryList.ToList();
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during SearchDirectory api call with the given search term {searchTerm}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<DirectoryResponseItem>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<IList<OrganizationResponseItem>> FindAllOrganizationsAsync(Guid baseOrgId)
        {
            var items = new List<OrganizationResponseItem>();

            try
            {
                var page = 1;
                int remainderItemsCount;
                do
                {
                    var result = _nexportApiService.GetNexportOrganizations(_nexportSettings.Url,
                        _nexportSettings.AuthenticationToken, baseOrgId, page);
                    items.AddRange(result.OrganizationList);

                    remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
                    page++;
                } while (remainderItemsCount > -1);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetOrganizations with the parameter: org_id - {baseOrgId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<OrganizationResponseItem>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return items;
        }

        public async Task<IList<OrganizationResponseItem>> FindAllOrganizationsUnderRootOrganizationAsync()
        {
            if (!_nexportSettings.RootOrganizationId.HasValue)
                throw new NullReferenceException("Root organization has not been set");

            return await FindAllOrganizationsAsync(_nexportSettings.RootOrganizationId.Value);
        }

        [CanBeNull]
        public async Task<OrganizationResponseItem> GetOrganizationDetailsAsync(Guid orgId)
        {
            var availableOrganizations = await FindAllOrganizationsAsync(orgId);
            var result = availableOrganizations.SingleOrDefault(s => s.OrgId == orgId);

            return result;
        }

        public async Task<IList<SubscriptionResponse>> FindAllSubscriptionsAsync(Guid userId)
        {
            var items = new List<SubscriptionResponse>();

            try
            {
                var page = 1;
                int remainderItemsCount;
                do
                {
                    var result = _nexportApiService.GetNexportSubscriptions(_nexportSettings.Url,
                        _nexportSettings.AuthenticationToken, userId, page);
                    items.AddRange(result.Subscriptions);

                    remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
                    page++;
                } while (remainderItemsCount > -1);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetSubscriptions for user {userId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SubscriptionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return items;
        }

        [CanBeNull]
        public async Task<SetCustomProfileFieldValuesResponse> SetCustomProfileFieldValuesAsync(Guid subscriberId, Dictionary<string, string> profileFields)
        {
            SetCustomProfileFieldValuesResponse result;

            try
            {
                result = _nexportApiService.SetNexportCustomerProfileFieldValues(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, subscriberId, profileFields);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call SetCustomProfileFieldValues for subscriber {subscriberId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SetCustomEnrollmentFieldValuesResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        public async Task<IPagedList<CatalogResponseItem>> FindAllCatalogsAsync(Guid? orgId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var items = new List<CatalogResponseItem>();

            if (orgId.HasValue)
            {
                try
                {
                    var page = 1;
                    int remainderItemsCount;
                    do
                    {
                        var result = _nexportApiService.GetNexportCatalogs(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, orgId.Value, page);

                        items.AddRange(result.CatalogList);

                        remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
                        page++;
                    } while (remainderItemsCount > -1);
                }
                catch (Exception ex)
                {
                    var errMsg = $"Error occurred during Web API call GetCatalogs for organization {orgId}";
                    await _logger.ErrorAsync($"{errMsg}", ex);

                    if (ex is ApiException exception)
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ApiResponseBase>(exception.ErrorContent.ToString());
                        if (errorResponse != null)
                        {
                            throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                        }
                    }

                    throw;
                }
            }

            var pagedItems = new PagedList<CatalogResponseItem>(items, pageIndex, pageSize);

            return pagedItems;
        }

        [CanBeNull]
        public async Task<CatalogResponseItem> GetCatalogDetailsAsync(Guid catalogId)
        {
            CatalogResponseItem result;

            try
            {
                result = _nexportApiService.GetNexportCatalogDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetCatalog for catalog {catalogId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseBase>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetDescriptionResponse> GetCatalogDescriptionAsync(Guid catalogId)
        {
            GetDescriptionResponse result;

            try
            {
                result = _nexportApiService.GetNexportCatalogDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetCatalogDescription for catalog {catalogId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetDescriptionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetCatalogCreditHoursResponse> GetCatalogCreditHoursAsync(Guid catalogId)
        {
            GetCatalogCreditHoursResponse result;

            try
            {
                result = _nexportApiService.GetNexportCatalogCreditHours(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetCatalogCreditHours for catalog {catalogId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetCatalogCreditHoursResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        public async Task<IPagedList<GetSyllabiResponseItem>> FindAllSyllabusesAsync(Guid? catalogId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var items = new List<GetSyllabiResponseItem>();

            if (catalogId.HasValue)
            {
                try
                {
                    var page = 1;
                    int remainderItemsCount;
                    do
                    {
                        var result = _nexportApiService.GetNexportSyllabuses(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, catalogId.Value, page);

                        items.AddRange(result.SyllabusList);

                        remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
                        page++;
                    } while (remainderItemsCount > -1);
                }
                catch (Exception ex)
                {
                    var errMsg = $"Error occurred during Web API call GetCatalogSyllabi for catalog {catalogId}";
                    await _logger.ErrorAsync($"{errMsg}", ex);

                    if (ex is ApiException exception)
                    {
                        var errorResponse = JsonConvert.DeserializeObject<GetSyllabiResponseItem>(exception.ErrorContent.ToString());
                        if (errorResponse != null)
                        {
                            throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                        }
                    }

                    throw;
                }
            }

            var pagedItems = new PagedList<GetSyllabiResponseItem>(items, pageIndex, pageSize);

            return pagedItems;
        }

        [CanBeNull]
        public async Task<SectionResponse> GetSectionDetailsAsync(Guid sectionId)
        {
            SectionResponse result;

            try
            {
                result = _nexportApiService.GetNexportSectionDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetSection for section {sectionId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SectionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetDescriptionResponse> GetSectionDescriptionAsync(Guid sectionId)
        {
            GetDescriptionResponse result;

            try
            {
                result = _nexportApiService.GetNexportSectionDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetSectionDescription for section {sectionId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetSectionsResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetObjectivesResponse> GetSectionObjectivesAsync(Guid sectionId)
        {
            GetObjectivesResponse result;

            try
            {
                result = _nexportApiService.GetNexportSectionObjectives(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetSectionObjectives for section {sectionId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetObjectivesResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<SectionEnrollmentsResponse> GetSectionEnrollmentDetailsAsync(Guid orgId, Guid userId, Guid syllabusId)
        {
            SectionEnrollmentsResponse result;

            try
            {
                result = _nexportApiService.GetNexportSectionEnrollment(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    orgId, userId, syllabusId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetSectionEnrollments for section {syllabusId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SectionEnrollmentsResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<TrainingPlanResponse> GetTrainingPlanDetailsAsync(Guid trainingPlanId)
        {
            TrainingPlanResponse result;

            try
            {
                result = _nexportApiService.GetNexportTrainingPlanDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, trainingPlanId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetTrainingPlan for training plan {trainingPlanId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<TrainingPlanResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetDescriptionResponse> GetTrainingPlanDescriptionAsync(Guid trainingPlanId)
        {
            GetDescriptionResponse result;

            try
            {
                result = _nexportApiService.GetNexportTrainingPlanDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, trainingPlanId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetTrainingPlanDescription for training plan {trainingPlanId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetDescriptionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<TrainingPlanEnrollmentsResponse> GetTrainingPlanEnrollmentDetailsAsync(Guid orgId, Guid userId, Guid trainingPlanId)
        {
            TrainingPlanEnrollmentsResponse result;

            try
            {
                result = _nexportApiService.GetNexportTrainingPlanEnrollment(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    orgId, userId, trainingPlanId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during Web API call GetTrainingPlanEnrollments for training plan {trainingPlanId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<TrainingPlanEnrollmentsResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return result;
        }

        [CanBeNull]
        public async Task<GetInvoiceResponse> GetNexportInvoiceAsync(Guid invoiceId)
        {
            try
            {
                var response = _nexportApiService.GetNexportInvoice(_nexportSettings.Url, _nexportSettings.AuthenticationToken, invoiceId);

                if (response.StatusCode == 409)
                    return null;

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message = $"Nexport plugin access does not have permission to look up the invoice {invoiceId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get the invoice {invoiceId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during GetNexportInvoice api call for the invoice {invoiceId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<GetInvoiceResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<Guid> BeginNexportOrderInvoiceTransactionAsync(Guid orgId, Guid purchasingAgentId)
        {
            try
            {
                var beginOrderResult = _nexportApiService.BeginNexportInvoiceTransaction(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, orgId, purchasingAgentId);

                return beginOrderResult.InvoiceId;
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Error occurred during BeginInvoiceTransaction api call with the parameters: org_id - {orgId}, purchasing_agent_id - {purchasingAgentId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<BeginInvoiceTransactionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        [CanBeNull]
        public async Task<Guid?> AddItemToNexportOrderInvoiceAsync(Guid invoiceId, Guid nexportProductId,
            Enums.ProductTypeEnum productType, decimal productCost,
            Guid subscriptionOrgId, IList<Guid> groupMembershipIds = null,
            DateTime? accessExpirationDate = null, string accessExpirationTimeLimit = null, string note = null)
        {
            AddInvoiceItemResponse addInvoiceItemResult;

            try
            {
                addInvoiceItemResult = _nexportApiService.AddNexportInvoiceItem(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId, nexportProductId,
                    productType, subscriptionOrgId, groupMembershipIds,
                    productCost, note, accessExpirationDate, accessExpirationTimeLimit);
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Error occurred during AddInvoiceItem api call with the parameters: invoice_id - {invoiceId}, product_id - {nexportProductId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<AddInvoiceItemResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return addInvoiceItemResult?.InvoiceItemId;
        }

        public async Task CommitNexportOrderInvoiceTransactionAsync(Guid invoiceId)
        {
            try
            {
                _nexportApiService.CommitNexportInvoiceTransaction(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId);
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Error occurred during CommitInvoiceTransaction api call for the invoice {invoiceId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<CommitInvoiceResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        public async Task AddPaymentToNexportOrderInvoiceAsync(Guid invoiceId, decimal totalCost, Guid payeeId, int nopOrderId,
            DateTime dueDate)
        {
            if (!_nexportSettings.MerchantAccountId.HasValue)
                throw new Exception("Merchant account is empty. Cannot processing payment without a merchant account.");

            try
            {
                _nexportApiService.AddNexportInvoicePayment(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId, totalCost, _nexportSettings.MerchantAccountId.Value,
                    payeeId, InvoicePaymentRequest.PaymentProcessorEnum.NopCommercePlugin,
                    nopOrderId.ToString(), dueDate, note: "Payment for NopCommerce order");
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Error occurred during AddInvoicePayment api call with the parameters: invoice_id - {invoiceId}, payee_id - {payeeId}, payment_processor_transaction_id: {nopOrderId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<AddInvoicePaymentResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        public async Task RedeemNexportInvoiceItemAsync(NexportOrderInvoiceItem invoiceItem, Guid redeemingUserId,
            RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction =
                RedeemInvoiceItemRequest.RedemptionActionTypeEnum.NormalRedemption)
        {
            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            if (redeemingUserId == Guid.Empty)
                throw new ArgumentException("Redeeming User Id cannot be empty identifier", nameof(redeemingUserId));

            try
            {
                var redeemInvoiceResult = _nexportApiService.RedeemNexportInvoice(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceItem.InvoiceItemId, redeemingUserId, redemptionAction);

                if (redeemInvoiceResult.ApiErrorEntity.ErrorCode != ApiErrorEntity.ErrorCodeEnum.NoError)
                    throw new ApiException((int)redeemInvoiceResult.ApiErrorEntity.ErrorCode,
                        redeemInvoiceResult.ApiErrorEntity.ErrorMessage);

                invoiceItem.RedeemingUserId = redeemingUserId;
                invoiceItem.UtcDateRedemption = redeemInvoiceResult.UtcRedemptionDate;
                invoiceItem.RequireManualApproval = null;

                if (redeemInvoiceResult.RedemptionEnrollmentId != null)
                {
                    invoiceItem.RedemptionEnrollmentId = redeemInvoiceResult.RedemptionEnrollmentId;
                }

                UpdateNexportOrderInvoiceItem(invoiceItem);
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Error occurred during RedeemInvoiceItem api call with the parameters: invoice_item_id - {invoiceItem.InvoiceItemId}, redeeming_user_id - {redeemingUserId}, redemption_action_type - {redemptionAction}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<InvoiceRedemptionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        [CanBeNull]
        public async Task<InvoiceRedemptionResponse> GetNexportInvoiceRedemptionAsync(Guid invoiceItemId)
        {
            if (invoiceItemId == Guid.Empty)
                throw new ArgumentException("Invoice item Id cannot be an empty GUID");

            try
            {
                var response = _nexportApiService.GetNexportInvoiceRedemption(_nexportSettings.Url, _nexportSettings.AuthenticationToken, invoiceItemId);

                if (response.StatusCode == 409)
                    return null;

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message = $"Nexport plugin access does not have permission to look up the invoice redemption for invoice item {invoiceItemId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get the invoice redemption for invoice item {invoiceItemId}";
                    await _logger.ErrorAsync(message);

                    throw new ApiException(response.StatusCode, message);
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during GetInvoiceRedemption api call for the invoice item {invoiceItemId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<InvoiceRedemptionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<string> SignInNexportAsync(NexportOrderInvoiceItem invoiceItem)
        {
            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            InvoiceRedemptionResponse redemption;

            try
            {
                redemption = await GetNexportInvoiceRedemptionAsync(invoiceItem.InvoiceItemId);

            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Error occurred during GetInvoiceRedemption api call with the invoice item {invoiceItem.InvoiceItemId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<InvoiceRedemptionResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            try
            {
                if (redemption != null && redemption.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                {
                    SsoResponse signInResult = null;
                    if (invoiceItem.RedemptionEnrollmentId == null)
                    {
                        if (redemption.RedemptionUserId != null)
                        {
                            signInResult = _nexportApiService.NexportSingleSignOn(_nexportSettings.Url,
                                _nexportSettings.AuthenticationToken, redemption.OrganizationId,
                                redemption.RedemptionUserId.Value, (await _storeContext.GetCurrentStoreAsync()).Url);
                        }
                    }
                    else
                    {
                        if (redemption.RedemptionEnrollmentId != null)
                        {
                            signInResult = _nexportApiService.NexportClassroomSingleSignOn(_nexportSettings.Url,
                                _nexportSettings.AuthenticationToken, redemption.RedemptionEnrollmentId.Value,
                                (await _storeContext.GetCurrentStoreAsync()).Url);
                        }
                    }

                    if (signInResult != null &&
                        signInResult.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                    {
                        return signInResult.Url;
                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = "Error occurred during SingleSignOn api call";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SsoResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<string> SignInNexportAsync(Guid orgId, Guid userId)
        {
            if (orgId == Guid.Empty)
                throw new ArgumentException("Organization Id cannot be null", nameof(orgId));

            try
            {
                var response = _nexportApiService.NexportSingleSignOn(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken,
                    orgId, userId, (await _storeContext.GetCurrentStoreAsync()).Url);

                if (response.ApiErrorEntity.ErrorCode == 0)
                    return response.Url;
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during SignIn api call for user {userId} in organization {orgId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SsoResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<string> SignInNexportClassroomAsync(Guid enrollmentId)
        {
            try
            {
                var response = _nexportApiService.NexportClassroomSingleSignOn(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, enrollmentId, (await _storeContext.GetCurrentStoreAsync()).Url);

                if (response.ApiErrorEntity.ErrorCode == 0)
                    return response.Url;
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during SignIn api call for enrollment {enrollmentId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<SsoResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode, errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }

            return null;
        }

        public async Task<List<MemberShipInfo>> AddNexportMembershipsAsync(Guid userId, IList<Guid> groupIds)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User Id cannot be an empty identifier", nameof(userId));

            try
            {
                var createMembershipResult = _nexportApiService.CreateNexportMemberships(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, userId, groupIds);

                if (createMembershipResult.ApiErrorEntity.ErrorCode != ApiErrorEntity.ErrorCodeEnum.NoError)
                    throw new ApiException((int)createMembershipResult.ApiErrorEntity.ErrorCode,
                        createMembershipResult.ApiErrorEntity.ErrorMessage);

                return createMembershipResult.Memberships;
            }
            catch (Exception ex)
            {
                var errMsg = $"Error occurred during CreateMembership api call for user {userId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<CreateMembershipResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode,
                            errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        public async Task<List<RemovedMembershipInfo>> RemoveNexportMembershipsAsync(IList<Guid> membershipIds)
        {
            try
            {
                var removeMembershipResult = _nexportApiService.RemoveNexportMemberships(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, membershipIds);

                if (removeMembershipResult.ApiErrorEntity.ErrorCode != ApiErrorEntity.ErrorCodeEnum.NoError)
                    throw new ApiException((int)removeMembershipResult.ApiErrorEntity.ErrorCode,
                        removeMembershipResult.ApiErrorEntity.ErrorMessage);

                return removeMembershipResult.Memberships;
            }
            catch (Exception ex)
            {
                var errMsg = "Error occurred during RemoveMembership api call";
                await _logger.ErrorAsync($"{errMsg}", ex);

                if (ex is ApiException exception)
                {
                    var errorResponse = JsonConvert.DeserializeObject<RemoveMembershipResponse>(exception.ErrorContent.ToString());
                    if (errorResponse != null)
                    {
                        throw new ApiException((int)errorResponse.ApiErrorEntity.ErrorCode,
                            errorResponse.ApiErrorEntity.ErrorMessage);
                    }
                }

                throw;
            }
        }

        public async Task<List<NexportOrganizationModel>> FindNexportRedemptionOrganizationsByCustomerIdAsync(int customerId)
        {
            var orders = await _orderService.SearchOrdersAsync((await _storeContext.GetCurrentStoreAsync()).Id, customerId: customerId);
            var organizationModelList = new List<NexportOrganizationModel>();

            foreach (var order in orders)
            {
                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                foreach (var orderItem in orderItems)
                {
                    var orderInvoiceItem = await FindNexportOrderInvoiceItem(order.Id, orderItem.Id);
                    if (orderInvoiceItem?.UtcDateRedemption != null)
                    {
                        var invoiceRedemption = await GetNexportInvoiceRedemptionAsync(orderInvoiceItem.InvoiceItemId)!;
                        if (invoiceRedemption != null && invoiceRedemption?.ApiErrorEntity.ErrorCode == 0)
                        {
                            if (!organizationModelList.Exists(i => i.OrgId == invoiceRedemption.OrganizationId))
                            {
                                var availableOrganizations = await FindAllOrganizationsAsync(invoiceRedemption.OrganizationId);

                                var org = availableOrganizations.FirstOrDefault(o =>
                                        o.OrgId == invoiceRedemption.OrganizationId);

                                if (org != null)
                                {
                                    organizationModelList.Add(new NexportOrganizationModel
                                    {
                                        OrgId = org.OrgId,
                                        OrgName = org.Name,
                                        OrgShortName = org.ShortName
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return organizationModelList;
        }

        public async Task SyncNexportProductAsync(int mappingId, [CanBeNull] Product product = null)
        {
            if (mappingId == 0)
                throw new ArgumentException("Mapping Id is invalid!", nameof(mappingId));

            var productMapping = await GetProductMappingById(mappingId);

            if (productMapping != null)
            {
                product ??= await _productService.GetProductByIdAsync(productMapping.NopProductId);

                if (product != null && productMapping.NopProductId == product.Id)
                {
                    switch (productMapping.Type)
                    {
                        case NexportProductTypeEnum.Catalog:
                            //var catalogDetails = GetCatalogDetails(productMapping.NexportCatalogId);
                            //var catalogDescription = GetCatalogDescription(productMapping.NexportCatalogId);
                            var catalogCreditHours = await GetCatalogCreditHoursAsync(productMapping.NexportCatalogId);

                            //product.Name = catalogDetails.Name;
                            //product.FullDescription = catalogDescription.Description;
                            productMapping.CreditHours = catalogCreditHours?.CreditHours;

                            break;

                        case NexportProductTypeEnum.Section:
                            if (!productMapping.NexportSyllabusId.HasValue)
                            {
                                throw new Exception("Section Id cannot be null");
                            }

                            var sectionId = productMapping.NexportSyllabusId.Value;

                            var sectionDetails = await GetSectionDetailsAsync(sectionId);
                            //var sectionDescription = GetSectionDescription(sectionId);
                            //var sectionObjective = GetSectionObjectives(sectionId);

                            //product.Name = sectionDetails.Title;
                            //product.FullDescription = sectionDescription.Description;
                            //product.ShortDescription = sectionObjective.Objectives;
                            //product.Sku = sectionDetails.SectionNumber;
                            //product.AvailableStartDateTimeUtc = sectionDetails.EnrollmentStart;
                            //product.AvailableEndDateTimeUtc = sectionDetails.EnrollmentEnd;

                            productMapping.CreditHours = sectionDetails?.CreditHours;
                            productMapping.SectionCeus = sectionDetails?.SectionCeus;

                            break;

                        case NexportProductTypeEnum.TrainingPlan:
                            if (!productMapping.NexportSyllabusId.HasValue)
                            {
                                throw new Exception("Training Plan Id cannot be null");
                            }

                            var trainingPlanId = productMapping.NexportSyllabusId.Value;

                            var trainingPlanDetails = await GetTrainingPlanDetailsAsync(trainingPlanId);
                            //var trainingPlanDescription = GetTrainingPlanDescription(trainingPlanId);

                            //product.Name = trainingPlanDetails.Title;
                            //product.FullDescription = trainingPlanDescription.Description;
                            //product.AvailableStartDateTimeUtc = trainingPlanDetails.EnrollmentStart;
                            //product.AvailableEndDateTimeUtc = trainingPlanDetails.EnrollmentEnd;

                            productMapping.CreditHours = trainingPlanDetails?.CreditHours;

                            break;

                        default:
                            goto case NexportProductTypeEnum.Catalog;
                    }

                    productMapping.IsSynchronized = true;
                    productMapping.UtcLastSynchronizationDate = DateTime.UtcNow;

                    await UpdateNexportProductMapping(productMapping);

                    _logger.DebugAsync($"Successfully synchronized product {productMapping.NopProductId} with Nexport using the information from mapping {productMapping.Id}");
                }
            }
        }

        //TODO: Switch to use built-in order service method
        public async Task AddOrderNote(Order order, string note, bool? displayToCustomer = null,
            DateTime? utcNoteCreationDate = null, bool updateOrder = false)
        {
            //order.OrderNotes.Add(new OrderNote
            //{
            //    Note = note,
            //    DisplayToCustomer = displayToCustomer.GetValueOrDefault(false),
            //    CreatedOnUtc = utcNoteCreationDate.GetValueOrDefault(DateTime.UtcNow)
            //});

            //if (updateOrder)
            //{
            //    _orderService.UpdateOrder(order);
            //}
        }

        public async Task CreateAndMapNewNexportUserAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null");

            if (!_nexportSettings.RootOrganizationId.HasValue)
                return;

            if (await FindUserMappingByCustomerId(customer.Id) != null)
                return;

            var login = Guid.NewGuid().ToString();
            var password = CommonHelper.GenerateRandomDigitCode(20);
            var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);

            UserContactInfoRequest contactInfo = null;

            if (customer.BillingAddressId != null)
            {
                var currentBillingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                if (currentBillingAddress != null)
                {
                    var customerStateProvince =
                        await _stateProvinceService.GetStateProvinceByIdAsync(currentBillingAddress.StateProvinceId.GetValueOrDefault(0));

                    var customerAddressState = customerStateProvince != null ? customerStateProvince.Name : "";

                    var customerCountry =
                        await _countryService.GetCountryByIdAsync(currentBillingAddress.CountryId.GetValueOrDefault(0));

                    var customerAddressCountry = customerCountry != null ? customerCountry.Name : "";

                    contactInfo = new UserContactInfoRequest(apiErrorEntity: new ApiErrorEntity())
                    {
                        AddressLine1 = currentBillingAddress.Address1,
                        AddressLine2 = currentBillingAddress.Address2,
                        City = currentBillingAddress.City,
                        State = customerAddressState,
                        Country = customerAddressCountry,
                        PostalCode = currentBillingAddress.ZipPostalCode,
                        Phone = currentBillingAddress.PhoneNumber,
                        Fax = currentBillingAddress.FaxNumber
                    };
                }
            }

            var nexportUser = await CreateNexportUserAsync(login, password, firstName, lastName,
                customer.Email, _nexportSettings.RootOrganizationId.Value, contactInfo)!;

            if (nexportUser != null)
            {
                await InsertUserMapping(new NexportUserMapping
                {
                    NexportUserId = nexportUser.UserId,
                    NopUserId = customer.Id
                });
            }
        }

        public async Task SynchronizeContactInfoFromNexportAsync(Customer customer, Guid nexportUserId)
        {
            var userContactInfo = await GetNexportUserContactInfoAsync(nexportUserId);

            if (userContactInfo != null)
            {
                var customerStateProvince =
                    (await _stateProvinceService.GetStateProvincesAsync())
                        .FirstOrDefault(x =>
                            x.Name.Contains(userContactInfo.State) ||
                            x.Abbreviation.Contains(userContactInfo.State));

                var customerCountry =
                    (await _countryService.GetAllCountriesAsync())
                        .FirstOrDefault(x =>
                            x.Name.Contains(userContactInfo.Country) ||
                            x.ThreeLetterIsoCode.Contains(userContactInfo.Country) ||
                            x.TwoLetterIsoCode.Contains(userContactInfo.Country));

                var customerFirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                var customerLastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);

                var address = new Address
                {
                    FirstName = customerFirstName,
                    LastName = customerLastName,
                    Address1 = userContactInfo.AddressLine1,
                    Address2 = userContactInfo.AddressLine2,
                    City = userContactInfo.City,
                    StateProvinceId = customerStateProvince?.Id,
                    CountryId = customerCountry?.Id,
                    ZipPostalCode = userContactInfo.PostalCode,
                    Email = customer.Email,
                    PhoneNumber = userContactInfo.Phone,
                    FaxNumber = userContactInfo.Fax,
                    CreatedOnUtc = DateTime.UtcNow
                };

                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _customerService.InsertCustomerAddressAsync(customer, address);

                customer.BillingAddressId = address.Id;
                customer.ShippingAddressId = address.Id;

                await _customerService.UpdateCustomerAsync(customer);
            }
        }

        [CanBeNull]
        public async Task<(Guid EnrollmentId, Enums.PhaseEnum Phase, Enums.ResultEnum Result,
            DateTime? enrollementExpirationDate, int completionPercentage)?>
            VerifyNexportEnrollmentStatusAsync(Product product, Customer customer,
            int? storeId = null)
        {
            var mapping = await GetProductMappingByNopProductId(product.Id, storeId) ?? await GetProductMappingByNopProductId(product.Id);
            if (mapping != null)
            {
                var userMapping = await FindUserMappingByCustomerId(customer.Id);

                if (userMapping != null)
                {
                    return await VerifyNexportEnrollmentStatusAsync(mapping, userMapping);
                }
            }

            return null;
        }

        public async Task<(Guid EnrollmentId, Enums.PhaseEnum Phase, Enums.ResultEnum Result,
                DateTime? EnrollmentExpirationDate, int CompletionPercentage)?>
            VerifyNexportEnrollmentStatusAsync(NexportProductMapping productMapping,
            NexportUserMapping nexportUserMapping)
        {
            if (productMapping == null)
                throw new ArgumentNullException(nameof(productMapping), "Product mapping cannot be null!");

            if (nexportUserMapping == null)
                throw new ArgumentNullException(nameof(nexportUserMapping), "Nexport user mapping cannot be null!");

            if (!_nexportSettings.RootOrganizationId.HasValue)
                throw new NullReferenceException("Root organization Id is currently empty");

            if (!productMapping.NexportSyllabusId.HasValue)
                return null;

            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                Guid orgId;

                if (productMapping.NexportSubscriptionOrgId != null)
                {
                    orgId = productMapping.NexportSubscriptionOrgId.Value;
                }
                else
                {
                    orgId = await _genericAttributeService.GetAttributeAsync<Guid?>(store,
                        "NexportSubscriptionOrganizationId", store.Id) ?? _nexportSettings.RootOrganizationId.Value;
                }

                if (productMapping.Type == NexportProductTypeEnum.Section)
                {
                    var existingEnrollment = await GetSectionEnrollmentDetailsAsync(orgId,
                        nexportUserMapping.NexportUserId, productMapping.NexportSyllabusId.Value)!;

                    if (existingEnrollment != null)
                    {
                        return (existingEnrollment.EnrollmentId, existingEnrollment.Phase, existingEnrollment.Result,
                            existingEnrollment.ExpirationDate, existingEnrollment.PercentAssignmentsComplete);
                    }
                }
                else if (productMapping.Type == NexportProductTypeEnum.TrainingPlan)
                {
                    var existingEnrollment = await GetTrainingPlanEnrollmentDetailsAsync(orgId,
                        nexportUserMapping.NexportUserId, productMapping.NexportSyllabusId.Value)!;

                    if (existingEnrollment != null)
                    {
                        return (existingEnrollment.EnrollmentId, existingEnrollment.Phase, existingEnrollment.Result,
                            existingEnrollment.ExpirationDate, existingEnrollment.PercentRequirementsFulfilled);
                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot verify Nexport enrollment status of the product {productMapping.NexportProductName} [{productMapping.NexportSyllabusId.Value}] for user {nexportUserMapping.NexportUserId}";
                await _logger.ErrorAsync($"{errMsg}", ex);

                throw;
            }

            return null;
        }

        public async Task<bool> CanPurchaseNexportProductAsync(Product product, Customer customer)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            var storeModel = await _genericAttributeService.GetAttributeAsync<NexportStoreSaleModel>(
                store, "NexportStoreSaleModel", store.Id);

            if (storeModel != NexportStoreSaleModel.Retail)
                return true;

            var existingEnrollmentStatus = await VerifyNexportEnrollmentStatusAsync(product, customer, store.Id);
            var mapping = await GetProductMappingByNopProductId(product.Id, store.Id) ?? await GetProductMappingByNopProductId(product.Id);

            if (mapping != null)
            {
                if (existingEnrollmentStatus == null)
                {
                    if (mapping.IsExtensionProduct)
                        return false;

                    return await CanPurchaseDifferentProductInNexportCategoryAsync(product, customer, store.Id);
                }

                switch (existingEnrollmentStatus)
                {
                    case var status
                        when status.Value.Phase == Enums.PhaseEnum.Finished &&
                             status.Value.Result == Enums.ResultEnum.Failing:
                        {
                            var allowRepurchaseFailedCourses = await _genericAttributeService.GetAttributeAsync<bool>(store,
                                NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
                            return allowRepurchaseFailedCourses;
                        }

                    case var status
                        when status.Value.Phase == Enums.PhaseEnum.Finished &&
                             status.Value.Result == Enums.ResultEnum.Passing:
                        {
                            var allowRepurchasePassedCourses = await _genericAttributeService.GetAttributeAsync<bool>(store,
                                NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
                            return allowRepurchasePassedCourses;
                        }

                    case var status
                        when status.Value.Phase is Enums.PhaseEnum.InProgress or Enums.PhaseEnum.NotStarted:
                        {
                            // Customer is allowed to purchase this product under one of these scenarios:
                            // A - The product allows extension, the purchase limit has not exceed yet, and the enrollment has been expired.
                            // B - The product allows extension, the purchase limit has not exceed yet, the enrollment has not yet expired,
                            // and it is within the renewal window time-frame.
                            if (mapping.AllowExtension)
                            {
                                if (!await ExceedExtensionPurchaseLimitAsync(customer, mapping, status.Value.EnrollmentId))
                                {
                                    var currentEnrollmentExpirationDate = status.Value.enrollementExpirationDate;
                                    if (currentEnrollmentExpirationDate.HasValue)
                                    {
                                        if (currentEnrollmentExpirationDate >= DateTime.UtcNow)
                                        {
                                            if (!string.IsNullOrWhiteSpace(mapping.RenewalWindow))
                                            {
                                                var renewalWindowTimeSpan = TimeSpan.Parse(mapping.RenewalWindow);
                                                return DateTime.UtcNow >= currentEnrollmentExpirationDate - renewalWindowTimeSpan;
                                            }
                                        }

                                        // Allow customer to purchase since the enrollment has been expired
                                        return true;
                                    }

                                    // Customer is not allowed to purchase since there is still an active enrollment that has no expiration date
                                    return false;
                                }
                            }

                            // Customer is not allowed to purchase since the extension purchase limit has been met
                            return false;
                        }
                }
            }

            // Customer is allowed to purchase this product since no product mapping is available to determine the purchase eligibility
            return true;
        }

        public async Task<(ShoppingCartItem, Category)> CanPurchaseProductInNexportCategoryAsync(Product product,
            int storeId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (storeId < 1)
                return (null, null);

            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
            var shoppingCartItemsExceptCurrentProduct = await (await _shoppingCartService
                .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, storeId))
                .Where(x => x.ProductId != product.Id)
                .ToListAsync();

            foreach (var productCategory in productCategories)
            {
                var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
                if (category == null)
                    continue;

                var limitSinglePurchase = await _genericAttributeService.GetAttributeAsync<bool>(category,
                            NexportDefaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY);

                if (limitSinglePurchase)
                {
                    var productInSameCategory =
                        await shoppingCartItemsExceptCurrentProduct.FirstOrDefaultAwaitAsync(async itemProduct =>
                            (await _categoryService
                                .GetProductCategoriesByProductIdAsync(itemProduct.ProductId, true))
                                .Any(x => x.CategoryId == productCategory.CategoryId));

                    return (productInSameCategory, category);
                }
            }

            return (null, null);
        }

        public async Task<bool> CanPurchaseDifferentProductInNexportCategoryAsync(Product product, Customer customer,
            int storeId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (storeId < 1)
                throw new ArgumentException("Store Id is an invalid number", nameof(storeId));

            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

            foreach (var productCategory in productCategories)
            {
                var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
                if (category == null)
                    continue;

                var allowPurchaseDifferentProductInCategory = await _genericAttributeService.GetAttributeAsync(
                    category,
                    NexportDefaults.ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT,
                    defaultValue: true);

                if (!allowPurchaseDifferentProductInCategory)
                {
                    var productsCategoryInSameCategory =
                        await _categoryService.GetProductCategoriesByCategoryIdAsync(productCategory.CategoryId, showHidden: true);

                    foreach (var otherProductCategory in productsCategoryInSameCategory)
                    {
                        var productMapping = await GetProductMappingByNopProductId(otherProductCategory.ProductId, storeId) ??
                                             await GetProductMappingByNopProductId(otherProductCategory.ProductId);
                        if (productMapping != null)
                        {
                            var otherProduct = await _productService.GetProductByIdAsync(otherProductCategory.ProductId);
                            if (otherProduct != null)
                            {
                                var existingEnrollmentStatus = await VerifyNexportEnrollmentStatusAsync(otherProduct, customer, storeId)!;

                                var currentEnrollmentExpirationDate = existingEnrollmentStatus?.enrollementExpirationDate;
                                if (currentEnrollmentExpirationDate.HasValue &&
                                    currentEnrollmentExpirationDate >= DateTime.UtcNow &&
                                    existingEnrollmentStatus.Value.Phase is Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress)
                                {
                                    // Customer is not allowed to purchase this product
                                    // since there is an existing enrollment from a different product within this category
                                    // that has not been expired and that enrollment is currently either in the Not Started or In Progress phase.
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public async Task<bool> ExceedExtensionPurchaseLimitAsync(Customer customer, NexportProductMapping productMapping,
            Guid enrollmentId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (productMapping == null)
                throw new ArgumentNullException(nameof(productMapping));

            var nexportUserMapping = await FindUserMappingByCustomerId(customer.Id);

            if (productMapping.ExtensionPurchaseLimit != null && nexportUserMapping != null)
            {
                var previousInvoiceItems = await GetNexportOrderInvoiceItems(nexportUserMapping.NexportUserId);
                var previousExtensionCount = await previousInvoiceItems
                    .SelectAwait(async invoiceItem => await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId))
                    .CountAsync(orderItem => orderItem.ProductId == productMapping.NopProductId) - 1;

                return previousExtensionCount > productMapping.ExtensionPurchaseLimit;
            }

            return false;
        }

        public async Task<string> GetStoreNameAsync(int storeId)
        {
            var store = await _storeService.GetStoreByIdAsync(storeId);
            return store != null ? store.Name : "";
        }

        public async Task<List<SelectListItem>> GetSupplementalInfoQuestionList()
        {
            var supplementalInfoQuestions = await GetAllNexportSupplementalInfoQuestions();
            var listItems = supplementalInfoQuestions.Select(s => new SelectListItem
            {
                Text = s.QuestionText,
                Value = s.Id.ToString()
            });

            return await listItems.Select(item => new SelectListItem { Text = item.Text, Value = item.Value }).ToListAsync();
        }

        public async Task<List<int>> GetUnansweredQuestions(int customerId, int storeId, List<int> questionIds)
        {
            var currentAnswers = await GetNexportSupplementalInfoAnswers(customerId, storeId);
            var currentAnswered =
                currentAnswers.Where(x => questionIds.Contains(x.QuestionId)).ToList();
            var questionWithAnswerIds = currentAnswered.Count > 0
                ? currentAnswered.Select(x => x.QuestionId).ToList()
                : new List<int>();

            return await questionIds.Except(questionWithAnswerIds).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetRegistrationFieldCategoryList()
        {
            var registrationFieldCategories = await GetNexportRegistrationFieldCategories();
            var listItems = registrationFieldCategories.Select(s => new SelectListItem
            {
                Text = s.Title,
                Value = s.Id.ToString()
            });

            var result = listItems.Select(item => new SelectListItem { Text = item.Text, Value = item.Value }).ToList();

            result.Insert(0, new SelectListItem { Text = "None", Value = "" });

            return result;
        }

        public async Task<Dictionary<int, string>> ParseRegistrationFieldsAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var result = new Dictionary<int, string>();

            var registrationFields =
                (await GetNexportRegistrationFields((await _storeContext.GetCurrentStoreAsync()).Id))
                .Where(f => f.Type != NexportRegistrationFieldType.CustomType);

            foreach (var field in registrationFields)
            {
                if (!field.IsActive)
                    continue;

                var controlId = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{field.Id}";
                var controlValue = form[controlId];
                if (!StringValues.IsNullOrEmpty(controlValue))
                {
                    result.Add(field.Id, controlValue.ToString().Trim());
                }
            }

            return result;
        }

        public async Task<Dictionary<int, Dictionary<string, string>>> ParseCustomRegistrationFieldsAsync(
            IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var result = new Dictionary<int, Dictionary<string, string>>();

            var registrationFields =
                (await GetNexportRegistrationFields((await _storeContext.GetCurrentStoreAsync()).Id))
                .Where(f => f.Type == NexportRegistrationFieldType.CustomType);

            foreach (var field in registrationFields)
            {
                if (!field.IsActive)
                    continue;

                var customRender = await _registrationFieldCustomerRenderPluginManager.LoadPluginBySystemNameAsync(field.CustomFieldRender);

                if (customRender != null)
                {
                    var parseResult = await customRender.ParseCustomRegistrationFields(field.Id, form);
                    result.Add(field.Id, parseResult);
                }
            }

            return result;
        }

        public virtual async Task<IList<string>> GetRegistrationFieldWarnings(Dictionary<int, string> fields)
        {
            var warnings = new List<string>();

            foreach (var field in fields)
            {
                var registrationField = await GetNexportRegistrationFieldById(field.Key);

                var fieldNameTruncated = registrationField.Name.TruncateAtWord(5);

                if (registrationField.Type == NexportRegistrationFieldType.Email)
                {
                    if (!string.IsNullOrWhiteSpace(field.Value) && !field.Value.IsValidEmail())
                        warnings.Add($"Field {fieldNameTruncated} value is not a valid email");
                }
                else if (registrationField.Type == NexportRegistrationFieldType.DateOnly)
                {
                    if (!string.IsNullOrWhiteSpace(field.Value) && !field.Value.IsValidDateFormat("MM/dd/yyyy"))
                        warnings.Add($"Field {fieldNameTruncated} value is not a valid date format");
                }
                else if (registrationField.Type == NexportRegistrationFieldType.DateTime)
                {
                    if (!string.IsNullOrWhiteSpace(field.Value) && !field.Value.IsValidDateFormat("MM/dd/yyyy HH:mm"))
                        warnings.Add($"Field {fieldNameTruncated} value is not a valid date time format");
                }

                if (!registrationField.IsRequired)
                    continue;

                var emptyValue = string.IsNullOrWhiteSpace(field.Value);

                if (!emptyValue)
                    continue;

                var notFoundWarning = $"Field {fieldNameTruncated} value is empty";

                warnings.Add(notFoundWarning);
            }

            return warnings;
        }

        public virtual async Task<IList<string>> GetCustomRegistrationFieldWarnings(Dictionary<int, Dictionary<string, string>> fields)
        {
            var warnings = new List<string>();

            //foreach (var customField in fields)
            //{
            //}

            return warnings;
        }

        public virtual async Task SaveNexportRegistrationFields(Customer customer, Dictionary<int, string> fields)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            foreach (var field in fields)
            {
                var registrationField = await GetNexportRegistrationFieldById(field.Key);
                if (registrationField != null)
                {
                    if (registrationField.Type == NexportRegistrationFieldType.SelectCheckbox)
                    {
                        var optionIds = field.Value.Split(",");
                        foreach (var optionId in optionIds)
                        {
                            var newAnswer = new NexportRegistrationFieldAnswer
                            {
                                CustomerId = customer.Id,
                                FieldId = registrationField.Id,
                                UtcDateCreated = DateTime.UtcNow,
                                FieldOptionId = int.TryParse(optionId, out var selectionResult) ? selectionResult : default(int?)
                            };

                            await InsertNexportRegistrationFieldAnswer(newAnswer);
                        }
                    }
                    else
                    {
                        var newAnswer = new NexportRegistrationFieldAnswer
                        {
                            CustomerId = customer.Id,
                            FieldId = registrationField.Id
                        };
                        switch (registrationField.Type)
                        {
                            case NexportRegistrationFieldType.Text:
                            case NexportRegistrationFieldType.Email:
                                newAnswer.TextValue = field.Value;
                                break;

                            case NexportRegistrationFieldType.Numeric:
                                newAnswer.NumericValue = int.TryParse(field.Value, out var intResult) ? intResult : default(int?);
                                break;

                            case NexportRegistrationFieldType.Boolean:
                                newAnswer.BooleanValue = bool.TryParse(field.Value, out var booleanResult) ? booleanResult : default(bool?);
                                break;

                            case NexportRegistrationFieldType.DateOnly:
                            case NexportRegistrationFieldType.DateTime:
                                var fieldValue = DateTime.TryParse(field.Value, out var dateTimeResult) ? dateTimeResult : default(DateTime?);
                                newAnswer.DateTimeValue = fieldValue.HasValue ? _dateTimeHelper.ConvertToUtcTime(fieldValue.Value) : default(DateTime?);
                                break;

                            case NexportRegistrationFieldType.SelectDropDown:
                                newAnswer.FieldOptionId = int.TryParse(field.Value, out var selectionResult) ? selectionResult : default(int?);
                                break;

                            case NexportRegistrationFieldType.None:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        newAnswer.UtcDateCreated = DateTime.UtcNow;

                        await InsertNexportRegistrationFieldAnswer(newAnswer);
                    }
                }
            }
        }

        public virtual async Task<Dictionary<string, string>> ConvertFieldAnswersToSubmissionProfileFields(
            IList<NexportRegistrationFieldAnswer> fieldAnswers)
        {
            var result = new Dictionary<string, string>();

            var fieldAnswersByFieldId = fieldAnswers
                .GroupBy(x => x.FieldId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in fieldAnswersByFieldId)
            {
                var field = await GetNexportRegistrationFieldById(item.Key);

                if (field == null ||
                    (string.IsNullOrWhiteSpace(field.NexportCustomProfileFieldKey) && field.Type == NexportRegistrationFieldType.CustomType))
                    continue;

                var fieldValue = "";

                if (field.Type is NexportRegistrationFieldType.SelectCheckbox or NexportRegistrationFieldType.SelectDropDown)
                {
                    var answerValues = await item.Value.Where(answer => answer.FieldOptionId.HasValue)
                        .SelectAwait(async answer => await GetNexportRegistrationFieldOptionById(answer.FieldOptionId.Value))
                        .Where(fieldOption => fieldOption != null)
                        .Select(fieldOption => fieldOption.OptionValue).ToListAsync();

                    fieldValue = string.Join(", ", answerValues.ToArray());
                }
                else
                {
                    var answer = item.Value.FirstOrDefault();
                    if (answer != null)
                    {
                        if (!string.IsNullOrEmpty(answer.TextValue))
                        {
                            fieldValue = answer.TextValue;
                        }
                        else if (answer.NumericValue.HasValue)
                        {
                            fieldValue = answer.NumericValue.ToString();
                        }
                        else if (answer.BooleanValue.HasValue)
                        {
                            fieldValue = answer.BooleanValue.ToString();
                        }
                        else if (answer.DateTimeValue.HasValue)
                        {
                            fieldValue = answer.DateTimeValue.Value.ToString("MM/dd/yyyy HH:mm:ss");
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(field.NexportCustomProfileFieldKey))
                    result.Add(field.NexportCustomProfileFieldKey, fieldValue);
            }

            return result;
        }

        public async Task<Dictionary<string, string>> ConvertCustomFieldAnswersToSubmissionProfileFieldsAsync(
            IList<NexportRegistrationFieldAnswer> fieldAnswers)
        {
            var result = new Dictionary<string, string>();

            foreach (var answer in fieldAnswers)
            {
                var field = await GetNexportRegistrationFieldById(answer.FieldId);

                if (field is not { Type: NexportRegistrationFieldType.CustomType })
                    continue;

                var customRender =
                    await _registrationFieldCustomerRenderPluginManager.LoadPluginBySystemNameAsync(field.CustomFieldRender);
                var processResult = await customRender?.ProcessCustomRegistrationFields(answer.CustomerId, field.Id);
                if (processResult != null)
                {
                    result = result.Concat(processResult)
                        .ToDictionary(x => x.Key,
                            x => x.Value);
                }
            }

            return result;
        }

        public async Task<IList<NexportRegistrationFieldCustomRender>> GetNexportRegistrationFieldCustomRendersAsync()
        {
            var availablePlugins = await (await _registrationFieldCustomerRenderPluginManager.LoadAllPluginsAsync()).ToListAsync();

            var list = new List<NexportRegistrationFieldCustomRender>();

            foreach (var plugin in availablePlugins)
            {
                var pluginAttr = plugin.PluginDescriptor.PluginType.GetCustomAttributes(typeof(CustomRegistrationFieldRenderAttribute), false);

                if (pluginAttr.Length > 0)
                {
                    list.Add(new NexportRegistrationFieldCustomRender
                    {
                        Name = plugin.PluginDescriptor.FriendlyName,
                        RenderAssembly = plugin.PluginDescriptor.SystemName
                    });
                }
            }

            return list;
        }

        public async Task<List<SelectListItem>> GetCustomRegistrationFieldRendersAsync()
        {
            var customRegistrationFieldRenders = await GetNexportRegistrationFieldCustomRendersAsync();
            var listItems = customRegistrationFieldRenders.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.RenderAssembly.ToString()
            });

            var result = listItems.Select(item => new SelectListItem { Text = item.Text, Value = item.Value }).ToList();

            result.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = ""
            });

            return result;
        }
    }
}
