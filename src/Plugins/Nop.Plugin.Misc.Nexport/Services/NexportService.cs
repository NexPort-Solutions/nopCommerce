using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
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
using Nop.Plugin.Misc.Nexport.Models;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public partial class NexportService
    {
        #region Fields

        private readonly NexportPluginObjectContext _context;
        private readonly NexportSettings _nexportSettings;
        private readonly IAclService _aclService;
        private readonly ICacheManager _cacheManager;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<NexportProductMapping> _nexportProductRepository;
        private readonly IRepository<NexportOrderProcessingQueueItem> _nexportOrderProcessingQueueRepository;
        private readonly IRepository<NexportOrderInvoiceItem> _nexportOrderInvoiceItemRepository;
        private readonly IRepository<NexportUserMapping> _nexportUserMappingRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public NexportService(
            NexportPluginObjectContext context,
            NexportSettings nexportSettings,
            IAclService aclService,
            ICacheManager cacheManager,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            IProductService productService,
            IRepository<AclRecord> aclRepository,
            IRepository<Product> productRepository,
            IRepository<NexportProductMapping> nexportProduct,
            IRepository<NexportOrderProcessingQueueItem> nexportOrderProcessingQueueRepository,
            IRepository<NexportOrderInvoiceItem> nexportOrderInvoiceItemRepository,
            IRepository<NexportUserMapping> nexportUserMappingRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger logger)
        {
            _context = context;
            _nexportSettings = nexportSettings;
            _aclService = aclService;
            _cacheManager = cacheManager;
            _dataProvider = dataProvider;
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _productService = productService;
            _aclRepository = aclRepository;
            _productRepository = productRepository;
            _nexportProductRepository = nexportProduct;
            _nexportOrderProcessingQueueRepository = nexportOrderProcessingQueueRepository;
            _nexportOrderInvoiceItemRepository = nexportOrderInvoiceItemRepository;
            _nexportUserMappingRepository = nexportUserMappingRepository;
            _storeMappingRepository = storeMappingRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
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

                var response = NexportApiService.AuthenticateNexportApi(model.Url, model.Username, model.Password, tokenExpiration);

                _nexportSettings.AuthenticationToken = response.AccessToken;
                _nexportSettings.Url = model.Url;
                _nexportSettings.UtcExpirationDate = tokenExpiration;

                _settingService.SaveSetting(_nexportSettings);

                _settingService.ClearCache();

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
                _notificationService.ErrorNotification(e.ErrorContent);
            }
        }

        public JsonResult GenerateOrganizationList(string searchTerm, int? page = null)
        {
            try
            {
                var result = NexportApiService.GetNexportOrganization(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, searchTerm, page);
                return new JsonResult(result.OrganizationList.ToList());
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return null;
        }

        public IList<OrganizationResponseItem> FindAllOrganizations()
        {
            var items = new List<OrganizationResponseItem>();

            if(!_nexportSettings.RootOrganizationId.HasValue)
                throw new NullReferenceException("Root organization has not been set");

            try
            {
                var page = 0;
                var remainderItemsCount = 0;
                do
                {
                    var result = NexportApiService.GetNexportOrganizations(_nexportSettings.Url,
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
                        var result = NexportApiService.GetNexportCatalogs(_nexportSettings.Url,
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

        public CatalogResponseItem FindCatalogDetails(Guid catalogId)
        {
            CatalogResponseItem result = null;

            try
            {
                result = NexportApiService.GetNexportCatalogDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public GetDescriptionResponse FindCatalogDescription(Guid catalogId)
        {
            GetDescriptionResponse result = null;

            try
            {
                result = NexportApiService.GetNexportCatalogDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, catalogId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public GetCatalogCreditHoursResponse FindCatalogCreditHours(Guid catalogId)
        {
            GetCatalogCreditHoursResponse result = null;

            try
            {
                result = NexportApiService.getNexportCatalogCreditHours(_nexportSettings.Url,
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
                        var result = NexportApiService.GetNexportSyllabuses(_nexportSettings.Url,
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

        public SectionResponse FindSectionDetails(Guid sectionId)
        {
            SectionResponse result = null;

            try
            {
                result = NexportApiService.GetNexportSectionDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public GetDescriptionResponse FindSectionDescription(Guid sectionId)
        {
            GetDescriptionResponse result = null;

            try
            {
                result = NexportApiService.GetNexportSectionDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public GetObjectivesResponse FindSectionObjectives(Guid sectionId)
        {
            GetObjectivesResponse result = null;

            try
            {
                result = NexportApiService.GetNexportSectionObjectives(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, sectionId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public TrainingPlanResponse FindTrainingPlanDetails(Guid trainingPlanId)
        {
            TrainingPlanResponse result = null;

            try
            {
                result = NexportApiService.GetNexportTrainingPlanDetails(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, trainingPlanId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        public GetDescriptionResponse FindTrainingPlanDescription(Guid trainingPlanId)
        {
            GetDescriptionResponse result = null;

            try
            {
                result = NexportApiService.GetNexportTrainingPlanDescription(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, trainingPlanId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return result;
        }

        [NotNull]
        public string BeginNexportOrderInvoiceTransaction(Guid orgId, Guid purchasingAgentId)
        {
            try
            {
                var beginOrderResult = NexportApiService.BeginNexportInvoiceTransaction(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, orgId, purchasingAgentId);

                return beginOrderResult.InvoiceId;
            }
            catch (ApiException e)
            {
                _logger.Error("Fail to create Nexport order invoice transaction", e);
                throw;
            }
        }

        public string AddItemToNexportOrderInvoice(string invoiceId, Guid nexportProductId,
            CreateInvoiceItemRequest.ProductTypeEnum productType, decimal productCost,
            Guid subscriptionOrgId, string note = null)
        {
            AddInvoiceItemResponse addInvoiceItemResult = null;

            try
            {
                addInvoiceItemResult = NexportApiService.AddNexportInvoiceItem(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId, nexportProductId,
                    productType, subscriptionOrgId, productCost, note);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return addInvoiceItemResult?.InvoiceItemId;
        }

        public void CommitNexportOrderInvoiceTransaction(string invoiceId)
        {
            try
            {
                NexportApiService.CommitNexportInvoiceTransaction(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId);
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }
        }

        public void AddPaymentToNexportOrderInvoice(string invoiceId, decimal totalCost, Guid payeeId, int nopOrderId, DateTime dueDate)
        {
            if (!_nexportSettings.MerchantAccountId.HasValue)
            {
                throw new Exception("Merchant account is empty. Cannot processing payment without a merchant account.");
            }

            try
            {
                NexportApiService.AddNexportInvoicePayment(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceId, totalCost, _nexportSettings.MerchantAccountId.Value,
                    payeeId, InvoicePaymentRequest.PaymentProcessorEnum.NopCommercePlugin,
                    nopOrderId.ToString(), dueDate, note: "Payment for NopCommerce order");
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
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
                var redeemInvoiceResult = NexportApiService.RedeemNexportInvoice(_nexportSettings.Url,
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
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }
        }

        public string SignInNexport(NexportOrderInvoiceItem invoiceItem)
        {
            if (invoiceItem == null)
                throw new ArgumentNullException(nameof(invoiceItem));

            try
            {
                var redemption = NexportApiService.GetNexportInvoiceRedemption(_nexportSettings.Url,
                    _nexportSettings.AuthenticationToken, invoiceItem.InvoiceItemId.ToString());
                if (redemption.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                {
                    SsoResponse signInResult;
                    if (invoiceItem.RedemptionEnrollmentId == null)
                    {
                        signInResult = NexportApiService.NexportSingleSignOn(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, redemption.OrganizationId,
                            redemption.RedemptionUserId, _storeContext.CurrentStore.Url);
                    }
                    else
                    {
                        signInResult = NexportApiService.NexportClassroomSingleSignOn(_nexportSettings.Url,
                            _nexportSettings.AuthenticationToken, redemption.RedemptionEnrollmentId, _storeContext.CurrentStore.Url);
                    }

                    if (signInResult.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.NoError)
                    {
                        return signInResult.Url;
                    }
                }
            }
            catch (ApiException e)
            {
                _logger.Error(e.Message);
            }

            return null;
        }
    }
}
