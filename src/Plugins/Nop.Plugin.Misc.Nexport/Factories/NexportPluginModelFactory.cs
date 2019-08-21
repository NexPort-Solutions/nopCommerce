using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Factories
{
    public class NexportPluginModelFactory : INexportPluginModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IMeasureService _measureService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        private readonly NexportService _nexportService;

        #endregion

        #region Constructor

        public NexportPluginModelFactory(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            IOrderService orderService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager cacheManager,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            NexportService nexportService)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _cacheManager = cacheManager;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _manufacturerService = manufacturerService;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _orderService = orderService;
            _pictureService = pictureService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _settingModelFactory = settingModelFactory;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _nexportService = nexportService;
        }

        #endregion

        public virtual NexportProductMappingSearchModel PrepareNexportProductMappingSearchModel(NexportProductMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public MapProductToNexportProductListModel PrepareMapProductToNexportProductListModel(
            NexportProductMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                //categoryIds: new List<int> { searchModel.SearchCategoryId },
                //manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                //vendorId: searchModel.SearchVendorId,
                //productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var nexportMappings = _nexportService.GetProductMappings();

            // Prepare grid model
            var model = new MapProductToNexportProductListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Where(product => nexportMappings.All(m => m.NopProductId != product.Id)).Select(product =>
                {
                    // Fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        public NexportProductMappingListModel PrepareNexportProductMappingListModel(
            NexportProductMappingSearchModel searchModel, Guid nexportProductId, NexportProductTypeEnum nexportProductType)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var mappings = _nexportService.GetProductMappings(nexportProductId, nexportProductType,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            // Prepare grid model
            var model = new NexportProductMappingListModel().PrepareToGrid(searchModel, mappings, () =>
            {
                return mappings.Select(mapping =>
                {
                    // Fill in model values from the entity
                    var mappingModel = mapping.ToModel<NexportProductMappingModel>();

                    return mappingModel;
                });
            });

            return model;
        }

        public NexportProductGroupMembershipMappingListModel PrepareNexportProductMappingGroupMembershipListModel(
            NexportProductGroupMembershipMappingSearchModel searchModel, int nexportProductMappingId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var groupMembershipMappings = _nexportService.GetProductGroupMembershipMappings(nexportProductMappingId,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = new NexportProductGroupMembershipMappingListModel().PrepareToGrid(searchModel, groupMembershipMappings, () =>
            {
                return groupMembershipMappings.Select(mapping =>
                {
                    var mappingModel = mapping.ToModel<NexportProductGroupMembershipMappingModel>();

                    return mappingModel;
                });
            });

            return model;
        }

        public NexportUserMappingModel PrepareNexportUserMappingModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var model = new NexportUserMappingModel();
            var nexportUserMapping = _nexportService.FindUserMappingByCustomerId(customer.Id);
            if (nexportUserMapping != null)
            {
                model.NexportUserId = nexportUserMapping.NexportUserId;
            }

            return model;
        }

        public NexportCatalogListModel PrepareNexportCatalogListModel(NexportCatalogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var catalogs = _nexportService.FindAllCatalogs(searchModel.OrgId, searchModel.Page - 1, searchModel.PageSize);

            // Prepare grid model
            var model = new NexportCatalogListModel().PrepareToGrid(searchModel, catalogs, () =>
            {
                return catalogs.Select(catalog =>
                {
                    // Fill in model values from the entity
                    var catalogItemModel = new NexportCatalogResponseItemModel()
                    {
                        OrgId = Guid.Parse(catalog.OrgId),
                        CatalogId = Guid.Parse(catalog.CatalogId),
                        IsEnabled = catalog.IsEnabled,
                        Name = catalog.Name,
                        OwnerId = Guid.Parse(catalog.OwnerId),
                        PricingModel = catalog.PricingModel,
                        PublishingModel = catalog.PublishingModel,
                        UtcDateCreated = catalog.DateCreated,
                        UtcDateLastModified = catalog.LastModified,
                        AccessTimeLimit = catalog.AccessTimeLimit
                    };

                    return catalogItemModel;
                });
            });

            return model;
        }

        public NexportSyllabusListModel PrepareNexportSyllabusListMode(NexportSyllabusListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var syllabus = _nexportService.FindAllSyllabuses(searchModel.CatalogId, searchModel.Page - 1, searchModel.PageSize);

            // Prepare grid model
            var model = new NexportSyllabusListModel().PrepareToGrid(searchModel, syllabus, () =>
            {
                return syllabus.Select(syllabi =>
                {
                    // Fill in model values from the entity
                    var syllabiId = Guid.Parse(syllabi.SyllabusId);
                    var syllabiItemModel = new NexportSyllabiResponseItemModel()
                    {
                        SyllabusId = syllabiId,
                        Name = syllabi.SyllabusName,
                        Type = syllabi.SyllabusType,
                        ProductId = Guid.Parse(syllabi.ProductId),
                        TotalMappings = _nexportService.FindMappingCountPerSyllabi(syllabiId)
                    };

                    return syllabiItemModel;
                });
            });

            return model;
        }

        public NexportLoginModel PrepareNexportLoginModel(bool? checkoutAsGuest)
        {
            return new NexportLoginModel()
            {
                UsernamesEnabled = true,
                RegistrationType = _customerSettings.UserRegistrationType,
                CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage
            };
        }

        public NexportTrainingListModel PrepareNexportTrainingListModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof (customer));

            var userMapping = _nexportService.FindUserMappingByCustomerId(customer.Id);
            var redemptionOrganizations = _nexportService.FindNexportRedemptionOrganizationsByCustomerId(customer.Id);

            var model = new NexportTrainingListModel()
            {
                RedemptionOrganizations = redemptionOrganizations,
                UserId = userMapping.NexportUserId
            };

            return model;
        }
    }
}
