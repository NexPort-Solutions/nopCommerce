using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
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
using Nop.Plugin.Misc.Nexport.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Services.Orders;

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
        private readonly IRepository<NexportUserMapping> _nexportUserMappingRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
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
            IRepository<NexportUserMapping> nexportUserMappingRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            ICustomerActivityService customerActivityService,
            IOrderService orderService,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
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
            _nexportUserMappingRepository = nexportUserMappingRepository;
            _storeMappingRepository = storeMappingRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _customerActivityService = customerActivityService;
            _orderService = orderService;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
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

                _nexportSettings.AuthenticationToken = response.AccessToken;
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
                _notificationService.ErrorNotification(e.ErrorContent);
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
                _notificationService.ErrorNotification(e.ErrorContent);
            }

            return null;
        }

        public CreateUserResponse CreateNexportUser(string login, string password,
            string firstName, string lastName, string email, Guid ownerOrgId)
        {
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
                _notificationService.ErrorNotification(e.ErrorContent);
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
                var errMsg = $"Error occured during GetUser api call for Nexport user {userId}";
                _logger.Error($"{errMsg}: {e.Message}", e);
                _notificationService.ErrorNotification(errMsg);
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
                _notificationService.ErrorNotification(e.ErrorContent);
            }

            return null;
        }

        public OrganizationResponseItem GetOrganizationDetails(Guid orgId)
        {
            OrganizationResponseItem result = null;

            try
            {
                var response = _nexportApiService.GetNexportOrganizations(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                    orgId, 0);
                if (response.TotalRecord > 0)
                    result = response.OrganizationList.SingleOrDefault(s => s.OrgId == orgId.ToString());
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public IList<OrganizationResponseItem> FindAllOrganizations()
        {
            var items = new List<OrganizationResponseItem>();

            if (!_nexportSettings.RootOrganizationId.HasValue)
                throw new NullReferenceException("Root organization has not been set");

            try
            {
                var page = 0;
                var remainderItemsCount = 0;
                do
                {
                    var result = _nexportApiService.GetNexportOrganizations(_nexportSettings.Url,
                        _nexportSettings.AuthenticationToken, _nexportSettings.RootOrganizationId.Value, page);

                    page++;
                    remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
                    items.AddRange(result.OrganizationList);
                } while (remainderItemsCount > -1);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return items;
        }

        public IPagedList<CatalogResponseItem> FindAllCatalogs(Guid? orgId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var items = new List<CatalogResponseItem>();

            if (orgId.HasValue)
            {
                try
                {
                    var page = 0;
                    var remainderItemsCount = 0;
                    do
                    {
                        var result = _nexportApiService.GetNexportCatalogs(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, orgId.Value, page);

                        page++;
                        remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);

                        items.AddRange(result.CatalogList);
                    } while (remainderItemsCount > -1);
                }
                catch (ApiException e)
                {
                    _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
                    var page = 0;
                    var remainderItemsCount = 0;
                    do
                    {
                        var result = _nexportApiService.GetNexportSyllabuses(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, catalogId.Value, page);

                        page++;
                        remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);

                        items.AddRange(result.SyllabusList);
                    } while (remainderItemsCount > -1);
                }
                catch (ApiException e)
                {
                    _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
            catch (ApiException e)
            {
                var errMsg = $"Error occured during GetNexportInvoice api call for the invoice {invoiceId}";
                _logger.Error($"{errMsg}: {e.Message}", e);

                _notificationService.ErrorNotification(errMsg);
            }

            return null;
        }

        [NotNull]
        public string BeginNexportOrderInvoiceTransaction(Guid orgId, Guid purchasingAgentId)
        {
            try
            {
                var beginOrderResult = _nexportApiService.BeginNexportInvoiceTransaction(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, orgId, purchasingAgentId);

                return beginOrderResult.InvoiceId;
            }
            catch (ApiException ex)
            {
                var errMsg =
                    $"Fail to create Nexport order invoice transaction in organization {orgId} with purchasing agent {purchasingAgentId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);

                throw;
            }
        }

        public string AddItemToNexportOrderInvoice(Guid invoiceId, Guid nexportProductId,
            Enums.ProductTypeEnum productType, decimal productCost,
            Guid subscriptionOrgId, List<Guid> groupMembershipIds = null,
            DateTime? accessExpirationDate = null, string accessExpirationTimeLimit = null, string note = null)
        {
            AddInvoiceItemResponse addInvoiceItemResult = null;

            try
            {
                addInvoiceItemResult = _nexportApiService.AddNexportInvoiceItem(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId, nexportProductId,
                    productType, subscriptionOrgId, groupMembershipIds,
                    productCost, note, accessExpirationDate, accessExpirationTimeLimit);
            }
            catch (ApiException ex)
            {
                var errMsg = $"Cannot add the Nexport product {nexportProductId} to the invoice {invoiceId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);
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
            catch (ApiException ex)
            {
                var errMsg = $"Cannot commit the Nexport invoice {invoiceId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);
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
            catch (ApiException ex)
            {
                var errMsg = $"Cannot add payment to the the Nexport invoice {invoiceId} in the order {nopOrderId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);
            }
        }

        public void RedeemNexportOrder(NexportOrderInvoiceItem invoiceItem, Guid redeemingUserId)
        {
            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            if (redeemingUserId == Guid.Empty)
                throw new ArgumentException("Redeeming User Id cannot be empty identifier", nameof(redeemingUserId));

            try
            {
                var redeemInvoiceResult = _nexportApiService.RedeemNexportInvoice(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceItem.InvoiceItemId.ToString(), redeemingUserId, RedeemInvoiceItemRequest.RedemptionActionTypeEnum.NormalRedemption);

                if (redeemInvoiceResult.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                {
                    invoiceItem.RedeemingUserId = redeemingUserId;
                    invoiceItem.UtcDateRedemption = redeemInvoiceResult.UtcRedemptionDate;
                    if (!string.IsNullOrEmpty(redeemInvoiceResult.RedemptionEnrollmentId))
                    {
                        invoiceItem.RedemptionEnrollmentId = Guid.Parse(redeemInvoiceResult.RedemptionEnrollmentId);
                    }

                    UpdateNexportOrderInvoiceItem(invoiceItem);
                }
            }
            catch (ApiException ex)
            {
                var errMsg =
                    $"Cannot redeem the Nexport invoice item {invoiceItem.InvoiceItemId} in the order {invoiceItem.OrderId} for user {redeemingUserId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);
            }
        }

        public string SignInNexport(NexportOrderInvoiceItem invoiceItem)
        {
            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            try
            {
                var redemption = _nexportApiService.GetNexportInvoiceRedemption(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceItem.InvoiceItemId.ToString());
                if (redemption.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                {
                    SsoResponse signInResult;
                    if (invoiceItem.RedemptionEnrollmentId == null)
                    {
                        signInResult = _nexportApiService.NexportSingleSignOn(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, redemption.OrganizationId,
                            redemption.RedemptionUserId, _storeContext.CurrentStore.Url);
                    }
                    else
                    {
                        signInResult = _nexportApiService.NexportClassroomSingleSignOn(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, redemption.RedemptionEnrollmentId, _storeContext.CurrentStore.Url);
                    }

                    if (signInResult.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                    {
                        return signInResult.Url;
                    }
                }
            }
            catch (ApiException ex)
            {
                var errMsg = $"Cannot sign-on the user {invoiceItem.RedeemingUserId} into Nexport through SSO using the invoice item {invoiceItem.InvoiceItemId}";
                _logger.Error($"{errMsg}: {ex.Message}", ex);
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
                    orgId.ToString(), userId.ToString(), _storeContext.CurrentStore.Url);

                if (response.ApiErrorEntity.ErrorCode == 0)
                    return response.Url;
            }
            catch (Exception ex)
            {
                var errMsg = $"Cannot sign-on the user {userId} into Nexport organization {orgId} through SSO";
                _logger.Error($"{errMsg}: {ex.Message}", ex);
            }

            return null;
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
                        var invoiceRedemption = _nexportApiService.GetNexportInvoiceRedemption(_nexportSettings.Url, _nexportSettings.AuthenticationToken,
                            orderInvoiceItem.InvoiceItemId.ToString());
                        if (invoiceRedemption.ApiErrorEntity.ErrorCode == 0)
                        {
                            var orgModel = new NexportOrganizationModel
                            {
                                OrgId = Guid.Parse(invoiceRedemption.OrganizationId),
                                OrgName = invoiceRedemption.OrganizationName,
                                OrgShortName = invoiceRedemption.OrganizationShortName
                            };

                            if (!organizationModelList.Exists(i => i.OrgId == orgModel.OrgId))
                            {
                                organizationModelList.Add(orgModel);
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

                    //_productService.UpdateProduct(product);

                    productMapping.IsSynchronized = true;
                    productMapping.UtcLastSynchronizationDate = DateTime.UtcNow;

                    UpdateMapping(productMapping);

                    //_customerActivityService.InsertActivity("EditProduct",
                    //    string.Format(_localizationService.GetResource("ActivityLog.EditProduct"), product.Name),
                    //    product);

                    _logger.Information($"Successfully synchronized product {productMapping.NopProductId} with Nexport using the information from mapping {productMapping.Id}.");
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
    }
}
