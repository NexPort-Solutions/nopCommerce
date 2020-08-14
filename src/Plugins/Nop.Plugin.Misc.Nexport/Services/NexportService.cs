﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
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
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Services.Helpers;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public partial class NexportService
    {
        #region Fields

        private readonly NexportApiService _nexportApiService;

        private readonly NexportSettings _nexportSettings;
        private readonly IAclService _aclService;
        private readonly ICacheManager _cacheManager;
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
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomerRenderPluginManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public NexportService(
            NexportApiService nexportApiService,
            NexportSettings nexportSettings,
            IAclService aclService,
            ICacheManager cacheManager,
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
            ICustomerActivityService customerActivityService,
            IOrderService orderService,
            ICategoryService categoryService,
            ISettingService settingService,
            IStoreService storeService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomerRenderPluginManager,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger logger)
        {
            _nexportApiService = nexportApiService;
            _nexportSettings = nexportSettings;
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
            _customerActivityService = customerActivityService;
            _orderService = orderService;
            _categoryService = categoryService;
            _settingService = settingService;
            _storeService = storeService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _registrationFieldCustomerRenderPluginManager = registrationFieldCustomerRenderPluginManager;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            _storeContext = storeContext;
            _logger = logger;
        }

        #endregion

        public void GenerateNewNexportToken(ConfigurationModel model)
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

                _settingService.SaveSetting(_nexportSettings);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            }
            catch (ApiException e)
            {
                var errMsg = "Cannot generate new Nexport authentication token!";
                _logger.Error($"{errMsg}: {e.Message}", e);
                _notificationService.ErrorNotification(errMsg);
            }
        }

        public GetUserResponse AuthenticateUser(string username, string password)
        {
            GetUserResponse result = null;
            try
            {
                var response = _nexportApiService.AuthenticateNexportUser(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    username, password);

                result = response.Response;
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
                _notificationService.ErrorNotification((string)e.ErrorContent);
            }

            return result;
        }

        public GetUserResponse ValidateUser(string username)
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
                    var message =
                        $"Nexport plugin access does not have permission to look up the user with login {username}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get a user with login {username}";
                    _logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
                _notificationService.ErrorNotification((string)e.ErrorContent);
            }

            return null;
        }

        public CreateUserResponse CreateNexportUser(string login, string password,
            string firstName, string lastName, string email, Guid ownerOrgId)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentNullException(nameof(login), "Login cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be empty");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentNullException(nameof(login), "First name cannot be empty");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentNullException(nameof(password), "Last name cannot be empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be empty");

            try
            {
                var response = _nexportApiService.CreateNexportUser(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    login, password, firstName, lastName, email, ownerOrgId);

                if (response.StatusCode == 200)
                    return response.Response;

                if (response.StatusCode == 403)
                {
                    var message =
                        $"Nexport plugin access does not have permission to create new user with login {login} in the organization {ownerOrgId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 409)
                {
                    var message = response.Response.ApiErrorEntity.ErrorMessage.Contains("exists") ?
                        $"Cannot create new user with {login} in the organization {ownerOrgId} because of duplication" :
                        $"Cannot find any organization with the Id of {ownerOrgId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 422)
                {
                    var message =
                        $"Validation exception occurred when trying to create new user with login {login} in organization {ownerOrgId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
                _notificationService.ErrorNotification((string)e.ErrorContent);
            }

            return null;
        }

        public GetUserResponse GetNexportUser(Guid userId)
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
                    var message =
                        $"Nexport plugin access does not have permission to look up the user with Id {userId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get a user with Id {userId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch (ApiException e)
            {
                var errMsg = $"Error occurred during GetUser api call for Nexport user {userId}";
                _logger.Error($"{errMsg}: {e.Message}", e);
                _notificationService.ErrorNotification(errMsg);
            }

            return null;
        }

        public EditUserResponse UpdateNexportUserContactInfo(Guid userId, UserContactInfoRequest updatedInfo)
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
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 409)
                {
                    var message = $"Cannot find user with the Id of {userId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 422)
                {
                    var message =
                        $"Validation exception occurred when trying to edit user with the Id {userId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
                _notificationService.ErrorNotification((string)e.ErrorContent);
            }

            return null;
        }

        public List<DirectoryResponseItem> SearchNexportDirectory(string searchTerm, int? page = null)
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
                _notificationService.ErrorNotification((string)e.ErrorContent);
            }

            return null;
        }

        public OrganizationResponseItem GetOrganizationDetails(Guid orgId)
        {
            OrganizationResponseItem result = null;

            try
            {
                var availableOrganizations = FindAllOrganizations(orgId);

                result = availableOrganizations.SingleOrDefault(s => s.OrgId == orgId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public IList<OrganizationResponseItem> FindAllOrganizations(Guid baseOrgId)
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
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return items;
        }

        public IList<OrganizationResponseItem> FindAllOrganizationsUnderRootOrganization()
        {
            if (!_nexportSettings.RootOrganizationId.HasValue)
                throw new NullReferenceException("Root organization has not been set");

            return FindAllOrganizations(_nexportSettings.RootOrganizationId.Value);
        }

        public IList<SubscriptionResponse> FindAllSubscriptions(Guid userId)
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
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return items;
        }

        public SetCustomProfileFieldValuesResponse SetCustomProfileFieldValues(Guid subscriberId, Dictionary<string, string> profileFields)
        {
            SetCustomProfileFieldValuesResponse result = null;

            try
            {
                result = _nexportApiService.SetNexportCustomerProfileFieldValues(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, subscriberId, profileFields);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public IPagedList<CatalogResponseItem> FindAllCatalogs(Guid? orgId, int pageIndex = 0, int pageSize = int.MaxValue)
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
                catch (Exception e)
                {
                    _logger.Error(e.Message, e);
                }
            }

            var pagedItems = new PagedList<CatalogResponseItem>(items, pageIndex, pageSize);

            return pagedItems;
        }

        public CatalogResponseItem GetCatalogDetails(Guid catalogId)
        {
            CatalogResponseItem result = null;

            try
            {
                result = _nexportApiService.GetNexportCatalogDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public GetDescriptionResponse GetCatalogDescription(Guid catalogId)
        {
            GetDescriptionResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportCatalogDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public GetCatalogCreditHoursResponse GetCatalogCreditHours(Guid catalogId)
        {
            GetCatalogCreditHoursResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportCatalogCreditHours(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public IPagedList<GetSyllabiResponseItem> FindAllSyllabuses(Guid? catalogId, int pageIndex = 0, int pageSize = int.MaxValue)
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
                catch (Exception e)
                {
                    _logger.Error(e.Message, e);
                }
            }

            var pagedItems = new PagedList<GetSyllabiResponseItem>(items, pageIndex, pageSize);

            return pagedItems;
        }

        public SectionResponse GetSectionDetails(Guid sectionId)
        {
            SectionResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportSectionDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public GetDescriptionResponse GetSectionDescription(Guid sectionId)
        {
            GetDescriptionResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportSectionDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public GetObjectivesResponse GetSectionObjectives(Guid sectionId)
        {
            GetObjectivesResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportSectionObjectives(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public TrainingPlanResponse GetTrainingPlanDetails(Guid trainingPlanId)
        {
            TrainingPlanResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportTrainingPlanDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, trainingPlanId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public GetDescriptionResponse GetTrainingPlanDescription(Guid trainingPlanId)
        {
            GetDescriptionResponse result = null;

            try
            {
                result = _nexportApiService.GetNexportTrainingPlanDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, trainingPlanId);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }

            return result;
        }

        public GetInvoiceResponse GetNexportInvoice(Guid invoiceId)
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
                    var message =
                        $"Nexport plugin access does not have permission to look up the invoice {invoiceId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (response.StatusCode == 422)
                {
                    var message = $"Validation exception occurred when trying to get the invoice {invoiceId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch (Exception e)
            {
                var errMsg = $"Error occured during GetNexportInvoice api call for the invoice {invoiceId}";
                _logger.Error($"{errMsg}: {e.Message}", e);

                _notificationService.ErrorNotification(errMsg);
            }

            return null;
        }

        public Guid BeginNexportOrderInvoiceTransaction(Guid orgId, Guid purchasingAgentId)
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
                    $"Fail to create Nexport order invoice transaction in organization {orgId} with purchasing agent {purchasingAgentId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }
        }

        public Guid? AddItemToNexportOrderInvoice(Guid invoiceId, Guid nexportProductId,
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
                var errMsg = $"Cannot add the Nexport product {nexportProductId} to the invoice {invoiceId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }

            return addInvoiceItemResult?.InvoiceItemId;
        }

        public void CommitNexportOrderInvoiceTransaction(Guid invoiceId)
        {
            try
            {
                _nexportApiService.CommitNexportInvoiceTransaction(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId);
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot commit the Nexport invoice {invoiceId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }
        }

        public void AddPaymentToNexportOrderInvoice(Guid invoiceId, decimal totalCost, Guid payeeId, int nopOrderId,
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
                var errMsg = $"Cannot add payment to the the Nexport invoice {invoiceId} in the order {nopOrderId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }
        }

        public void RedeemNexportInvoiceItem(NexportOrderInvoiceItem invoiceItem, Guid redeemingUserId,
            RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction = RedeemInvoiceItemRequest.RedemptionActionTypeEnum.NormalRedemption)
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

                if (redeemInvoiceResult.RedemptionEnrollmentId != null)
                {
                    invoiceItem.RedemptionEnrollmentId = redeemInvoiceResult.RedemptionEnrollmentId;
                }

                UpdateNexportOrderInvoiceItem(invoiceItem);
            }
            catch (Exception ex)
            {
                var errMsg =
                    $"Cannot redeem the Nexport invoice item {invoiceItem.InvoiceItemId} in the order {invoiceItem.OrderId} for user {redeemingUserId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }
        }

        public string SignInNexport(NexportOrderInvoiceItem invoiceItem)
        {
            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            try
            {
                var redemption = _nexportApiService.GetNexportInvoiceRedemption(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceItem.InvoiceItemId);
                if (redemption.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                {
                    SsoResponse signInResult = null;
                    if (invoiceItem.RedemptionEnrollmentId == null)
                    {
                        if (redemption.RedemptionUserId != null)
                        {
                            signInResult = _nexportApiService.NexportSingleSignOn(_nexportSettings.Url,
                                _nexportSettings.AuthenticationToken, redemption.OrganizationId,
                                redemption.RedemptionUserId.Value, _storeContext.CurrentStore.Url);
                        }
                    }
                    else
                    {
                        if (redemption.RedemptionEnrollmentId != null)
                        {
                            signInResult = _nexportApiService.NexportClassroomSingleSignOn(_nexportSettings.Url,
                                _nexportSettings.AuthenticationToken, redemption.RedemptionEnrollmentId.Value,
                                _storeContext.CurrentStore.Url);
                        }
                    }

                    if (signInResult != null && signInResult.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                    {
                        return signInResult.Url;
                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot sign-on the user {invoiceItem.RedeemingUserId} into Nexport through SSO using the invoice item {invoiceItem.InvoiceItemId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }

            return null;
        }

        public string SignInNexport(Guid orgId, Guid userId)
        {
            if (orgId == Guid.Empty)
                throw new ArgumentException("Organization Id cannot be null", nameof(orgId));

            try
            {
                var response = _nexportApiService.NexportSingleSignOn(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken,
                    orgId, userId, _storeContext.CurrentStore.Url);

                if (response.ApiErrorEntity.ErrorCode == 0)
                    return response.Url;
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot sign-on the user {userId} into Nexport organization {orgId} through SSO";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }

            return null;
        }

        public List<MemberShipInfo> AddNexportMemberships(Guid userId, IList<Guid> groupIds)
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
                var errMsg = $"Cannot add new memberships for user {userId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

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

        public List<RemovedMembershipInfo> RemoveNexportMemberships(IList<Guid> membershipIds)
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
                var errMsg = $"Cannot remove Nexport memberships";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

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

        public List<NexportOrganizationModel> FindNexportRedemptionOrganizationsByCustomerId(int customerId)
        {
            var orders = _orderService.SearchOrders(_storeContext.CurrentStore.Id, customerId: customerId);
            var organizationModelList = new List<NexportOrganizationModel>();

            foreach (var order in orders)
            {
                var orderItems = order.OrderItems;
                foreach (var orderItem in orderItems)
                {
                    var orderInvoiceItem = FindNexportOrderInvoiceItem(order.Id, orderItem.Id);
                    if (orderInvoiceItem?.UtcDateRedemption != null)
                    {
                        var invoiceRedemption = _nexportApiService.GetNexportInvoiceRedemption(
                            _nexportSettings.Url, _nexportSettings.AuthenticationToken,
                            orderInvoiceItem.InvoiceItemId);
                        if (invoiceRedemption.ApiErrorEntity.ErrorCode == 0)
                        {
                            if (!organizationModelList.Exists(i => i.OrgId == invoiceRedemption.OrganizationId))
                            {
                                var availableOrganizations = FindAllOrganizations(invoiceRedemption.OrganizationId);

                                var org = availableOrganizations
                                    .FirstOrDefault(o =>
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

        public void SyncNexportProduct(int mappingId, [CanBeNull] Product product = null)
        {
            if (mappingId == 0)
                throw new ArgumentException("Mapping Id is invalid!", nameof(mappingId));

            var productMapping = GetProductMappingById(mappingId);

            if (productMapping != null)
            {
                product = product ?? _productService.GetProductById(productMapping.NopProductId);

                if (product != null && productMapping.NopProductId == product.Id)
                {
                    switch (productMapping.Type)
                    {
                        case NexportProductTypeEnum.Catalog:
                            //var catalogDetails = GetCatalogDetails(productMapping.NexportCatalogId);
                            //var catalogDescription = GetCatalogDescription(productMapping.NexportCatalogId);
                            var catalogCreditHours = GetCatalogCreditHours(productMapping.NexportCatalogId);

                            //product.Name = catalogDetails.Name;
                            //product.FullDescription = catalogDescription.Description;
                            productMapping.CreditHours = catalogCreditHours.CreditHours;

                            break;

                        case NexportProductTypeEnum.Section:
                            if (!productMapping.NexportSyllabusId.HasValue)
                            {
                                throw new Exception("Section Id cannot be null");
                            }

                            var sectionId = productMapping.NexportSyllabusId.Value;

                            var sectionDetails = GetSectionDetails(sectionId);
                            //var sectionDescription = GetSectionDescription(sectionId);
                            //var sectionObjective = GetSectionObjectives(sectionId);

                            //product.Name = sectionDetails.Title;
                            //product.FullDescription = sectionDescription.Description;
                            //product.ShortDescription = sectionObjective.Objectives;
                            //product.Sku = sectionDetails.SectionNumber;
                            //product.AvailableStartDateTimeUtc = sectionDetails.EnrollmentStart;
                            //product.AvailableEndDateTimeUtc = sectionDetails.EnrollmentEnd;

                            productMapping.CreditHours = sectionDetails.CreditHours;
                            productMapping.SectionCeus = sectionDetails.SectionCeus;

                            break;

                        case NexportProductTypeEnum.TrainingPlan:
                            if (!productMapping.NexportSyllabusId.HasValue)
                            {
                                throw new Exception("Training Plan Id cannot be null");
                            }

                            var trainingPlanId = productMapping.NexportSyllabusId.Value;

                            var trainingPlanDetails = GetTrainingPlanDetails(trainingPlanId);
                            //var trainingPlanDescription = GetTrainingPlanDescription(trainingPlanId);

                            //product.Name = trainingPlanDetails.Title;
                            //product.FullDescription = trainingPlanDescription.Description;
                            //product.AvailableStartDateTimeUtc = trainingPlanDetails.EnrollmentStart;
                            //product.AvailableEndDateTimeUtc = trainingPlanDetails.EnrollmentEnd;

                            productMapping.CreditHours = trainingPlanDetails.CreditHours;

                            break;

                        default:
                            goto case NexportProductTypeEnum.Catalog;
                    }

                    productMapping.IsSynchronized = true;
                    productMapping.UtcLastSynchronizationDate = DateTime.UtcNow;

                    UpdateNexportProductMapping(productMapping);

                    _logger.Debug($"Successfully synchronized product {productMapping.NopProductId} with Nexport using the information from mapping {productMapping.Id}");
                }
            }
        }

        public void AddOrderNote(Order order, string note, bool? displayToCustomer = null, DateTime? utcNoteCreationDate = null, bool updateOrder = false)
        {
            order.OrderNotes.Add(new OrderNote
            {
                Note = note,
                DisplayToCustomer = displayToCustomer.GetValueOrDefault(false),
                CreatedOnUtc = utcNoteCreationDate.GetValueOrDefault(DateTime.UtcNow)
            });

            if (updateOrder)
            {
                _orderService.UpdateOrder(order);
            }
        }

        public void CreateAndMapNewNexportUser(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null");

            if (!_nexportSettings.RootOrganizationId.HasValue)
                return;

            if (FindUserMappingByCustomerId(customer.Id) != null)
                return;

            var login = Guid.NewGuid().ToString();
            var password = CommonHelper.GenerateRandomDigitCode(20);
            var firstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
            var lastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);

            var nexportUser = CreateNexportUser(login, password, firstName, lastName,
                customer.Email, _nexportSettings.RootOrganizationId.Value);

            InsertUserMapping(new NexportUserMapping()
            {
                NexportUserId = nexportUser.UserId,
                NopUserId = customer.Id
            });
        }

        [CanBeNull]
        public (Enums.PhaseEnum Phase, Enums.ResultEnum Result, DateTime? enrollementExpirationDate)? VerifyNexportEnrollmentStatus(Product product,
            Customer customer, int? storeId = null)
        {
            var mapping = GetProductMappingByNopProductId(product.Id, storeId);
            if (mapping != null)
            {
                var userMapping = FindUserMappingByCustomerId(customer.Id);

                if (userMapping != null)
                {
                    return VerifyNexportEnrollmentStatus(mapping, userMapping);
                }
            }

            return null;
        }

        public (Enums.PhaseEnum Phase, Enums.ResultEnum Result, DateTime? enrollmentExpirationDate)? VerifyNexportEnrollmentStatus(NexportProductMapping productMapping, NexportUserMapping nexportUserMapping)
        {
            if (productMapping == null)
                throw new ArgumentNullException(nameof(productMapping), "Product mapping cannot be null!");

            if (nexportUserMapping == null)
                throw new ArgumentNullException(nameof(nexportUserMapping), "Nexport user mapping cannot be null!");

            try
            {
                var store = _storeContext.CurrentStore;

                Guid orgId;

                if (productMapping.NexportSubscriptionOrgId != null)
                {
                    orgId = productMapping.NexportSubscriptionOrgId.Value;
                }
                else
                {
                    orgId = _genericAttributeService.GetAttribute<Guid?>(store,
                        "NexportSubscriptionOrganizationId", store.Id) ?? _nexportSettings.RootOrganizationId.Value;
                }

                if (productMapping.Type == NexportProductTypeEnum.Section)
                {
                    var existingEnrollment = _nexportApiService.GetNexportSectionEnrollment(
                        _nexportSettings.Url, _nexportSettings.AuthenticationToken,
                        orgId, nexportUserMapping.NexportUserId, productMapping.NexportSyllabusId.Value);

                    if (existingEnrollment != null)
                    {
                        return (existingEnrollment.Phase, existingEnrollment.Result, existingEnrollment.ExpirationDate);
                    }
                }
                else if (productMapping.Type == NexportProductTypeEnum.TrainingPlan)
                {
                    var existingEnrollment = _nexportApiService.GetNexportTrainingPlanEnrollment(
                        _nexportSettings.Url, _nexportSettings.AuthenticationToken,
                        orgId, nexportUserMapping.NexportUserId, productMapping.NexportSyllabusId.Value);

                    if (existingEnrollment != null)
                    {
                        return (existingEnrollment.Phase, existingEnrollment.Result, existingEnrollment.ExpirationDate);
                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot verify Nexport enrollment status of the product {productMapping.NexportProductName} [{productMapping.NexportSyllabusId.Value}] for user {nexportUserMapping.NexportUserId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }

            return null;
        }

        public bool CanRepurchaseNexportProduct(Product product, Customer customer)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = _storeContext.CurrentStore;
            var storeModel = _genericAttributeService.GetAttribute<NexportStoreSaleModel>(
                store, "NexportStoreSaleModel", store.Id);

            if (storeModel != NexportStoreSaleModel.Retail)
                return true;

            var existingEnrollmentStatus = VerifyNexportEnrollmentStatus(product, customer, store.Id);
            var mapping = GetProductMappingByNopProductId(product.Id, store.Id);

            if (mapping != null)
            {
                if (existingEnrollmentStatus != null)
                {
                    switch (existingEnrollmentStatus)
                    {
                        case var status
                            when status.Value.Phase == Enums.PhaseEnum.Finished && status.Value.Result == Enums.ResultEnum.Failing:
                            {
                                var allowRepurchaseFailedCourses = _genericAttributeService.GetAttribute<bool>(store,
                                    NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
                                return allowRepurchaseFailedCourses;
                            }

                        case var status
                            when status.Value.Phase == Enums.PhaseEnum.Finished && status.Value.Result == Enums.ResultEnum.Passing:
                            {
                                var allowRepurchasePassedCourses = _genericAttributeService.GetAttribute<bool>(store,
                                    NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
                                return allowRepurchasePassedCourses;
                            }

                        case var status
                            when status.Value.Phase == Enums.PhaseEnum.InProgress:
                            {
                                var currentEnrollmentExpirationDate = status.Value.enrollementExpirationDate;

                                if (currentEnrollmentExpirationDate.HasValue)
                                {
                                    if (mapping.AllowExtension)
                                    {
                                        if (mapping.UtcAccessExpirationDate.HasValue)
                                        {
                                            if (mapping.UtcAccessExpirationDate.Value > currentEnrollmentExpirationDate.Value)
                                            {
                                                if (!string.IsNullOrWhiteSpace(mapping.RenewalWindow))
                                                {
                                                    var renewalWindowTimeSpan = TimeSpan.Parse(mapping.RenewalWindow);
                                                    return DateTime.UtcNow >= mapping.UtcAccessExpirationDate.Value - renewalWindowTimeSpan;
                                                }

                                                return true;
                                            }

                                            return false;
                                        }

                                        return true;
                                    }

                                    return CanRepurchaseProductInNexportCategory(product, customer, store.Id);
                                }

                                return CanRepurchaseProductInNexportCategory(product, customer, store.Id);
                            }

                        case var status
                            when status.Value.Phase == Enums.PhaseEnum.NotStarted:
                            {
                                return CanRepurchaseProductInNexportCategory(product, customer, store.Id);
                            }
                    }
                }
                else
                {
                    if (mapping.IsExtensionProduct)
                        return false;
                }
            }

            return true;
        }

        public (ShoppingCartItem, Category) CanPurchaseProductInNexportCategory(Product product, int storeId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (storeId < 1)
                return (null, null);

            var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id, storeId);
            var shoppingCartItemsExceptCurrentProduct = _shoppingCartService
                .GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, storeId)
                .Where(x => x.ProductId != product.Id)
                .ToList();

            foreach (var productCategory in productCategories)
            {
                var limitSinglePurchase = _genericAttributeService.GetAttribute<bool>(productCategory.Category,
                            NexportDefaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY);

                if (limitSinglePurchase)
                {
                    var productInSameCategory =
                        shoppingCartItemsExceptCurrentProduct.FirstOrDefault(itemProduct =>
                            _categoryService
                                .GetProductCategoriesByProductId(itemProduct.ProductId,
                                    _storeContext.CurrentStore.Id)
                                .Any(x => x.CategoryId == productCategory.CategoryId));

                    return (productInSameCategory, productCategory.Category);
                }
            }

            return (null, null);
        }

        public bool CanRepurchaseProductInNexportCategory(Product product, Customer customer, int storeId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (storeId < 1)
                return false;

            var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id, storeId);

            return productCategories.Select(productCategory =>
                _genericAttributeService.GetAttribute(
                    productCategory.Category,
                    NexportDefaults.ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT,
                    defaultValue: false))
                .FirstOrDefault();
        }

        public string GetStoreName(int storeId)
        {
            var store = _storeService.GetStoreById(storeId);
            return store != null ? store.Name : "";
        }

        public List<SelectListItem> GetSupplementalInfoQuestionList()
        {
            var supplementalInfoQuestions = GetAllNexportSupplementalInfoQuestions();
            var listItems = supplementalInfoQuestions.Select(s => new SelectListItem
            {
                Text = s.QuestionText,
                Value = s.Id.ToString()
            });

            return listItems.Select(item => new SelectListItem { Text = item.Text, Value = item.Value }).ToList();
        }

        public List<int> GetUnansweredQuestions(int customerId, int storeId, List<int> questionIds)
        {
            var currentAnswers = GetNexportSupplementalInfoAnswers(customerId, storeId);
            var currentAnswered = currentAnswers.Where(x => questionIds.Contains(x.QuestionId)).ToList();
            var questionWithAnswerIds = currentAnswered.Count > 0
                ? currentAnswered.Select(x => x.QuestionId).ToList()
                : new List<int>();

            return questionIds.Except(questionWithAnswerIds).ToList();
        }

        public List<SelectListItem> GetRegistrationFieldCategoryList()
        {
            var registrationFieldCategories = GetNexportRegistrationFieldCategories();
            var listItems = registrationFieldCategories.Select(s => new SelectListItem
            {
                Text = s.Title,
                Value = s.Id.ToString()
            });

            var result = listItems.Select(item => new SelectListItem { Text = item.Text, Value = item.Value }).ToList();

            result.Insert(0, new SelectListItem { Text = "None", Value = "" });

            return result;
        }

        public Dictionary<int, string> ParseRegistrationFields(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var result = new Dictionary<int, string>();

            var registrationFields = GetNexportRegistrationFields(_storeContext.CurrentStore.Id)
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

        public Dictionary<int, Dictionary<string, string>> ParseCustomRegistrationFields(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var result = new Dictionary<int, Dictionary<string, string>>();

            var registrationFields = GetNexportRegistrationFields(_storeContext.CurrentStore.Id)
                .Where(f => f.Type == NexportRegistrationFieldType.CustomType);

            foreach (var field in registrationFields)
            {
                if (!field.IsActive)
                    continue;

                var customRender = _registrationFieldCustomerRenderPluginManager.LoadPluginBySystemName(field.CustomFieldRender);

                if (customRender != null)
                {
                    var parseResult = customRender.ParseCustomRegistrationFields(field.Id, form);
                    result.Add(field.Id, parseResult);
                }
            }

            return result;
        }

        public virtual IList<string> GetRegistrationFieldWarnings(Dictionary<int, string> fields)
        {
            var warnings = new List<string>();

            foreach (var field in fields)
            {
                var registrationField = GetNexportRegistrationFieldById(field.Key);

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

        public virtual IList<string> GetCustomRegistrationFieldWarnings(Dictionary<int, Dictionary<string, string>> fields)
        {
            var warnings = new List<string>();

            //foreach (var customField in fields)
            //{
            //}

            return warnings;
        }

        public virtual void SaveNexportRegistrationFields(Customer customer, Dictionary<int, string> fields)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            foreach (var field in fields)
            {
                var registrationField = GetNexportRegistrationFieldById(field.Key);
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

                            InsertNexportRegistrationFieldAnswer(newAnswer);
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

                        InsertNexportRegistrationFieldAnswer(newAnswer);
                    }
                }
            }
        }

        public virtual Dictionary<string, string> ConvertFieldAnswersToSubmissionProfileFields(IList<NexportRegistrationFieldAnswer> fieldAnswers)
        {
            var result = new Dictionary<string, string>();

            var fieldAnswersByFieldId = fieldAnswers
                .GroupBy(x => x.FieldId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in fieldAnswersByFieldId)
            {
                var field = GetNexportRegistrationFieldById(item.Key);

                if (field == null ||
                    (string.IsNullOrWhiteSpace(field.NexportCustomProfileFieldKey) && field.Type == NexportRegistrationFieldType.CustomType))
                    continue;

                var fieldValue = "";

                if (field.Type == NexportRegistrationFieldType.SelectCheckbox ||
                    field.Type == NexportRegistrationFieldType.SelectDropDown)
                {
                    var answerValues = item.Value.Where(answer => answer.FieldOptionId.HasValue)
                        .Select(answer => GetNexportRegistrationFieldOptionById(answer.FieldOptionId.Value))
                        .Where(fieldOption => fieldOption != null)
                        .Select(fieldOption => fieldOption.OptionValue).ToList();

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

                if(!string.IsNullOrWhiteSpace(field.NexportCustomProfileFieldKey))
                    result.Add(field.NexportCustomProfileFieldKey, fieldValue);
            }

            return result;
        }

        public Dictionary<string, string> ConvertCustomFieldAnswersToSubmissionProfileFields(IList<NexportRegistrationFieldAnswer> fieldAnswers)
        {
            var result = new Dictionary<string, string>();

            foreach (var answer in fieldAnswers)
            {
                var field = GetNexportRegistrationFieldById(answer.FieldId);

                if (field == null || field.Type != NexportRegistrationFieldType.CustomType)
                    continue;

                var customRender =
                    _registrationFieldCustomerRenderPluginManager.LoadPluginBySystemName(field.CustomFieldRender);
                var processResult = customRender?.ProcessCustomRegistrationFields(answer.CustomerId, field.Id);
                if (processResult != null)
                {
                    result = result.Concat(processResult)
                        .ToDictionary(x => x.Key,
                            x => x.Value);
                }
            }

            return result;
        }

        public IList<NexportRegistrationFieldCustomRender> GetNexportRegistrationFieldCustomRenders()
        {
            var availablePlugins = _registrationFieldCustomerRenderPluginManager.LoadAllPlugins().ToList();

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

        public List<SelectListItem> GetCustomRegistrationFieldRenders()
        {
            var customRegistrationFieldRenders = GetNexportRegistrationFieldCustomRenders();
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
