using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.Products;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.Plugins;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Models.Enrollment;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Media;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;
using Nop.Core.Infrastructure.Mapper;

namespace Nop.Plugin.Misc.Nexport.Factories;

public class NexportPluginModelFactory : INexportPluginModelFactory
{
    #region Fields

    private readonly NexportSettings _nexportSettings;
    private readonly CatalogSettings _catalogSettings;
    private readonly CurrencySettings _currencySettings;
    private readonly IRepository<GenericAttribute> _genericAttributeRepository;
    private readonly IRepository<LocaleStringResource> _localeStringResourceRepository;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ICategoryService _categoryService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IDiscountService _discountService;
    private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
    private readonly IDownloadService _downloadService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IMeasureService _measureService;
    private readonly IOrderService _orderService;
    private readonly IReturnRequestService _returnRequestService;
    private readonly IReturnRequestModelFactory _returnRequestModelFactory;
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
    private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly MeasureSettings _measureSettings;
    private readonly TaxSettings _taxSettings;
    private readonly VendorSettings _vendorSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly CaptchaSettings _captchaSettings;
    private readonly ILogger _logger;
    private readonly ICountryService _countryService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly ISettingService _settingService;
    private readonly NopHttpClient _nopHttpClient;
    private readonly AddressSettings _addressSettings;
    private readonly NexportService _nexportService;
    private readonly INexportWholesaleService _nexportWholesaleService;
    private readonly IAddressService _addressService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly NexportPluginService _pluginService;
    private readonly IPermissionService _permissionService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;

    #endregion

    #region Constructor

    public NexportPluginModelFactory(
        NexportSettings nexportSettings,
        CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IRepository<GenericAttribute> genericAttributeRepository,
        IRepository<LocaleStringResource> localeStringResourceRepository,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IDiscountService discountService,
        IDiscountSupportedModelFactory discountSupportedModelFactory,
        IDownloadService downloadService,
        ILocalizationService localizationService,
        ILocalizedModelFactory localizedModelFactory,
        IGenericAttributeService genericAttributeService,
        IManufacturerService manufacturerService,
        IMeasureService measureService,
        IReturnRequestService returnRequestService,
        IReturnRequestModelFactory returnRequestModelFactory,
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
        IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
        IWorkContext workContext,
        IStoreContext storeContext,
        MeasureSettings measureSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        CustomerSettings customerSettings,
        CaptchaSettings captchaSettings,
        ILogger logger,
        NexportService nexportService,
        INexportWholesaleService nexportWholesaleService,
        NexportPluginService pluginService,
        ICountryService countryService,
        IPaymentPluginManager paymentPluginManager,
        ISettingService settingService,
        NopHttpClient nopHttpClient,
        AddressSettings addressSettings,
        IAddressService addressService,
        IPermissionService permissionService,
        IPriceFormatter priceFormatter,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IStaticCacheManager staticCacheManager)
    {
        _nexportSettings = nexportSettings;
        _catalogSettings = catalogSettings;
        _currencySettings = currencySettings;
        _genericAttributeRepository = genericAttributeRepository;
        _localeStringResourceRepository = localeStringResourceRepository;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _baseAdminModelFactory = baseAdminModelFactory;
        _cacheManager = cacheManager;
        _categoryService = categoryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _discountService = discountService;
        _discountSupportedModelFactory = discountSupportedModelFactory;
        _downloadService = downloadService;
        _localizationService = localizationService;
        _localizedModelFactory = localizedModelFactory;
        _genericAttributeService = genericAttributeService;
        _manufacturerService = manufacturerService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _returnRequestService = returnRequestService;
        _returnRequestModelFactory = returnRequestModelFactory;
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
        _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
        _workContext = workContext;
        _storeContext = storeContext;
        _taxSettings = taxSettings;
        _vendorSettings = vendorSettings;
        _customerSettings = customerSettings;
        _captchaSettings = captchaSettings;
        _logger = logger;
        _nexportService = nexportService;
        _nexportWholesaleService = nexportWholesaleService;
        _countryService = countryService;
        _paymentPluginManager = paymentPluginManager;
        _settingService = settingService;
        _nopHttpClient = nopHttpClient;
        _addressSettings = addressSettings;
        _addressService = addressService;
        _priceFormatter = priceFormatter;
        _pluginService = pluginService;
        _permissionService = permissionService;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    public virtual async Task<NexportPluginResourceListModel> PrepareNexportPluginResourceListModelAsync(NexportPluginResourceListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var results = await _pluginService.GetConflictedLocalizedResourcesAsync();

        var resources = new PagedList<LocaleStringResource>(results, searchModel.Page - 1, searchModel.PageSize);

        var model = await new NexportPluginResourceListModel().PrepareToGridAsync(searchModel, resources, () =>
        {
            return resources.SelectAwait(async resource =>
            {
                var localeResourceModel = new LocaleResourceModel()
                {
                    Id = resource.Id,
                    ResourceValue = resource.ResourceValue,
                    ResourceName = resource.ResourceName,
                    LanguageId = resource.LanguageId
                };

                return localeResourceModel;
            });
        });

        return model;
    }

    public virtual async Task<NexportProductMappingModel> PrepareNexportProductMappingModelAsync(NexportProductMapping productMapping, bool isEditable)
    {
        var model = productMapping.ToModel<NexportProductMappingModel>();
        model.Editable = isEditable;
        if (model.StoreId != null)
        {
            model.StoreName = await _nexportService.GetStoreNameAsync(model.StoreId.Value);
        }

        if (model.NexportSyllabusId != null)
        {
            if (model.Type == NexportProductTypeEnum.Section)
            {
                var sectionDetails = await _nexportService.GetSectionDetailsAsync(model.NexportSyllabusId.Value);
                model.SectionNumber = sectionDetails?.SectionNumber;
                model.UniqueName = sectionDetails?.UniqueName;
            }
            else if (model.Type == NexportProductTypeEnum.TrainingPlan)
            {
                var trainingPlanDetails = await _nexportService.GetTrainingPlanDetailsAsync(model.NexportSyllabusId.Value);
                model.UniqueName = trainingPlanDetails?.UniqueName;
            }
        }

        if (model.NexportCatalogId != Guid.Empty)
        {
            model.NexportCatalogName = (await _nexportService.GetCatalogDetailsAsync(productMapping.NexportCatalogId)).Name;
        }

        model.SupplementalInfoQuestionIds =
            (await _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id))
                .Select(x => x.QuestionId).ToList();

        var availableSupplementalInfoQuestions = await _nexportService.GetSupplementalInfoQuestionList();
        foreach (var questionItem in availableSupplementalInfoQuestions)
        {
            model.AvailableSupplementalInfoQuestions.Add(questionItem);
        }

        foreach (var questionItem in model.AvailableSupplementalInfoQuestions)
        {
            questionItem.Selected = int.TryParse(questionItem.Value, out var questionId) &&
                                    model.SupplementalInfoQuestionIds.Contains(questionId);
        }

        if (model.NopCategoryId != null)
            model.NopCategoryName = (await _categoryService.GetCategoryByIdAsync(model.NopCategoryId.Value)).Name;

        return model;
    }

    /// <summary>
    /// Prepare product search model to add to the order
    /// </summary>
    /// <param name="searchModel">Product search model to add to the order</param>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product search model to add to the order
    /// </returns>
    public virtual async Task<NexportProductMappingListSearchModel> PrepareNexportProductMappingListSearchModelAsync(NexportProductMappingListSearchModel searchModel, ProductModel productModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (productModel == null)
            throw new ArgumentNullException(nameof(productModel));

        searchModel.NopProductId = productModel.Id;

        //prepare available product types
        searchModel.AvailableNexportProductTypes.Add(new SelectListItem { Value = null, Text = "All" });

        //for each nexportproducttype enum create a selectlistitem and add it for the product type filter
        foreach (var e in Enum.GetValues(typeof(NexportProductTypeEnum)))
        {
            searchModel.AvailableNexportProductTypes.Add(new SelectListItem
            {
                Value = e.ToString(),
                Text = e.GetDisplayName()
            });
        }

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<NexportProductMappingListModel> PrepareNexportProductMappingListModelAsync(
        NexportProductMappingListSearchModel searchModel, int nopProductId)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var mappings = await _nexportService.GetAllNexportProductMappingsAsync(searchProductName: searchModel.SearchNexportProductName,
            searchProductType: searchModel.NexportProductType,
            searchStoreName: searchModel.SearchStoreName,
            productId: nopProductId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        // Prepare grid model
        var model = await new NexportProductMappingListModel().PrepareToGridAsync(searchModel, mappings, () =>
        {
            return mappings.SelectAwait(async mapping =>
            {
                // Fill in model values from the entity
                var mappingModel = mapping.ToModel<NexportProductMappingModel>();
                if (mappingModel.StoreId.HasValue)
                {
                    mappingModel.StoreName = await _nexportService.GetStoreNameAsync(mappingModel.StoreId.Value);
                }

                if (mappingModel.NexportSyllabusId != null)
                {
                    if (mappingModel.Type == NexportProductTypeEnum.Section)
                    {
                        var sectionDetails = await _nexportService.GetSectionDetailsAsync(mappingModel.NexportSyllabusId.Value);
                        mappingModel.SectionNumber = sectionDetails?.SectionNumber;
                        mappingModel.UniqueName = sectionDetails?.UniqueName;
                    }
                    else if (mappingModel.Type == NexportProductTypeEnum.TrainingPlan)
                    {
                        var trainingPlanDetails = await _nexportService.GetTrainingPlanDetailsAsync(mappingModel.NexportSyllabusId.Value);
                        mappingModel.UniqueName = trainingPlanDetails?.UniqueName;
                    }
                }

                if (mappingModel.NexportCatalogId != Guid.Empty)
                {
                    mappingModel.NexportCatalogName = (await _nexportService.GetCatalogDetailsAsync(mapping.NexportCatalogId)).Name;
                }
                var groupMemberships =
                    await _nexportService.GetProductGroupMembershipMappings(mappingModel.Id);
                foreach (var groupMembership in groupMemberships)
                {
                    var groupMembershipModel = groupMembership.ToModel<NexportProductGroupMembershipMappingModel>();
                    mappingModel.GroupMembershipMappingModels.Add(groupMembershipModel);
                }

                if (mappingModel.NopCategoryId != null)
                    mappingModel.NopCategoryName = (await _categoryService.GetCategoryByIdAsync(mappingModel.NopCategoryId.Value)).Name;

                return mappingModel;
            });
        });

        return model;
    }

    public virtual async Task<NexportProductMappingListModel> PrepareNexportCategoryProductMappingListModelAsync(
       NexportCategoryProductMappingListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var mappings =
            await _nexportService.GetAllNexportProductMappingsByCategoryIdAsync(searchModel.NopCategoryId,
                searchModel.Page - 1, searchModel.PageSize);

        // Prepare grid model
        var model = await new NexportProductMappingListModel().PrepareToGridAsync(searchModel, mappings, () =>
        {
            return mappings.SelectAwait(async mapping =>
            {
                // Fill in model values from the entity
                var mappingModel = mapping.ToModel<NexportProductMappingModel>();

                return mappingModel;
            });
        });

        return model;
    }

    public virtual async Task<NexportProductGroupMembershipMappingListModel>
        PrepareNexportProductMappingGroupMembershipListModelAsync(
            NexportProductGroupMembershipMappingListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var groupMembershipMappings =
            await _nexportService.GetProductGroupMembershipMappingsPagination(searchModel.NexportProductMappingId,
                searchModel.Page - 1, searchModel.PageSize);

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

    public async Task<DuplicateNexportProductMappingModel> PrepareDuplicateNexportProductMappingModel(Product product)
    {
        var model = new DuplicateNexportProductMappingModel();

        var defaultMapping = await _nexportService.GetProductMappingByNopProductId(product.Id);
        if (defaultMapping == null)
            return model;

        model.AvailableStores.Add(new SelectListItem
        {
            Text = "Default",
            Value = ""
        });

        var availableStores = await _storeService.GetAllStoresAsync();
        foreach (var store in availableStores)
        {
            var mapping = await _nexportService.GetProductMappingByNopProductId(product.Id, store.Id);
            if (mapping != null)
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString()
                });
            }
            else
            {
                model.DestinationStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString()
                });
            }
        }

        return model;
    }

    public virtual async Task<NexportCustomerAdditionalInfoModel> PrepareNexportAdditionalInfoModelAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        var model = new NexportCustomerAdditionalInfoModel { CustomerId = customer.Id };
        var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
        if (nexportUserMapping != null)
        {
            model.NexportUserId = nexportUserMapping.NexportUserId;
        }

        model.NexportSupplementalInfoAnswerListSearchModel = new NexportSupplementalInfoAnswerListSearchModel
        {
            CustomerId = customer.Id,
        };

        await _baseAdminModelFactory.PrepareStoresAsync(model.NexportSupplementalInfoAnswerListSearchModel.AvailableStores);

        model.NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel = new NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel
        {
            CustomerId = customer.Id
        };

        var availableStores = await _storeService.GetAllStoresAsync();

        model.NexportCustomerRegistrationFieldWithAnswersListSearchModel = new NexportCustomerRegistrationFieldWithAnswersListSearchModel
        {
            CustomerId = customer.Id,
            AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString()
            }).ToList()
        };

        model.NexportCustomerRegistrationFieldAnswerListSearchModel = new NexportCustomerRegistrationFieldAnswerListSearchModel
        {
            CustomerId = customer.Id
        };

        return model;
    }

    public async Task<AddNexportCustomerAdditionalInfoModel> PrepareAddNexportAdditionalInfoModel(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        var model = new AddNexportCustomerAdditionalInfoModel { CustomerId = customer.Id };

        var availableStores = await _storeService.GetAllStoresAsync();
        model.AvailableStores = availableStores.Select(store => new SelectListItem
        {
            Text = store.Name,
            Value = store.Id.ToString()
        }).ToList();

        model.AvailableStores.Insert(0, new SelectListItem("Select store", ""));

        return model;
    }

    public virtual async Task<NexportCatalogListModel> PrepareNexportCatalogListModelAsync(NexportCatalogSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var catalogs =
            await _nexportService.FindAllCatalogsAsync(searchModel.OrgId, searchModel.Page - 1, searchModel.PageSize);

        // Prepare grid model
        var model = new NexportCatalogListModel().PrepareToGrid(searchModel, catalogs, () =>
        {
            return catalogs.Select(catalog =>
            {
                // Fill in model values from the entity
                var catalogItemModel = new NexportCatalogResponseItemModel()
                {
                    OrgId = catalog.OrgId,
                    CatalogId = catalog.CatalogId,
                    IsEnabled = catalog.IsEnabled,
                    Name = catalog.Name,
                    OwnerName = catalog.OwnerName,
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

    public virtual async Task<NexportSyllabusListModel> PrepareNexportSyllabusListModelAsync(NexportSyllabusListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var syllabus = await _nexportService.FindAllSyllabusesAsync(searchModel.CatalogId, searchModel.Page - 1, searchModel.PageSize);

        // Prepare grid model
        var model = await new NexportSyllabusListModel().PrepareToGridAsync(searchModel, syllabus, () =>
        {
            return syllabus.SelectAwait(async syllabi =>
            {
                // Fill in model values from the entity
                var syllabiId = syllabi.SyllabusId;
                var syllabiItemModel = new NexportSyllabiResponseItemModel()
                {
                    CatalogId = searchModel.CatalogId,
                    SyllabusId = syllabiId,
                    Name = syllabi.SyllabusName,
                    Type = syllabi.SyllabusType,
                    ProductId = syllabi.ProductId,
                    TotalMappings = await _nexportService.FindMappingCountPerSyllabi(syllabiId)
                };

                if (syllabi.SyllabusType == GetSyllabiResponseItem.SyllabusTypeEnum.Section)
                {
                    var sectionDetails = await _nexportService.GetSectionDetailsAsync(syllabi.SyllabusId);
                    if (sectionDetails != null)
                    {
                        syllabiItemModel.UniqueName = sectionDetails.UniqueName;
                        syllabiItemModel.SectionNumber = sectionDetails.SectionNumber;
                    }
                }
                else if (syllabi.SyllabusType == GetSyllabiResponseItem.SyllabusTypeEnum.TrainingPlan)
                {
                    var trainingPlanDetails = await _nexportService.GetTrainingPlanDetailsAsync(syllabi.SyllabusId);
                    if (trainingPlanDetails != null)
                    {
                        syllabiItemModel.UniqueName = trainingPlanDetails.UniqueName;
                    }
                }

                return syllabiItemModel;
            });
        });

        return model;
    }

    public virtual Task<NexportLoginModel> PrepareNexportLoginModelAsync(bool? checkoutAsGuest)
    {
        return Task.FromResult(new NexportLoginModel
        {
            UsernamesEnabled = false,
            RegistrationType = _customerSettings.UserRegistrationType,
            CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
            DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage
        });
    }

    public virtual async Task<NexportTrainingListModel> PrepareNexportTrainingListModelAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
        var redemptionOrganizations = await _nexportService.FindNexportRedemptionOrganizationsByCustomerId(customer.Id);

        var model = new NexportTrainingListModel();

        if (userMapping != null)
        {
            model.UserId = userMapping.NexportUserId;
            model.RedemptionOrganizations = redemptionOrganizations;

            var customerOrderInvoices = (await _nexportService.GetNexportOrderInvoiceItems(userMapping.NexportUserId))
                .Where(x => x.InvoiceItemId != Guid.Empty)
                .GroupBy(x => x.RedemptionEnrollmentId)
                .Select(x => x.OrderByDescending(invoice => invoice.UtcDateRedemption).First())
                .OrderByDescending(x => x.UtcDateRedemption)
                .ToList();

            var trainingList = new List<NexportTrainingItemModel>();
            foreach (var orderInvoice in customerOrderInvoices)
            {
                try
                {
                    var nexportInvoiceDetails =
                        await _nexportService.GetNexportInvoiceRedemptionAsync(orderInvoice.InvoiceItemId);
                    if (nexportInvoiceDetails?.UtcRedemptionDate != null)
                    {
                        DateTime? enrollmentStartDate = null;
                        DateTime? enrollmentExpirationDate = null;
                        var enrollmentStatus = Enums.PhaseEnum.NotStarted;
                        if (nexportInvoiceDetails.RedemptionUserId != null)
                        {
                            var enrollmentExisted = false;

                            try
                            {
                                if (nexportInvoiceDetails.SyllabusId.HasValue)
                                {
                                    if (nexportInvoiceDetails.RedemptionType is null or InvoiceRedemptionResponse.RedemptionTypeEnum.Section)
                                    {
                                        var enrollmentDetails = await _nexportService.GetSectionEnrollmentDetailsAsync(
                                            nexportInvoiceDetails.OrganizationId,
                                            nexportInvoiceDetails.RedemptionUserId.Value, nexportInvoiceDetails.SyllabusId.Value);
                                        if (enrollmentDetails != null)
                                        {
                                            enrollmentExisted = true;
                                            enrollmentStartDate = enrollmentDetails.EnrollmentDate;
                                            enrollmentExpirationDate = enrollmentDetails.ExpirationDate;
                                            enrollmentStatus = enrollmentDetails.Phase;
                                        }
                                    }
                                    else if (nexportInvoiceDetails.RedemptionType == InvoiceRedemptionResponse.RedemptionTypeEnum.TrainingPlan)
                                    {
                                        var enrollmentDetails = await _nexportService.GetTrainingPlanEnrollmentDetailsAsync(
                                            nexportInvoiceDetails.OrganizationId,
                                            nexportInvoiceDetails.RedemptionUserId.Value, nexportInvoiceDetails.SyllabusId.Value);
                                        if (enrollmentDetails != null)
                                        {
                                            enrollmentExisted = true;
                                            enrollmentStartDate = enrollmentDetails.EnrollmentDate;
                                            enrollmentExpirationDate = enrollmentDetails.ExpirationDate;
                                            enrollmentStatus = enrollmentDetails.Phase;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                await _logger.WarningAsync($"Unable to get syllabus details for syllabus {nexportInvoiceDetails.SyllabusId}", ex);
                            }

                            if (enrollmentExisted)
                            {
                                if (nexportInvoiceDetails.SyllabusId.HasValue)
                                {
                                    var trainingItem = new NexportTrainingItemModel
                                    {
                                        Name = nexportInvoiceDetails.SyllabusTitle,
                                        Type = nexportInvoiceDetails.RedemptionType ?? InvoiceRedemptionResponse.RedemptionTypeEnum.Section,
                                        UtcStartDate = enrollmentStartDate,
                                        UtcExpirationDate = enrollmentExpirationDate,
                                        UtcRedemptionDate = nexportInvoiceDetails.UtcRedemptionDate,
                                        EnrollmentId = nexportInvoiceDetails.RedemptionEnrollmentId,
                                        SyllabusId = nexportInvoiceDetails.SyllabusId.Value,
                                        OrganizationId = nexportInvoiceDetails.OrganizationId,
                                        Status = enrollmentStatus
                                    };

                                    if (!trainingList.Any(x => x.SyllabusId == nexportInvoiceDetails.SyllabusId))
                                        trainingList.Add(trainingItem);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _logger.WarningAsync($"Unable to get Nexport invoice item {orderInvoice.InvoiceItemId}", ex, customer);
                }
            }

            model.Trainings = trainingList
                .GroupBy(x => x.OrganizationId)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToList());

            var nexportOrgs = await _nexportService.FindAllOrganizationsForUserAsync(userMapping.NexportUserId);
            model.Organizations = nexportOrgs.ToList();
        }

        return model;
    }

    public virtual async Task<NexportEnrollmentListModel> PrepareNexportEnrollmentListModelAsync(NexportEnrollmentListSearchModel searchModel)
    {
        var sectionEnrollments = await _nexportService.FindSectionEnrollmentsAsync(
            searchModel.UserId, searchModel.OrganizationId, searchModel.Page - 1, searchModel.PageSize);

        var model = await new NexportEnrollmentListModel().PrepareToGridAsync(searchModel,
            sectionEnrollments, () =>
            {
                var enrollments = sectionEnrollments.SelectAwait(async item =>
                {
                    var enrollmentModel = new NexportEnrollmentResponseItemModel
                    {
                        Id = item.EnrollmentId,
                        Name = item.Title,
                        EnrollmentDate = item.EnrollmentDate,
                        ExpirationDate = item.ExpirationDate,
                        HasExpired = item.ExpirationDate != null && item.ExpirationDate < DateTime.UtcNow,
                        IsExpiringSoon = item.ExpirationDate != null && (item.ExpirationDate.Value - DateTime.UtcNow).TotalDays < 30,
                        StartDate = item.StartDate,
                        SyllabusId = item.SyllabusId,
                        LastActivityDate = item.LastActivityDate,
                        Status = item.Phase,
                    };

                    if (item.Phase == Enums.PhaseEnum.Finished)
                    {
                        var certResult = await _nexportService.GetNexportEnrollmentCertificateUrl(item.EnrollmentId)!;
                        if (certResult is { CertificateReady: true })
                        {
                            enrollmentModel.HasCertificate = true;
                            enrollmentModel.CertificateUrl = certResult.CertificateUrl;
                        }
                    }

                    var nexportOrderInvoice = await _nexportService.GetNexportOrderInvoiceItem(searchModel.UserId, item.EnrollmentId);

                    if (nexportOrderInvoice != null)
                    {
                        enrollmentModel.IsMarketplacePurchase = true;
                        var orderItem = await _orderService.GetOrderItemByIdAsync(nexportOrderInvoice.OrderItemId);
                        if (orderItem != null)
                        {
                            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                            if (product != null)
                            {
                                var seName = await _urlRecordService.GetSeNameAsync(product);
                                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext!);
                                enrollmentModel.ProductUrl = urlHelper.RouteUrl<Product>(new { SeName = seName });
                            }
                        }
                    }

                    return enrollmentModel;
                });

                return enrollments.OrderByDescending(x => x.LastActivityDate);
            });

        return model;
    }

    public async Task<NexportEnrollmentListSearchModel> PrepareListNexportUserEnrollments(Customer customer, Guid organizationId)
    {
        var searchModel = new NexportEnrollmentListSearchModel();

        var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
        if (nexportUserMapping != null)
        {
            searchModel.UserId = nexportUserMapping.NexportUserId;
            searchModel.OrganizationId = organizationId;
        }

        return searchModel;
    }

    public async Task<NexportCustomerSupplementalInfoAnswersModel>
        PrepareNexportCustomerSupplementalInfoAnswersModelAsync(Customer customer, Store store)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var model = new NexportCustomerSupplementalInfoAnswersModel();

        var answers = await _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id);
        var questionIds = answers.Select(a => a.QuestionId).Distinct().ToList();

        foreach (var questionId in questionIds)
        {
            var answerWithOptionsList = answers.Where(a => a.QuestionId == questionId);
            var answerWithOptionsDictionary = answerWithOptionsList.ToDictionary(
                answerAndOption => answerAndOption.Id,
                answerAndOption => answerAndOption.OptionId);
            model.QuestionWithAnswersList.Add(questionId, answerWithOptionsDictionary);
        }

        return model;
    }

    public async Task<NexportCustomerSupplementalInfoAnswerEditModel>
        PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(Customer customer, Store store,
            NexportSupplementalInfoQuestion question)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (store == null)
            throw new ArgumentNullException(nameof(store));

        if (question == null)
            throw new ArgumentNullException(nameof(question));

        var currentAnswers = (await _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id, question.Id))
            .Select(currentAnswer => new EditSupplementInfoAnswerRequest
            {
                AnswerId = currentAnswer.Id,
                OptionId = currentAnswer.OptionId
            }).ToList();

        var model = new NexportCustomerSupplementalInfoAnswerEditModel
        {
            Question = question,
            Options = await _nexportService.GetNexportSupplementalInfoOptionsByQuestionId(question.Id, true),
            Answers = currentAnswers
        };

        return model;
    }

    public async Task<NexportCustomerSupplementalInfoAnsweredQuestionListModel>
        PrepareNexportSupplementalInfoQuestionListModelAsync(
            NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var customerSupplementalInfoAnsweredQuestions = await _nexportService.GetNexportSupplementalInfoAnsweredQuestionsPagination(searchModel.CustomerId,
            searchModel.Page - 1,
            searchModel.PageSize);

        var model = new NexportCustomerSupplementalInfoAnsweredQuestionListModel().PrepareToGrid(searchModel,
            customerSupplementalInfoAnsweredQuestions, () =>
        {
            return customerSupplementalInfoAnsweredQuestions.Select(question =>
            {
                var questionModel = question.ToModel<NexportCustomerSupplementalInfoAnsweredQuestionModel>();

                return questionModel;
            });
        });

        return model;
    }

    public async Task<NexportSupplementalInfoAnswerListModel> PrepareNexportSupplementalInfoAnswerListModelAsync(
        NexportSupplementalInfoAnswerListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var customerSupplementalInfoAnswers = await _nexportService.GetNexportSupplementalInfoAnswersPagination(
            searchModel.CustomerId,
            searchModel.QuestionId,
            searchModel.Page - 1,
            searchModel.PageSize);

        var model = await new NexportSupplementalInfoAnswerListModel().PrepareToGridAsync(searchModel, customerSupplementalInfoAnswers, () =>
        {
            return customerSupplementalInfoAnswers.SelectAwait(async answer =>
            {
                var answerModel = answer.ToModel<NexportSupplementalInfoAnswerModel>();

                answerModel.StoreName = (await _storeService.GetStoreByIdAsync(answer.StoreId)).Name;
                answerModel.OptionText =
                    (await _nexportService.GetNexportSupplementalInfoOptionById(answer.OptionId)).OptionText;
                answerModel.NexportMemberships = (await _nexportService
                    .GetNexportSupplementalInfoAnswerMembershipsByAnswerId(answer.Id))
                    .Select(am => am.NexportMembershipId).ToList();

                return answerModel;
            }).OrderBy(a => a.StoreName);
        });

        return model;
    }

    public virtual Task<NexportSupplementalInfoQuestionSearchModel>
        PrepareNexportSupplementalInfoQuestionSearchModelAsync(NexportSupplementalInfoQuestionSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        // Prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public virtual async Task<NexportSupplementalInfoQuestionListModel>
        PrepareNexportSupplementalInfoQuestionListModelAsync(NexportSupplementalInfoQuestionSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        // Get all available supplemental info questions
        var supplementalInfoQuestions =
            await _nexportService.GetAllNexportSupplementalInfoQuestionsPagination(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        // Prepare the list model
        var model = new NexportSupplementalInfoQuestionListModel().PrepareToGrid(searchModel,
            supplementalInfoQuestions, () =>
            {
                return supplementalInfoQuestions.Select(question =>
                {
                    //fill in model values from the entity
                    var requestModel = question.ToModel<NexportSupplementalInfoQuestionModel>();
                    return requestModel;
                });
            });

        return model;
    }

    public virtual async Task<NexportSupplementalInfoQuestionModel>
        PrepareNexportSupplementalInfoQuestionModelAsync(NexportSupplementalInfoQuestionModel model, NexportSupplementalInfoQuestion question)
    {
        if (question != null)
        {
            model ??= question.ToModel<NexportSupplementalInfoQuestionModel>();

            await PrepareNexportSupplementalInfoOptionSearchModelAsync(model.NexportSupplementalInfoOptionSearchModel, question);
        }

        var availableQuestionTypes = new List<SelectListItem>
            {
                new()
                {
                    Text = NexportSupplementalInfoQuestionType.SingleOption.GetDisplayName(),
                    Value = ((int)NexportSupplementalInfoQuestionType.SingleOption).ToString(),
                    Selected = true
                },
                new()
                {
                    Text = NexportSupplementalInfoQuestionType.MultipleOptions.GetDisplayName(),
                    Value = ((int)NexportSupplementalInfoQuestionType.MultipleOptions).ToString()
                }
            };

        foreach (var type in availableQuestionTypes)
            model.AvailableQuestionTypes.Add(type);

        return model;
    }

    public virtual Task<NexportSupplementalInfoOptionSearchModel>
        PrepareNexportSupplementalInfoOptionSearchModelAsync(NexportSupplementalInfoOptionSearchModel searchModel,
            NexportSupplementalInfoQuestion question)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (question == null)
            throw new ArgumentNullException(nameof(question));

        searchModel.QuestionId = question.Id;

        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public virtual async Task<NexportSupplementalInfoOptionListModel>
        PrepareNexportSupplementalInfoOptionListModelAsync(NexportSupplementalInfoOptionSearchModel searchModel,
            NexportSupplementalInfoQuestion question)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (question == null)
            throw new ArgumentNullException(nameof(question));

        var options = (await _nexportService.GetNexportSupplementalInfoOptionsByQuestionId(question.Id))
            .ToPagedList(searchModel);

        //prepare list model
        var model = new NexportSupplementalInfoOptionListModel().PrepareToGrid(searchModel, options, () =>
        {
            return options.Select(option =>
            {
                //fill in model values from the entity
                var optionModel = option.ToModel<NexportSupplementalInfoOptionModel>();

                return optionModel;
            });
        });

        return model;
    }

    public virtual Task<NexportSupplementalInfoOptionModel> PrepareNexportSupplementalInfoOptionModelAsync(
        NexportSupplementalInfoOptionModel model, NexportSupplementalInfoQuestion question,
        NexportSupplementalInfoOption option)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        if (option != null)
        {
            //fill in model values from the entity
            model ??= option.ToModel<NexportSupplementalInfoOptionModel>();
        }

        model.QuestionId = question.Id;

        return Task.FromResult(model);
    }

    public async Task<NexportSupplementalInfoOptionGroupAssociationListModel>
        PrepareNexportSupplementalInfoOptionGroupAssociationListModelAsync(
            NexportSupplementalInfoOptionGroupAssociationListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var groupAssociations =
            await _nexportService.GetNexportSupplementalInfoOptionGroupAssociationsPagination(searchModel.OptionId,
                searchModel.Page - 1, searchModel.PageSize);

        var model = new NexportSupplementalInfoOptionGroupAssociationListModel().PrepareToGrid(searchModel, groupAssociations, () =>
        {
            return groupAssociations.Select(mapping =>
            {
                var mappingModel = mapping.ToModel<NexportSupplementalInfoOptionGroupAssociationModel>();

                return mappingModel;
            });
        });

        return model;
    }

    public async Task<NexportSupplementalInfoAnswerQuestionModel> PrepareNexportSupplementalInfoAnswerQuestionModelAsync(
        IList<int> questionIds, Customer customer, Store store)
    {
        if (questionIds == null)
            throw new ArgumentNullException(nameof(questionIds));

        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var model = new NexportSupplementalInfoAnswerQuestionModel();

        var questionWithoutAnswerIds = new List<int>();

        var answeredQuestions = new Dictionary<int, string>();

        var answers = await _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id);
        var answered = answers.Where(x => questionIds.Contains(x.QuestionId)).ToList();

        var questionWithAnswerIds = answered.Count > 0
            ? answered.Select(x => x.QuestionId).ToList()
            : new List<int>();

        questionWithoutAnswerIds.AddRange(questionIds.Except(questionWithAnswerIds));

        foreach (var answer in answered.Where(answer => !answeredQuestions.ContainsKey(answer.Id)))
        {
            answeredQuestions.Add(answer.Id, $"{answer.QuestionId},{answer.OptionId}");
        }

        model.QuestionIds = questionIds.Distinct().ToList();
        model.QuestionWithoutAnswerIds = questionWithoutAnswerIds;

        return model;
    }

    public async Task<NexportCustomerAdditionalSettingsModel> PrepareNexportCustomerAdditionalSettingsModelAsync()
    {
        var model = new NexportCustomerAdditionalSettingsModel();

        model.NexportRegistrationFieldCategorySearchModel.SetGridPageSize();
        model.NexportRegistrationFieldSearchModel =
            await PrepareNexportRegistrationFieldSearchModelAsync(model.NexportRegistrationFieldSearchModel);

        return model;
    }

    public async Task<NexportRegistrationFieldSearchModel> PrepareNexportRegistrationFieldSearchModelAsync(NexportRegistrationFieldSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare all the stores for the store search filter
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<NexportRegistrationFieldCategoryListModel> PrepareNexportRegistrationFieldCategoryListModelAsync(
        NexportRegistrationFieldCategorySearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var registrationFields =
            await _nexportService.GetNexportRegistrationFieldCategoriesPagination(searchModel.Page - 1, searchModel.PageSize);

        var model = new NexportRegistrationFieldCategoryListModel().PrepareToGrid(searchModel,
            registrationFields, () =>
            {
                return registrationFields.Select(field =>
                {
                    var fieldModel = field.ToModel<NexportRegistrationFieldCategoryModel>();
                    return fieldModel;
                });
            });

        return model;
    }

    public Task<NexportRegistrationFieldCategoryModel> PrepareNexportRegistrationFieldCategoryModelAsync(
        NexportRegistrationFieldCategoryModel model, NexportRegistrationFieldCategory registrationFieldCategory)
    {
        if (registrationFieldCategory != null)
        {
            model ??= registrationFieldCategory.ToModel<NexportRegistrationFieldCategoryModel>();
        }

        return Task.FromResult(model);
    }

    public Task<NexportRegistrationFieldOptionSearchModel> PrepareNexportRegistrationFieldOptionSearchModelAsync(
        NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (registrationField == null)
            throw new ArgumentNullException(nameof(registrationField));

        searchModel.RegistrationFieldId = registrationField.Id;

        //  searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public virtual async Task<NexportRegistrationFieldListModel> PrepareNexportRegistrationFieldListModelAsync(
        NexportRegistrationFieldSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var registrationFields =
            await _nexportService.GetNexportRegistrationFieldsPagination(searchModel.SelectedStoreIds, searchModel.Page - 1, searchModel.PageSize);

        var model = await new NexportRegistrationFieldListModel().PrepareToGridAsync(searchModel,
            registrationFields, () =>
            {
                return registrationFields.SelectAwait(async field =>
                {
                    var fieldModel = field.ToModel<NexportRegistrationFieldModel>();

                    if (fieldModel.FieldCategoryId.HasValue)
                        fieldModel.FieldCategoryName =
                            (await _nexportService.GetNexportRegistrationFieldCategoryById(
                                fieldModel.FieldCategoryId.Value)).Title;

                    var storeMappings = await _nexportService
                        .GetNexportRegistrationFieldStoreMappings(fieldModel.Id);

                    for (var i = 0; i < storeMappings.Count; i++)
                    {
                        var store = await _storeService.GetStoreByIdAsync(storeMappings[i].StoreId);
                        if (store != null)
                        {
                            if (i < storeMappings.Count - 1)
                                fieldModel.StoreMappings += $"{store.Name}, ";
                            else
                                fieldModel.StoreMappings += $"{store.Name}";
                            fieldModel.StoreMappingIds.Add(store.Id);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fieldModel.CustomFieldRender))
                    {
                        var customRenderPlugin =
                            await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(fieldModel
                                .CustomFieldRender);
                        fieldModel.CustomFieldRenderDescription = customRenderPlugin?.PluginDescriptor.FriendlyName;
                    }

                    return fieldModel;
                });
            });

        return model;
    }

    public virtual async Task<NexportRegistrationFieldModel> PrepareNexportRegistrationFieldModelAsync(
        NexportRegistrationFieldModel model,
        NexportRegistrationField registrationField, bool excludeProperties = false)
    {
        Func<NexportRegistrationFieldLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (registrationField != null)
        {
            model ??= registrationField.ToModel<NexportRegistrationFieldModel>();

            model.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(registrationField,
                nameof(model.AllowMultipleSelection), defaultValue: false);

            model.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(registrationField,
                nameof(model.DisplayOptionByAscendingOrder), defaultValue: false);

            model.StoreMappingIds = (await _nexportService.GetNexportRegistrationFieldStoreMappings(registrationField.Id))
                .Select(s => s.StoreId).ToList();

            await PrepareNexportRegistrationFieldOptionSearchModelAsync(model.RegistrationFieldOptionSearchModel, registrationField);

            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(registrationField,
                    entity => entity.Name, languageId,
                    false, false);
            };
        }

        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        model.AvailableFieldCategory = await _nexportService.GetRegistrationFieldCategoryList();

        var availableStores = await _storeService.GetAllStoresAsync();
        model.AvailableStores = availableStores.Select(store => new SelectListItem
        {
            Text = store.Name,
            Value = store.Id.ToString(),
            Selected = model.StoreMappingIds.Contains(store.Id)
        }).ToList();

        model.AvailableCustomFieldRenders = await _nexportService.GetCustomRegistrationFieldRendersAsync();

        return model;
    }

    public virtual async Task<NexportRegistrationFieldOptionListModel>
        PrepareNexportRegistrationFieldOptionListModelAsync(NexportRegistrationFieldOptionSearchModel searchModel,
            NexportRegistrationField registrationField)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var registrationFieldOptions =
            await _nexportService.GetNexportRegistrationFieldOptionsPagination(registrationField.Id,
                searchModel.Page - 1, searchModel.PageSize);

        var model = new NexportRegistrationFieldOptionListModel().PrepareToGrid(searchModel,
            registrationFieldOptions, () =>
            {
                return registrationFieldOptions.Select(fieldOption =>
                {
                    var fieldOptionModel = fieldOption.ToModel<NexportRegistrationFieldOptionModel>();
                    return fieldOptionModel;
                });
            });

        return model;
    }

    public Task<NexportRegistrationFieldOptionModel> PrepareNexportRegistrationFieldOptionModelAsync(
        NexportRegistrationFieldOptionModel model, NexportRegistrationField registrationField,
        NexportRegistrationFieldOption registrationFieldOption)
    {
        if (registrationFieldOption != null)
        {
            model ??= registrationFieldOption.ToModel<NexportRegistrationFieldOptionModel>();
        }

        return Task.FromResult(model);
    }

    public async Task<NexportCustomerRegistrationFieldsModel> PrepareNexportCustomerRegistrationFieldsModelAsync(
        Store store)
    {
        var model = new NexportCustomerRegistrationFieldsModel();

        var availableFields = await _nexportService.GetNexportRegistrationFields(store.Id);

        var fieldsWithCategory = (await availableFields.Where(x => x.FieldCategoryId != null)
            .GroupByAwait(async x =>
            {
                var fieldCategory = await _nexportService.GetNexportRegistrationFieldCategoryById(x.FieldCategoryId.Value);
                return fieldCategory;
            })
            .ToDictionaryAwaitAsync(
                async x => x.Key.ToModel<NexportRegistrationFieldCategoryModel>(),
                x => x
                    .SelectAwait(async f =>
                        {
                            var fieldModel = f.ToModel<NexportRegistrationFieldModel>();
                            if (fieldModel.Type is NexportRegistrationFieldType.SelectCheckbox or NexportRegistrationFieldType.SelectDropDown)
                            {
                                if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                                    fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(f,
                                        nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                                fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(f,
                                    nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                            }

                            return fieldModel;
                        }).OrderBy(f => f.DisplayOrder).ToListAsync()))
            .OrderBy(x => x.Key.DisplayOrder)
            .ThenBy(x => x.Key.Title);

        model.RegistrationFieldsWithCategory = fieldsWithCategory.ToDictionary(
            x => x.Key,
            x => x.Value);

        model.RegistrationFieldsWithoutCategory = await availableFields
            .Where(x => x.FieldCategoryId == null)
            .OrderBy(x => x.DisplayOrder)
            .SelectAwait(async f =>
            {
                var fieldModel = f.ToModel<NexportRegistrationFieldModel>();
                if (fieldModel.Type is NexportRegistrationFieldType.SelectCheckbox or NexportRegistrationFieldType.SelectDropDown)
                {
                    if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                        fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(f,
                            nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(f,
                        nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                }

                return fieldModel;
            })
            .ToListAsync();

        return model;
    }

    public async Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(
        Store store)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var model = new NexportAddCustomerRegistrationFieldsModel();

        var availableFields = await _nexportService.GetNexportRegistrationFields(store.Id);

        model.RegistrationFields = await availableFields
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .SelectAwait(async x =>
            {
                var fieldModel = x.ToModel<NexportRegistrationFieldModel>();
                if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox ||
                    fieldModel.Type == NexportRegistrationFieldType.SelectDropDown)
                {
                    if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                        fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(x,
                            nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(x,
                        nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                }

                return fieldModel;
            })
            .ToListAsync();

        return model;
    }

    public async Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(
        Customer customer, Store store)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var model = new NexportAddCustomerRegistrationFieldsModel();

        var availableFields = await _nexportService.GetNexportRegistrationFields(store.Id);
        var customerExistingFields = await _nexportService.GetNexportRegistrationFieldsWithAnswers(customer.Id, store.Id);
        var fields = availableFields.Where(x => customerExistingFields.All(f => f.Id != x.Id));

        model.RegistrationFields = await fields
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .SelectAwait(async x =>
            {
                var fieldModel = x.ToModel<NexportRegistrationFieldModel>();
                if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox ||
                    fieldModel.Type == NexportRegistrationFieldType.SelectDropDown)
                {
                    if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                        fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(x,
                            nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(x,
                        nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                }

                return fieldModel;
            })
            .ToListAsync();

        return model;
    }

    public async Task<NexportCustomerRegistrationFieldAnswerListModel>
        PrepareNexportCustomerRegistrationFieldAnswerListModel(
            NexportCustomerRegistrationFieldAnswerListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var customerNexportRegistrationFieldAnswers = await _nexportService.GetNexportRegistrationFieldAnswersPagination(searchModel.CustomerId, searchModel.FieldId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new NexportCustomerRegistrationFieldAnswerListModel().PrepareToGridAsync(searchModel, customerNexportRegistrationFieldAnswers, () =>
        {
            return customerNexportRegistrationFieldAnswers.SelectAwait(async answer =>
            {
                var answerModel = answer.ToModel<NexportCustomerRegistrationFieldAnswerModel>();
                var registrationField = await _nexportService.GetNexportRegistrationFieldById(answer.FieldId);
                if (registrationField != null)
                {
                    if (string.IsNullOrEmpty(registrationField.CustomFieldRender))
                    {
                        if (!string.IsNullOrEmpty(answer.TextValue))
                        {
                            answerModel.FieldValue = answer.TextValue;
                        }
                        else if (answer.NumericValue != null)
                        {
                            answerModel.FieldValue = answer.NumericValue.ToString();
                        }
                        else if (answer.DateTimeValue != null)
                        {
                            answerModel.FieldValue = answer.DateTimeValue.ToString();
                        }
                        else if (answer.BooleanValue != null)
                        {
                            answerModel.FieldValue = answer.BooleanValue.Value ? "True" : "False";
                        }
                        else if (answer.FieldOptionId != null)
                        {
                            var fieldOption =
                                await _nexportService.GetNexportRegistrationFieldOptionById(answer.FieldOptionId.Value,
                                    answer.FieldId);
                            if (fieldOption != null)
                            {
                                answerModel.FieldValue = fieldOption.OptionValue;
                            }
                        }
                    }
                    else
                    {
                        var customRender = await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender);

                        if (customRender != null)
                        {
                            var customFieldRenderAnswers = await customRender.GetCustomFieldNamesAndValues(searchModel.CustomerId, registrationField.Id);
                            answerModel.FieldValue = string.Join("; ",
                                customFieldRenderAnswers
                                    .Select(customAnswer =>
                                        string.IsNullOrWhiteSpace(customAnswer.Value)
                                            ? $"{customAnswer.Key}: N/A"
                                            : $"{customAnswer.Key}: {customAnswer.Value}")
                                    .ToList());
                        }
                    }
                }

                return answerModel;
            });
        });

        return model;
    }

    public async Task<NexportCustomerRegistrationFieldWithAnswersListModel>
        PrepareNexportCustomerRegistrationFieldWithAnswersListModel(
            NexportCustomerRegistrationFieldWithAnswersListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var customerNexportRegistrationFieldsWithAnswers = await _nexportService.GetNexportRegistrationFieldsWithAnswersPagination(searchModel.CustomerId,
            searchModel.StoreId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new NexportCustomerRegistrationFieldWithAnswersListModel().PrepareToGrid(searchModel, customerNexportRegistrationFieldsWithAnswers, () =>
        {
            return customerNexportRegistrationFieldsWithAnswers.Select(field =>
            {
                var fieldModel = field.ToModel<NexportCustomerRegistrationFieldWithAnswersModel>();
                fieldModel.CustomerId = searchModel.CustomerId;
                fieldModel.FieldType = field.Type.GetDisplayName();
                fieldModel.NexportCustomProfileFieldKey = field.NexportCustomProfileFieldKey;

                return fieldModel;
            });
        });

        return model;
    }

    public async Task<NexportCustomerRegistrationFieldAnswersEditModel>
        PrepareNexportCustomerRegistrationFieldAnswersEditModel(Customer customer,
            NexportRegistrationField registrationField)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (registrationField == null)
            throw new ArgumentNullException(nameof(registrationField));

        var currentRegistrationFieldAnswers = await _nexportService.GetNexportRegistrationFieldAnswers(customer.Id, registrationField.Id);

        var registrationFieldModel = registrationField.ToModel<NexportRegistrationFieldModel>();
        await PrepareNexportRegistrationFieldModelAsync(registrationFieldModel, registrationField);

        var model = new NexportCustomerRegistrationFieldAnswersEditModel
        {
            RegistrationField = registrationFieldModel,
            Options = await _nexportService.GetNexportRegistrationFieldOptions(registrationField.Id),
            Answers = currentRegistrationFieldAnswers
        };

        return model;
    }

    public async Task<NexportOrderInvoiceItemListModel> PrepareNexportOrderInvoiceItemListModelAsync(
        NexportOrderInvoiceItemSearchModel searchModel, bool excludeNonApproval = false)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var nexportOrderInvoiceItems =
            await _nexportService.GetNexportOrderInvoiceItems(searchModel.OrderId, excludeNonApproval,
                searchModel.Page - 1, searchModel.PageSize);

        var model = await new NexportOrderInvoiceItemListModel().PrepareToGridAsync(searchModel,
            nexportOrderInvoiceItems, () =>
            {
                return nexportOrderInvoiceItems.SelectAwait(async orderInvoiceItem =>
                {
                    var orderInvoiceItemModel = orderInvoiceItem.ToModel<NexportOrderInvoiceItemModel>();

                    var order = await _orderService.GetOrderByIdAsync(orderInvoiceItemModel.OrderId);
                    var orderItem = await _orderService.GetOrderItemByIdAsync(orderInvoiceItemModel.OrderItemId);
                    var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                    var productMapping = await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, store.Id) ??
                                         await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);
                    if (productMapping != null)
                    {
                        orderInvoiceItemModel.ProductName = (await _productService.GetProductByIdAsync(orderItem.ProductId)).Name;
                        orderInvoiceItemModel.NexportProductName = productMapping.NexportProductName;
                        if (productMapping.NexportSyllabusId != null)
                        {
                            orderInvoiceItemModel.NexportSyllabusId = productMapping.NexportSyllabusId.Value;
                            Guid orgId;

                            if (productMapping.NexportSubscriptionOrgId != null)
                                orgId = productMapping.NexportSubscriptionOrgId.Value;
                            else
                                orgId = await _genericAttributeService.GetAttributeAsync<Guid?>(store,
                                    // ReSharper disable once PossibleInvalidOperationException
                                    "NexportSubscriptionOrganizationId", store.Id) ?? _nexportSettings.RootOrganizationId.Value;

                            var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                            var existingEnrollment = await _nexportService.GetSectionEnrollmentDetailsAsync(
                                orgId, nexportUserMapping.NexportUserId, productMapping.NexportSyllabusId.Value);
                            if (existingEnrollment != null)
                            {
                                orderInvoiceItemModel.ExistingEnrollmentId = existingEnrollment.EnrollmentId;
                                orderInvoiceItemModel.UtcExistingEnrollmentExpirationDate = existingEnrollment.ExpirationDate;
                            }
                        }
                    }

                    return orderInvoiceItemModel;
                });
            });

        return model;
    }

    public Task<NexportOrderInvoiceItemModel> PrepareNexportOrderInvoiceItemModelAsync(
        NexportOrderInvoiceItemModel model,
        NexportOrderInvoiceItem orderInvoiceItem)
    {
        model ??= orderInvoiceItem.ToModel<NexportOrderInvoiceItemModel>();

        return Task.FromResult(model);
    }

    /// <summary>
    /// Prepare paged store list model
    /// </summary>
    /// <param name="searchModel">Store search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the store list model
    /// </returns>
    public virtual async Task<StoreListModel> PrepareStoreListModel(NexportStoreSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get stores
        var stores = await _nexportService.GetAllStoresAsync(storeName: searchModel.SearchStoreName,
            storeUrl: searchModel.SearchStoreUrl, pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize, excludeDeleted: true);

        //prepare list model
        var model = new StoreListModel().PrepareToGrid(searchModel, stores, () =>
        {
            //fill in model values from the entity
            return stores.Select(store => store.ToModel<StoreModel>());
        });

        return model;
    }

    public async Task<NexportOrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter orders
        var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
        var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
        var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            searchModel.VendorId = currentVendor.Id;
        var startDateValue = !searchModel.StartDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var endDateValue = !searchModel.EndDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        var filterByProductId = product != null && (currentVendor == null || product.VendorId == currentVendor.Id)
            ? searchModel.ProductId : 0;

        //get orders
        var orders = await _orderService.SearchOrdersAsync(storeId: searchModel.StoreId,
            vendorId: searchModel.VendorId,
            productId: filterByProductId,
            warehouseId: searchModel.WarehouseId,
            paymentMethodSystemName: searchModel.PaymentMethodSystemName,
            createdFromUtc: startDateValue,
            createdToUtc: endDateValue,
            osIds: orderStatusIds,
            psIds: paymentStatusIds,
            ssIds: shippingStatusIds,
            billingPhone: searchModel.BillingPhone,
            billingEmail: searchModel.BillingEmail,
            billingLastName: searchModel.BillingLastName,
            billingCountryId: searchModel.BillingCountryId,
            orderNotes: searchModel.OrderNotes,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new NexportOrderListModel().PrepareToGridAsync(searchModel, orders, () =>
        {
            //fill in model values from the entity
            return orders.SelectAwait(async order =>
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                var storeById = await _storeService.GetStoreByIdAsync(order.StoreId);
                var orderModel = new NexportOrderModel
                {
                    Id = order.Id,
                    OrderStatusId = order.OrderStatusId,
                    PaymentStatusId = order.PaymentStatusId,
                    ShippingStatusId = order.ShippingStatusId,
                    CustomerEmail = billingAddress.Email,
                    CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                    CustomerId = order.CustomerId,
                    CustomOrderNumber = order.CustomOrderNumber,
                    StoreUrl = storeById?.Url,
                    //convert dates to the user time
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    StoreName = storeById?.Name ?? "Deleted",
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                    OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false)
                };
                return orderModel;
            });
        });
        return model;
    }

    public virtual async Task<WholesaleOrderProductSearchModel> PrepareWholesaleOrderProductSearchModel(int storeId)
    {
        var model = new WholesaleOrderProductSearchModel { SearchStoreId = storeId };

        await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
        //await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);

        return model;
    }

    public virtual async Task<WholesaleOrderModel> PrepareWholesaleOrderModelAsync(WholesaleOrderModel model)
    {
        //if (model != null)
        //    return model;

        model = new WholesaleOrderModel();

        await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores, false);

        var paymentMethods = await _paymentPluginManager.LoadActivePluginsAsync();
        model.AvailablePaymentMethods = paymentMethods.Select(payment =>
        {
            var paymentMethodModel = payment.ToPluginModel<PaymentMethodModel>();

            return new SelectListItem
            {
                Text = paymentMethodModel.FriendlyName,
                Value = paymentMethodModel.SystemName
            };
        }).ToList();
        model.AvailablePaymentMethods.Insert(0, new SelectListItem { Text = "Select payment method", Value = null });

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(currentCustomer.Id);
        if (nexportUserMapping == null)
            throw new Exception($"Unable to verify Nexport user mapping for customer {currentCustomer.Id}");

        var groupPermissionSearchCacheKey = new CacheKey("Misc.Nexport.SearchGroupForPermission.{0}-{1}",
            nexportUserMapping.NexportUserId.ToString(), _nexportSettings.RootOrganizationId?.ToString())
        {
            CacheTime = 30
        };

        var groupsFromApi = await _staticCacheManager.GetAsync(groupPermissionSearchCacheKey,
            // ReSharper disable once PossibleInvalidOperationException
            async () => (await _nexportService.SearchGroupsForPermissionAsync(nexportUserMapping.NexportUserId, _nexportSettings.RootOrganizationId.Value)));
        model.AvailableOrganizations = groupsFromApi.Select(org => new SelectListItem { Text = $"{org.Name} ({org.ShortName})", Value = org.Id.ToString() }).ToList();
        model.AvailableOrganizations.Insert(0, new SelectListItem { Text = "Select organization", Value = "" });

        var fundingPools = await _nexportWholesaleService.GetFundingPools();
        model.AvailableFundingPools = fundingPools.Select(fundingPool => new SelectListItem { Text = $"{fundingPool.Name}", Value = fundingPool.Id.ToString() }).ToList();
        model.AvailableFundingPools.Insert(0, new SelectListItem { Text = "Select funding pool", Value = "" });

        return model;
    }

    public virtual async Task<WholesaleOrderPurchasingProducts> PrepareWholesaleOrderPurchasingProductsListAsync(IList<int> productIds)
    {
        var model = new WholesaleOrderPurchasingProducts();

        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());
        model.Items = products.Select(product => product.ToModel<WholesaleOrderProductModel>()).ToList();

        return model;
    }

    public virtual async Task<WholesaleOrderProductListModel> PrepareWholesaleOrderProductListModelAsync(WholesaleOrderProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var categoryIds = new List<int> { searchModel.SearchCategoryId };

        var products = await _nexportWholesaleService.SearchProductsAsync(
            categoryIds: categoryIds,
            storeId: searchModel.SearchStoreId,
            searchNexportProducts: searchModel.SearchOnlyNexportProduct,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var productListModel = await new WholesaleOrderProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                //fill in model values from the entity
                var productModel = product.ToModel<WholesaleOrderProductModel>();

                var nexportProductMapping =
                    await _nexportService.GetProductMappingByNopProductId(productModel.Id, searchModel.SearchStoreId);
                if (nexportProductMapping == null)
                {
                    nexportProductMapping = await _nexportService.GetProductMappingByNopProductId(productModel.Id);
                    if (nexportProductMapping != null)
                        productModel.NexportProductMappingId = nexportProductMapping.Id;
                }
                else
                    productModel.NexportProductMappingId = nexportProductMapping.Id;

                //little performance optimization: ensure that "FullDescription" is not returned
                productModel.FullDescription = string.Empty;

                //fill in additional values (not existing in the entity)
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                (productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);

                return productModel;
            });
        });

        return productListModel;
    }

    public virtual async Task<WholesaleOrderPaymentInfoModel> PrepareWholesaleOrderPaymentInfoModelAsync(string paymentSystemName)
    {
        var paymentMethod = await _paymentPluginManager.LoadActivePluginsAsync(new List<string> { paymentSystemName });
        if (paymentMethod.Count > 0)
        {
            return new WholesaleOrderPaymentInfoModel
            {
                PaymentViewComponent = paymentMethod[0].GetPublicViewComponent()
            };
        }

        return null;
    }

    public async Task<OrderSummaryCartFooterModel> PrepareOrderSummaryCartFooterModel(OrderSummaryCartFooterModel orderSummaryCartFooterModel, Customer customer, Store store, IList<ShoppingCartItem> cart)
    {
        if (orderSummaryCartFooterModel == null)
            throw new ArgumentNullException(nameof(orderSummaryCartFooterModel));
        if (store == null)
            throw new ArgumentNullException(nameof(store));
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        // hide group select if autoredeem
        foreach (var shoppingCartItem in cart)
        {
            if (shoppingCartItem == null)
            {
                orderSummaryCartFooterModel.ShowPurchasingGroupArea = false;
                break;
            }

            // gets default product mapping if there is no product mapping for store specified
            var npmInCart = (await _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId, store.Id)) ?? (await _nexportService.GetProductMappingByNopProductId(shoppingCartItem.ProductId));

            //hide purchasing group area if there is no product mapping or if product is set to autoredeem
            if (npmInCart != null)
            {
                if (npmInCart.AutoRedeem)
                {
                    orderSummaryCartFooterModel.ShowPurchasingGroupArea = false;
                    break;
                }

            }
            else
            {
                orderSummaryCartFooterModel.ShowPurchasingGroupArea = false;
                break;
            }
        }

        var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);

        //hide purchasing group area if user mapping is null or if there are no available groups
        if (userMapping != null)
        {
            if (orderSummaryCartFooterModel.ShowPurchasingGroupArea)
            {
                var selectedGroupInfo = await _genericAttributeService.GetAttributeAsync<string>(customer, "WholesaleOrder-PurchasingGroup", store.Id);

                if (selectedGroupInfo != null)
                {
                    var selectedGroup = JsonConvert.DeserializeObject<NexportGroupModel>(selectedGroupInfo);

                    if (selectedGroup != null && selectedGroup.OrganizationId != Guid.Empty)
                    {
                        orderSummaryCartFooterModel.PurchasingGroup = selectedGroup;
                    }
                }

                var selectedFundingPoolId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "WholesaleOrder-FundingPoolId", store.Id);

                if (selectedFundingPoolId != null)
                {
                    var fundingPool = await _nexportWholesaleService.GetFundingPoolById(selectedFundingPoolId.Value);

                    if (fundingPool != null)
                    {
                        orderSummaryCartFooterModel.FundingPool = fundingPool.ToModel<NexportFundingPoolModel>();
                    }
                }

                var groupPermissionSearchCacheKey = new CacheKey("Misc.Nexport.SearchGroupForPermission.{0}-{1}",
                    userMapping.NexportUserId.ToString(), _nexportSettings.RootOrganizationId?.ToString())
                {
                    CacheTime = 30
                };

                var groupsFromApi = await _staticCacheManager.GetAsync(groupPermissionSearchCacheKey,
                    // ReSharper disable once PossibleInvalidOperationException
                    async () => (await _nexportService.SearchGroupsForPermissionAsync(userMapping.NexportUserId, _nexportSettings.RootOrganizationId.Value)));

                orderSummaryCartFooterModel.AvailableGroups = await groupsFromApi.Select(x =>
                    new NexportGroupModel
                    {
                        OrganizationId = x.Id,
                        Name = x.Name,
                        ShortName = x.ShortName
                    }).ToListAsync();

                if (orderSummaryCartFooterModel.AvailableGroups.Count < 1)
                    orderSummaryCartFooterModel.ShowPurchasingGroupArea = false;

                var fundingPools = await _nexportWholesaleService.GetFundingPools();
                orderSummaryCartFooterModel.AvailableFundingPools = fundingPools.Select(x => x.ToModel<NexportFundingPoolModel>()).ToList();
            }
        }
        else
        {
            orderSummaryCartFooterModel.ShowPurchasingGroupArea = false;
        }

        return orderSummaryCartFooterModel;
    }

    public virtual async Task<NexportGroupProductListModel> PrepareNexportGroupProductListModelAsync(NexportGroupProductListSearchModel searchModel, Customer currentCustomer)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var model = new NexportGroupProductListModel();

        IPagedList<WholesaleOrderInfo> wholesaleOrderInfos;

        var isAdmin = await _customerService.IsAdminAsync(currentCustomer);
        if (searchModel.AdminView && isAdmin)
        {
            wholesaleOrderInfos =
                await _nexportService.GetAllWholesaleOrderInfosAsync(searchModel.SearchGroupName, searchModel.SearchGroupShortName, searchModel.SearchProductName, searchModel.SearchStatusId, null, null,
                    searchModel.Page - 1, searchModel.PageSize);
        }
        else
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            wholesaleOrderInfos =
                await _nexportService.GetAllWholesaleOrderInfosAsync(searchModel.SearchGroupName, searchModel.SearchGroupShortName, searchModel.SearchProductName, searchModel.SearchStatusId, currentCustomer.Id, store.Id,
                    searchModel.Page - 1, searchModel.PageSize);
        }

        IList<NexportGroupProductModel> groupProductModels = new List<NexportGroupProductModel>();

        foreach (var orderInfo in wholesaleOrderInfos)
        {
            if (orderInfo.NexportGroupId != null && !searchModel.AdminView && !isAdmin)
            {
                //skip group if not admin view and user doesn't have group permission
                if (!await _nexportService.HasGroupPermissionAsync(currentCustomer, orderInfo.NexportGroupId.Value))
                    continue;
            }

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderInfo.OrderItemId);
            if (orderItem != null)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    var groupProductModel = new NexportGroupProductModel
                    {
                        Id = product.Id,
                        Available = orderInfo.Available,
                        Awaiting = orderInfo.Awaiting,
                        Redeemed = orderInfo.Redeemed,
                        GroupId = orderInfo.NexportGroupId,
                        ProductName = product.Name
                    };

                    if (orderInfo.NexportGroupId != null)
                    {
                        var group = await _nexportService.GetWholesalePurchaseGroupAsync(orderInfo.NexportGroupId.Value);

                        if (group != null)
                        {
                            groupProductModel.GroupName = group.NexportGroupName;
                            groupProductModel.GroupShortName = group.NexportGroupShortName;
                        }
                    }
                    else
                    {
                        groupProductModel.GroupName = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Group.NoGroup");
                    }

                    groupProductModels.Add(groupProductModel);
                }
            }
        }

        var pagedProducts = groupProductModels.ToPagedList(searchModel);
        model = await model.PrepareToGridAsync(searchModel, pagedProducts, () =>
        {
            return pagedProducts.SelectAwait(async groupProductModel => groupProductModel);
        });

        return model;
    }

    public virtual async Task<NexportGroupProductRedemptionListModel> PrepareNexportGroupProductRedemptionListModelAsync(
        NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId, Customer currentCustomer, int? orderId = null)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (productId < 1)
            throw new ArgumentOutOfRangeException(nameof(productId));

        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));

        var model = new NexportGroupProductRedemptionListModel();

        try
        {
            var redemptions = new List<NexportGroupProductRedemptionModel>();

            var dateAssignedFromValue = !searchModel.DateAssignedFrom.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.DateAssignedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var dateAssignedToValue = !searchModel.DateAssignedTo.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.DateAssignedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            IList<NexportOrderInvoiceItem> invoiceItems;

            // check for wholesale purchases that have no group
            // if current customer is admin, get all purchases for group not assigned
            // otherwise only get the ones for the current store and the current customer
            if (groupId == null)
            {
                var isAdmin = await _customerService.IsAdminAsync(currentCustomer);
                if (searchModel.AdminView && isAdmin)
                {
                    //show all items under group not assigned
                    invoiceItems = await _nexportService.SearchGroupProductRedemptionsAsync(null, productId,
                        searchModel.SearchName, searchModel.SearchEmail, searchModel.SearchStatusId,
                        dateAssignedFromValue, dateAssignedToValue, orderId);
                }
                else
                {
                    //show only items for group not assigned that belong to the current store and current customer
                    var store = await _storeContext.GetCurrentStoreAsync();
                    invoiceItems = await _nexportService.SearchGroupProductRedemptionsAsync(null, productId,
                                    searchModel.SearchName, searchModel.SearchEmail, searchModel.SearchStatusId, dateAssignedFromValue, dateAssignedToValue, orderId, store, currentCustomer);
                }
            }
            // check for wholesale purchases which the customer has purchasing agent permission on
            // if current customer is admin, get all purchases
            // otherwise only get the ones for the current store
            else
            {
                var hasGroupPermission = await _nexportService.HasGroupPermissionAsync(currentCustomer, groupId.Value);
                if (searchModel.AdminView && hasGroupPermission)
                {
                    //show all items for the group
                    invoiceItems = await _nexportService.SearchGroupProductRedemptionsAsync(groupId, productId,
                        searchModel.SearchName, searchModel.SearchEmail, searchModel.SearchStatusId,
                        dateAssignedFromValue, dateAssignedToValue, orderId);
                }
                else
                {
                    //show only items for the group that belong to the current store
                    var store = await _storeContext.GetCurrentStoreAsync();
                    invoiceItems = await _nexportService.SearchGroupProductRedemptionsAsync(groupId, productId,
                                    searchModel.SearchName, searchModel.SearchEmail, searchModel.SearchStatusId, dateAssignedFromValue, dateAssignedToValue, orderId, store);
                }
            }

            if (invoiceItems != null)
            {
                foreach (var invoiceItem in invoiceItems)
                {
                    var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                    if (order == null)
                    {
                        await _logger.WarningAsync($"Invoice item: {invoiceItem.Id} in redemptions list has null order");
                        continue;
                    }

                    var storeForOrder = await _storeService.GetStoreByIdAsync(order.StoreId);
                    var purchasedBy = await _customerService.GetCustomerByIdAsync(order.CustomerId);

                    var redemptionItem = new NexportGroupProductRedemptionModel
                    {
                        Id = invoiceItem.Id,
                        InvoiceItemId = invoiceItem.InvoiceItemId,
                        OrderId = invoiceItem.OrderId,
                        Status = invoiceItem.RedemptionStatus.GetDisplayName(),
                        DatePurchased = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                        DateRedeemed = invoiceItem.UtcDateRedemption.HasValue
                            //? (await _dateTimeHelper.ConvertToUserTimeAsync(invoiceItem.UtcDateRedemption.Value, DateTimeKind.Utc)).ToString("MM/dd/yyyy h:mm tt")
                            ? (await _dateTimeHelper.ConvertToUserTimeAsync(invoiceItem.UtcDateRedemption.Value, DateTimeKind.Utc))
                            : null,
                        PurchasedBy = purchasedBy != null ? $"{purchasedBy.Email}" : "",
                        PurchasedIn = storeForOrder != null ? storeForOrder.Name : ""
                    };

                    var selectedMappingForOpenEndedProductStr =
                        await _genericAttributeService.GetAttributeAsync<string>(
                            invoiceItem,
                            $"SelectedMappingForOpenEndedProduct-{invoiceItem.Id}",
                            order.StoreId);

                    //is open ended product and status is assigned or awaiting then display the product name from the generic attribute
                    //otherwise display the product name the normal way
                    if (selectedMappingForOpenEndedProductStr != null &&
                        invoiceItem.RedemptionStatus is NexportOrderInvoiceItemRedemptionStatus.Assigned or NexportOrderInvoiceItemRedemptionStatus.Awaiting)
                    {
                        var selectedMappingForOpenEndedProduct = JsonConvert.DeserializeObject<NexportProductMapping>(selectedMappingForOpenEndedProductStr);

                        if (selectedMappingForOpenEndedProduct != null)
                        {
                            var product = await _productService.GetProductByIdAsync(selectedMappingForOpenEndedProduct.NopProductId);
                            if (product != null)
                            {
                                redemptionItem.ProductName = product.Name;
                            }
                        }
                    }
                    else
                    {
                        var product = await _productService.GetProductByIdAsync(productId);
                        if (product != null)
                        {
                            redemptionItem.ProductName = product.Name;
                        }
                    }

                    //set the name and email of the redeeming customer for the redemption item
                    if (invoiceItem.RedeemingUserId.HasValue)
                    {
                        var redeemerUserMapping = await _nexportService.FindUserMappingByNexportUserId(invoiceItem.RedeemingUserId.Value);

                        if (redeemerUserMapping != null)
                        {
                            var redeemer = await _nexportService.FindCustomerByIdAsync(redeemerUserMapping.NopUserId);
                            if (redeemer != null)
                            {
                                redemptionItem.Name = $"{redeemer.FirstName} {redeemer.LastName}";
                                redemptionItem.Email = redeemer.Email;
                                redemptionItem.CustomerId = redeemer.Id;
                            }
                            else
                            {
                                redemptionItem.Name = "Deleted user";
                            }
                        }
                        else
                        {
                            //set the name and email for awaiting status redemption
                            var email = await _genericAttributeService.GetAttributeAsync<string>(invoiceItem,
                                $"redeeming-user-email-for-invoice-{invoiceItem.Id}", order.StoreId);
                            if (email != null)
                            {
                                redemptionItem.Email = email;
                                redemptionItem.Name =
                                    $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", order.StoreId)} " +
                                    $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", order.StoreId)}";
                            }
                        }
                    }
                    else
                    {
                        //set the name and email for processing and awaiting status redemption
                        var email = await _genericAttributeService.GetAttributeAsync<string>(invoiceItem,
                            $"redeeming-user-email-for-invoice-{invoiceItem.Id}", order.StoreId);
                        if (email != null)
                        {
                            redemptionItem.Email = email;
                            redemptionItem.Name =
                                $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", order.StoreId)} " +
                                $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", order.StoreId)}";
                        }
                    }

                    redemptions.Add(redemptionItem);
                }
            }

            var pagedRedemptions = redemptions.ToPagedList(searchModel);

            model = await model.PrepareToGridAsync(searchModel, pagedRedemptions, () =>
            {
                return pagedRedemptions.SelectAwait(async redemption => redemption);
            });
        }
        catch (Exception ex)
        {
            await _logger.WarningAsync("Unable to prepare redemptions list", ex);
        }

        return model;
    }

    public async Task<NexportGroupProductListSearchModel> PrepareNexportGroupProductListSearchModelAsync(Guid groupId)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException(null, nameof(groupId));

        var model = new NexportGroupProductListSearchModel();

        var groupAttr = await _nexportService.GetGroupByGroupIdAsync(groupId);
        if (groupAttr != null)
        {
            try
            {
                var group = JsonConvert.DeserializeObject<NexportGroupModel>(groupAttr.Value);
                if (group != null)
                    model.CurrentGroup = group;
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync($"Unable to get current group for product list", ex);
            }
        }

        return model;
    }

    public async Task<NexportGroupProductListSearchModel> PrepareNexportGroupProductListSearchModelAsync(int? customerId = null, int? productId = null, int? statusId = null)
    {
        var model = new NexportGroupProductListSearchModel();

        if (customerId != null)
        {
            var userMapping = await _nexportService.FindUserMappingByCustomerId(customerId.Value);
            if (userMapping != null)
            {
                var groupPermissionSearchCacheKey = new CacheKey("Misc.Nexport.SearchGroupForPermission.{0}-{1}",
                    userMapping.NexportUserId.ToString(), _nexportSettings.RootOrganizationId?.ToString())
                {
                    CacheTime = 30
                };

                var groupSearchResult = await _staticCacheManager.GetAsync(groupPermissionSearchCacheKey,
                    async () => (await _nexportService.SearchGroupsForPermissionAsync(userMapping.NexportUserId,
                        // ReSharper disable once PossibleInvalidOperationException
                        _nexportSettings.RootOrganizationId.Value)));

                model.HasPurchasingAgentPermissions = groupSearchResult.Any();
            }
        }

        if (productId != null)
        {
            var product = await _productService.GetProductByIdAsync(productId.Value);
            model.SearchProductName = product.Name;
        }

        if (statusId != null)
        {
            model.SearchStatusId = (NexportOrderInvoiceItemRedemptionStatus)statusId.Value;
        }

        //prepare available statuses
        model.AvailableStatuses.Add(new SelectListItem { Value = null, Text = "All" });

        //for each status enum create a selectlistitem and add it for the status filter
        //foreach (var e in Enum.GetValues(typeof(NexportOrderInvoiceItemRedemptionStatus)))
        //{
        //    if (e.GetDisplayName() ==
        //        NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable.GetDisplayName() ||
        //        e.GetDisplayName() == NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting.GetDisplayName())
        //        continue;

        //    model.AvailableStatuses.Add(new SelectListItem
        //    {
        //        Value = e.ToString(),
        //        Text = e.GetDisplayName()
        //    });
        //}

        var statusList = NexportOrderInvoiceItemRedemptionStatus.Available.ToNexportSelectList(false, valuesToExclude: new[]
        {
            (int)NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable,
            (int)NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting
        });

        foreach (var item in statusList)
        {
            model.AvailableStatuses.Add(item);
        }

        return model;
    }

    public virtual async Task<NexportGroupProductRedemptionListSearchModel>
        PrepareNexportGroupProductRedemptionListSearchModelAsync(Guid? groupId, int productId, int? orderId = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));
        if (productId < 1)
            throw new ArgumentOutOfRangeException(nameof(productId));

        var model = new NexportGroupProductRedemptionListSearchModel { OrderId = orderId };

        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            model.CurrentProduct = product;
        }

        //prepare available statuses
        model.AvailableStatuses.Add(new SelectListItem { Value = null, Text = "All" });

        //for each status enum create a selectlistitem and add it for the status filter
        foreach (var e in Enum.GetValues(typeof(NexportOrderInvoiceItemRedemptionStatus)))
        {
            if (e.GetDisplayName() ==
                NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable.GetDisplayName() ||
                e.GetDisplayName() == NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting.GetDisplayName())
                continue;

            model.AvailableStatuses.Add(new SelectListItem
            {
                Value = e.ToString(),
                Text = e.GetDisplayName()
            });
        }

        if (groupId == null)
            return model;

        var group = await _nexportService.GetWholesalePurchaseGroupAsync(groupId.Value);

        var groupModel = group?.ToModel<NexportGroupModel>();
        if (groupModel != null)
            model.CurrentGroup = groupModel;

        return model;
    }

    public async Task<NexportPurchasesByFundingPoolListSearchModel>
        PrepareNexportPurchasesByFundingPoolListSearchModelAsync(int? customerId = null,
            int? productId = null, int? statusId = null)
    {
        var model = new NexportPurchasesByFundingPoolListSearchModel();

        if (statusId != null)
        {
            model.SearchStatusId = (NexportOrderInvoiceItemRedemptionStatus)statusId.Value;
        }

        model.AvailableStatuses.Add(new SelectListItem { Value = null, Text = "All" });

        foreach (var e in Enum.GetValues(typeof(NexportOrderInvoiceItemRedemptionStatus)))
        {
            if (e.GetDisplayName() == NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable.GetDisplayName() ||
                e.GetDisplayName() == NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting.GetDisplayName())
                continue;

            model.AvailableStatuses.Add(new SelectListItem
            {
                Value = e.ToString(),
                Text = e.GetDisplayName()
            });
        }

        return model;
    }

    public async Task<NexportPurchasesByFundingPoolListModel> PrepareNexportPurchasesByFundingPoolListModelAsync(
        NexportPurchasesByFundingPoolListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var model = new NexportPurchasesByFundingPoolListModel();

        var wholesaleOrderInfos = await _nexportService.GetAllWholesaleOrderInfosByFundingPoolsAsync(
            searchModel.SearchFundingPoolName, searchModel.SearchStatusId, null, null,
            searchModel.Page - 1, searchModel.PageSize);

        var result = await wholesaleOrderInfos
            .GroupBy(x => x.FundingPoolId)
            .SelectAwait(async x =>
            {
                var fundingPoolName = "N/A";
                if (x.Key != null)
                {
                    var fundingPool = await _nexportWholesaleService.GetFundingPoolById(x.Key.Value);
                    if (fundingPool != null) fundingPoolName = fundingPool.Name;
                }

                return new NexportPurchasesByFundingPoolModel()
                {
                    FundingPoolId = x.Key,
                    FundingPool = fundingPoolName,
                    Available = x.Sum(y=>y.Available),
                    Awaiting = x.Sum(y => y.Awaiting),
                    Redeemed = x.Sum(y => y.Redeemed),
                };
            }).ToListAsync();

        var pageResult = result.ToPagedList(searchModel);
        model = await model.PrepareToGridAsync(searchModel, pageResult, () =>
        {
            return pageResult.SelectAwait(async groupProductModel => groupProductModel);
        });

        return model;
    }

    public async Task<NexportProductRedemptionListSearchModel>
        PrepareNexportPurchasesByFundingPoolsRedemptionListSearchModelAsync(int? fundingPoolId)
    {
        var model = new NexportProductRedemptionListSearchModel { FundingPoolId = fundingPoolId };

        model.AvailableStatuses.Add(new SelectListItem { Value = null, Text = "All" });

        var statusList = NexportOrderInvoiceItemRedemptionStatus.Available.ToNexportSelectList(false,
            valuesToExclude: new[]
            {
                (int)NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable,
                (int)NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting
            });

        foreach (var item in statusList)
        {
            model.AvailableStatuses.Add(item);
        }

        if (fundingPoolId == null)
            return model;

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(fundingPoolId.Value);
        var fundingPoolModel = fundingPool.ToModel<NexportFundingPoolModel>();
        if (fundingPoolModel != null)
            model.FundingPool = fundingPoolModel;

        return model;
    }

    public async Task<NexportGroupProductRedemptionListModel>
        PrepareNexportWholesalePurchasesByFundingPoolsRedemptionListModelAsync(
            NexportProductRedemptionListSearchModel searchModel, int? orderId = null)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var model = new NexportGroupProductRedemptionListModel();

        try
        {
            var redemptions = new List<NexportGroupProductRedemptionModel>();

            var dateAssignedFromValue = !searchModel.DateAssignedFrom.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.DateAssignedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var dateAssignedToValue = !searchModel.DateAssignedTo.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.DateAssignedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var invoiceItems = await _nexportService.SearchProductRedemptionsAsync(searchModel.FundingPoolId, null,
                searchModel.SearchCustomerEmail, searchModel.SearchCustomerName, searchModel.SearchStatusId,
                dateAssignedFromValue, dateAssignedToValue);

            if (invoiceItems != null)
            {
                foreach (var invoiceItem in invoiceItems)
                {
                    var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                    if (order == null)
                    {
                        await _logger.WarningAsync($"Invoice item: {invoiceItem.Id} in redemptions list has null order");
                        continue;
                    }

                    var storeForOrder = await _storeService.GetStoreByIdAsync(order.StoreId);
                    var purchasedBy = await _customerService.GetCustomerByIdAsync(order.CustomerId);

                    var redemptionItem = new NexportGroupProductRedemptionModel
                    {
                        Id = invoiceItem.Id,
                        InvoiceItemId = invoiceItem.InvoiceItemId,
                        OrderId = invoiceItem.OrderId,
                        Status = invoiceItem.RedemptionStatus.GetDisplayName(),
                        DatePurchased = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                        DateRedeemed = invoiceItem.UtcDateRedemption.HasValue
                            //? (await _dateTimeHelper.ConvertToUserTimeAsync(invoiceItem.UtcDateRedemption.Value, DateTimeKind.Utc)).ToString("MM/dd/yyyy h:mm tt")
                            ? (await _dateTimeHelper.ConvertToUserTimeAsync(invoiceItem.UtcDateRedemption.Value, DateTimeKind.Utc))
                            : null,
                        PurchasedBy = purchasedBy != null ? $"{purchasedBy.Email}" : "",
                        PurchasedIn = storeForOrder != null ? storeForOrder.Name : ""
                    };

                    var selectedMappingForOpenEndedProductStr =
                        await _genericAttributeService.GetAttributeAsync<string>(
                            invoiceItem,
                            $"SelectedMappingForOpenEndedProduct-{invoiceItem.Id}",
                            order.StoreId);

                    //is open ended product and status is assigned or awaiting then display the product name from the generic attribute
                    //otherwise display the product name the normal way
                    if (selectedMappingForOpenEndedProductStr != null &&
                        invoiceItem.RedemptionStatus is NexportOrderInvoiceItemRedemptionStatus.Assigned or NexportOrderInvoiceItemRedemptionStatus.Awaiting)
                    {
                        var selectedMappingForOpenEndedProduct = JsonConvert.DeserializeObject<NexportProductMapping>(selectedMappingForOpenEndedProductStr);

                        if (selectedMappingForOpenEndedProduct != null)
                        {
                            var product = await _productService.GetProductByIdAsync(selectedMappingForOpenEndedProduct.NopProductId);
                            if (product != null)
                            {
                                redemptionItem.ProductId = product.Id;
                                redemptionItem.ProductName = product.Name;
                            }
                        }
                    }
                    else
                    {
                        var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);
                        if (orderItem != null)
                        {
                            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                            if (product != null)
                            {
                                redemptionItem.ProductId = product.Id;
                                redemptionItem.ProductName = product.Name;
                            }
                        }
                    }

                    //set the name and email of the redeeming customer for the redemption item
                    if (invoiceItem.RedeemingUserId.HasValue)
                    {
                        var redeemerUserMapping = await _nexportService.FindUserMappingByNexportUserId(invoiceItem.RedeemingUserId.Value);

                        if (redeemerUserMapping != null)
                        {
                            var redeemer = await _nexportService.FindCustomerByIdAsync(redeemerUserMapping.NopUserId);
                            if (redeemer != null)
                            {
                                redemptionItem.Name = $"{redeemer.FirstName} {redeemer.LastName}";
                                redemptionItem.Email = redeemer.Email;
                                redemptionItem.CustomerId = redeemer.Id;
                            }
                            else
                            {
                                redemptionItem.Name = "Deleted user";
                            }
                        }
                        else
                        {
                            //set the name and email for awaiting status redemption
                            var email = await _genericAttributeService.GetAttributeAsync<string>(invoiceItem,
                                $"redeeming-user-email-for-invoice-{invoiceItem.Id}", order.StoreId);
                            if (email != null)
                            {
                                redemptionItem.Email = email;
                                redemptionItem.Name =
                                    $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", order.StoreId)} " +
                                    $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", order.StoreId)}";
                            }
                        }
                    }
                    else
                    {
                        //set the name and email for processing and awaiting status redemption
                        var email = await _genericAttributeService.GetAttributeAsync<string>(invoiceItem,
                            $"redeeming-user-email-for-invoice-{invoiceItem.Id}", order.StoreId);
                        if (email != null)
                        {
                            redemptionItem.Email = email;
                            redemptionItem.Name =
                                $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", order.StoreId)} " +
                                $"{await _genericAttributeService.GetAttributeAsync<string>(invoiceItem, $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", order.StoreId)}";
                        }
                    }

                    redemptions.Add(redemptionItem);
                }
            }

            var pagedRedemptions = redemptions.ToPagedList(searchModel);

            model = await model.PrepareToGridAsync(searchModel, pagedRedemptions, () =>
            {
                return pagedRedemptions.SelectAwait(async redemption => redemption);
            });
        }
        catch (Exception ex)
        {
            await _logger.WarningAsync("Unable to prepare redemptions list", ex);
        }

        return model;
    }

    public async Task<ProductStepModel> PrepareProductStepModel(int productId, Guid invoiceItemId)
    {
        if (productId < 1)
            throw new ArgumentException("Invalid product id", nameof(productId));

        var model = new ProductStepModel();

        var product = await _productService.GetProductByIdAsync(productId)
                      ?? throw new Exception("No product found with the specified id");

        model.CurrentProduct = product;

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId)
                          ?? throw new Exception("No invoiceItem found with the specified id");

        model.InvoiceItemId = invoiceItem.InvoiceItemId;

        var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId)
                    ?? throw new Exception("No order found with the specified id");

        var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId)
                        ?? throw new Exception("No orderItem found with the specified id");

        var mappingInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
            $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

        // Retrieve the stored mapping info if existed; otherwise, get the current mapping info
        var mapping = mappingInfo != null
            ? JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo)
            : await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
              await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

        if (mapping == null)
            throw new Exception("Product mapping could not be found");

        if (mapping.AssignWhenRedeemed.HasValue && mapping.AssignWhenRedeemed.Value)
        {
            if (mapping.NopCategoryId != null)
            {
                model.AvailableMappings = await ProductStepModelGetAvailableMappingsForCategoryAsync(mapping.NopCategoryId.Value, mapping.NexportCatalogId, model.AvailableMappings);
            }
            else
            {
                model.ProductMappingIdForOpenEndedProduct = mapping.Id;
                model.AvailableMappings = await ProductStepModelGetAvailableMappingsForCatalogIdAsync(mapping.NexportCatalogId, model.AvailableMappings);
            }
        }
        else
        {
            model.SelectedProductMappingId = mapping.Id;
            model.AvailableMappings = new List<SelectListItem> { new(product.Name, $"{mapping.Id}") };
        }

        return model;
    }

    private async Task<IList<SelectListItem>> ProductStepModelGetAvailableMappingsForCategoryAsync(int nopCategoryId, Guid nexportCatalogId, IList<SelectListItem> availableMappings)
    {
        //show available products for the category
        var products = await _nexportService.GetAllProductsByCategoryId(nopCategoryId);

        foreach (var product in products)
        {
            var productMapping =
                await _nexportService.GetProductMappingByNopProductId(product.Id);

            if (productMapping == null)
                continue;

            if (productMapping.AutoRedeem)
                continue;

            if (productMapping.AssignWhenRedeemed ?? false)
            {
                availableMappings = await ProductStepModelGetAvailableMappingsForCatalogIdAsync(productMapping.NexportCatalogId, availableMappings);
            }
            else
            {
                availableMappings.Add(new SelectListItem(product.Name, $"{productMapping.Id}"));
            }
        }

        return availableMappings;
    }

    private async Task<IList<SelectListItem>> ProductStepModelGetAvailableMappingsForCatalogIdAsync(Guid nexportCatalogId, IList<SelectListItem> availableMappings)
    {
        var listOfMappings = await _nexportService.GetAllProductMappingsByCatalogIdAsync(nexportCatalogId);

        foreach (var productMapping in listOfMappings)
        {
            if (productMapping.AutoRedeem || productMapping.NexportSyllabusId == null)
                continue;

            var productMappingProduct = await _productService.GetProductByIdAsync(productMapping.NopProductId);

            if (productMappingProduct == null)
                continue;

            if (availableMappings.All(x => x.Value != $"{productMapping.Id}"))
                availableMappings.Add(new SelectListItem(productMappingProduct.Name, $"{productMapping.Id}"));

        }
        return availableMappings;
    }

    public virtual async Task<RedeemProductModel> PrepareRedeemProductModel(Guid? groupId, Guid invoiceItemId,
        int productId)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));
        if (invoiceItemId == Guid.Empty)
            throw new ArgumentException("Invoice item Id cannot be empty Guid", nameof(invoiceItemId));
        if (productId < 1)
            throw new ArgumentException("Invalid product id", nameof(productId));

        var model = new RedeemProductModel();

        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            model.CurrentProduct = product;

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

            if (invoiceItem != null)
            {
                model.InvoiceItemId = invoiceItem.InvoiceItemId;

                var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);

                var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);

                if (order != null && orderItem != null)
                {
                    var mappingInfo = await _genericAttributeService.GetAttributeAsync<string>(orderItem,
                        $"ProductMapping-{order.Id}-{orderItem.Id}", order.StoreId);

                    // Retrieve the stored mapping info if existed; otherwise, get the current mapping info
                    var mapping = mappingInfo != null
                        ? JsonConvert.DeserializeObject<NexportProductMapping>(mappingInfo)
                        : await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, order.StoreId) ??
                          await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);

                    if (mapping != null)
                    {
                        if (mapping.AssignWhenRedeemed.HasValue && mapping.AssignWhenRedeemed.Value)
                        {
                            model.ProductMappingIdForOpenEndedProduct = mapping.Id;
                            var listOfMappings = await _nexportService.GetAllProductMappingsByCatalogIdAsync(mapping.NexportCatalogId);

                            model.AvailableMappings = new List<SelectListItem>();

                            foreach (var productMapping in listOfMappings)
                            {
                                if (!productMapping.AutoRedeem && productMapping.NexportSyllabusId != null)
                                {
                                    var productMappingProduct = await _productService.GetProductByIdAsync(productMapping.NopProductId);
                                    if (productMappingProduct != null)
                                    {
                                        model.AvailableMappings.Add(new SelectListItem(productMappingProduct.Name, $"{productMapping.Id}"));
                                    }
                                }
                            }
                        }
                        else
                        {
                            model.SelectedProductMappingId = mapping.Id;

                            var mappings = new List<SelectListItem> { new(product.Name, $"{mapping.Id}") };
                            model.AvailableMappings = await mappings.OrderBy(x => x.Value).ToListAsync();
                        }
                    }
                }
            }
        }

        if (groupId != null)
        {
            var group = await _nexportService.GetWholesalePurchaseGroupAsync(groupId.Value);

            var groupModel = group.ToModel<NexportGroupModel>();
            if (groupModel != null)
                model.CurrentGroup = groupModel;
        }

        return model;
    }

    public virtual async Task<RedeemByEmailModel> PrepareRedeemByEmailModel(int? invoiceItemId, string email, int? productMappingId)
    {
        ArgumentNullException.ThrowIfNull(invoiceItemId);
        ArgumentNullException.ThrowIfNull(productMappingId);
        ArgumentException.ThrowIfNullOrEmpty(email);

        if (invoiceItemId < 1)
            throw new ArgumentException("Invalid invoice item id", nameof(invoiceItemId));

        if (productMappingId < 1)
            throw new ArgumentException("Invalid product mapping id", nameof(productMappingId));

        var model = new RedeemByEmailModel();
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(invoiceItemId.Value);

        if (invoiceItem != null)
        {
            if (productMappingId > 0)
            {
                var productMapping = await _nexportService.GetProductMappingById(productMappingId.Value);
                var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);

                model.InvoiceItemId = invoiceItemId.Value;
                model.Redeemed = invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.Assigned;
                model.ProductName = product?.Name;
                model.RedeemedDate = invoiceItem.UtcDateProcessed;

                var customer = await _customerService.GetCustomerByEmailAsync(email);
                if (customer != null)
                {
                    var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);

                    // Create new Nexport user and map to this customer if the mapping does not existed
                    if (userMapping == null)
                    {
                        await _nexportService.CreateAndMapNewNexportUserAsync(customer);
                        userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                    }

                    model.NexportUserId = userMapping.NexportUserId;
                }

                model.ProductMappingId = productMappingId.Value;
                model.Status = invoiceItem.RedemptionStatus;
                model.EnrollmentId = invoiceItem.RedemptionEnrollmentId;
                var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                if (order != null)
                {
                    var selectedMappingForOpenEndedProductStr =
                        await _genericAttributeService.GetAttributeAsync<string>(
                            invoiceItem,
                            $"SelectedMappingForOpenEndedProduct-{invoiceItem.Id}",
                            order.StoreId);

                    if (selectedMappingForOpenEndedProductStr != null)
                    {
                        var selectedMappingForOpenEndedProduct = JsonConvert.DeserializeObject<NexportProductMapping>(selectedMappingForOpenEndedProductStr);
                        if (selectedMappingForOpenEndedProduct != null)
                        {
                            product = await _productService.GetProductByIdAsync(selectedMappingForOpenEndedProduct.NopProductId);
                            if (product != null)
                            {
                                model.ProductName = product.Name;
                            }
                        }
                    }
                }
            }
        }

        return model;
    }

    public async Task<MapProductToCategoryModel> PrepareMapProductToCategoryModel()
    {
        var model = new MapProductToCategoryModel
        {
            AvailableCategories =
                (await _categoryService.GetAllCategoriesAsync())
                .Select(x => new SelectListItem(x.Name, $"{x.Id}"))
                .ToList()
        };

        return model;
    }

    /// <summary>
    /// Prepare category search model
    /// </summary>
    /// <param name="searchModel">Category search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the category search model
    /// </returns>
    public virtual async Task<NexportCategorySearchModel> PrepareCategorySearchModelAsync(NexportCategorySearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

        //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.All")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.PublishedOnly")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.UnpublishedOnly")
        });

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged category list model
    /// </summary>
    /// <param name="searchModel">Category search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the category list model
    /// </returns>
    public virtual async Task<NexportCategoryListModel> PrepareCategoryListModelAsync(NexportCategorySearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get categories
        var categories = await _nexportService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
            showHidden: true,
            storeId: searchModel.SearchStoreId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1),
            hasProductMapping: searchModel.HasProductMapping);

        //prepare grid model
        var model = await new NexportCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                //fill in model values from the entity
                var categoryModel = category.ToModel<NexportCategoryModel>();

                var mappings =
                    await _nexportService.GetAllNexportProductMappingsByCategoryIdAsync(categoryModel.Id);
                if (mappings.Count > 0)
                    categoryModel.IncludesProductMapping = true;

                //fill in additional values (not existing in the entity)
                categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumbAsync(category);
                categoryModel.SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false);

                return categoryModel;
            });
        });

        return model;
    }

    public async Task<NexportProductRedemptionStatusesModel> PrepareNexportProductRedemptionStatusesModel(Customer customer, int productId, int storeId)
    {
        var model = new NexportProductRedemptionStatusesModel();

        var product = await _productService.GetProductByIdAsync(productId);
        model.ProductId = product.Id;

        var store = await _storeContext.GetCurrentStoreAsync();
        var wholesaleOrderInfos = await _nexportService.GetAllWholesaleOrderInfosAsync(null, null, product.Name, null, customer.Id, storeId);

        IList<NexportGroupProductModel> groupProductModels = new List<NexportGroupProductModel>();

        foreach (var orderInfo in wholesaleOrderInfos)
        {
            if (orderInfo.NexportGroupId != null)
            {
                //skip group if not admin view and user doesn't have group permission
                if (!await _nexportService.HasGroupPermissionAsync(customer, orderInfo.NexportGroupId.Value))
                    continue;
            }

            model.Available += orderInfo.Available;
            model.Awaiting += orderInfo.Awaiting;
            model.Assigned += orderInfo.Redeemed;
        }

        return model;
    }

    public async Task<SubmitRedemptionUnassignmentRequestModel> PrepareSubmitUnassignmentRequestModel(Guid? groupId, Guid? invoiceItemId, int? productId, int? customerId)
    {
        var model = new SubmitRedemptionUnassignmentRequestModel();

        if (groupId != null)
        {
            var group = await _nexportService.GetWholesalePurchaseGroupAsync(groupId.Value);
            if (group != null)
                model.GroupName = group.NexportGroupName;
        }

        if (productId != null)
        {
            var product = await _productService.GetProductByIdAsync(productId.Value);
            model.ProductName = product.Name;
        }

        if (customerId != null)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            model.CustomerName = $"{customer.FirstName} {customer.LastName}";
        }

        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportDefaults.RedemptionUnassignmentRequestReasonsCacheKey, workingLanguage.Id);

        model.AvailableUnassignmentReasons = await _cacheManager.GetAsync(cacheKey,
            async () =>
            {
                var requestReasons = await _nexportService
                    .GetAllRedemptionUnassignmentRequestReasonsAsync();

                return await requestReasons.SelectAwait(async reason =>
                    new NexportRedemptionUnassignmentRequestReasonModel
                    {
                        Id = reason.Id,
                        Name = await _localizationService.GetLocalizedAsync(reason, x => x.Name)
                    }).ToListAsync();
            });


        return model;
    }

    public async Task<NexportRedemptionUnassignmentRequestListSearchModel>
        PrepareRedemptionUnassignmentRequestSearchModelAsync(NexportRedemptionUnassignmentRequestListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var availableStatusItems = await NexportRedemptionUnassignmentRequestStatus.Accepted.ToSelectListAsync(false);
        foreach (var statusItem in availableStatusItems)
        {
            searchModel.RequestStatusList.Add(statusItem);
        }

        searchModel.RequestStatusId = -1;
        searchModel.RequestStatusList.Insert(0, new SelectListItem
        {
            Value = "-1",
            Text = await _localizationService.GetResourceAsync("RedemptionUnassignmentRequests.SearchUnassignmentRequestStatus.All")
        });

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<NexportRedemptionRequestUnassignmentListModel> PrepareNexportRedemptionUnassignmentRequestListModel(NexportRedemptionUnassignmentRequestListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var currentTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();

        var startDateValue = !searchModel.StartDate.HasValue
            ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, currentTimeZone);
        var endDateValue = !searchModel.EndDate.HasValue
            ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, currentTimeZone).AddDays(1);
        var cancelRequestStatus = searchModel.RequestStatusId == -1
            ? null
            : (NexportRedemptionUnassignmentRequestStatus?)searchModel.RequestStatusId;

        // Get cancellation requests
        var unassignmentRequests =
            await _nexportService.SearchUnassignmentRequestsAsync(
                requestStatus: cancelRequestStatus,
                createdFromUtc: startDateValue, createdToUtc: endDateValue,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new NexportRedemptionRequestUnassignmentListModel().PrepareToGridAsync(searchModel, unassignmentRequests, () =>
        {
            return unassignmentRequests.SelectAwait(async unassignmentRequest =>
            {
                var requestModel = unassignmentRequest.ToModel<NexportRedemptionUnassignmentRequestModel>();

                var invoiceItem =
                   await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(unassignmentRequest.InvoiceItemId);

                requestModel.OrderId = invoiceItem?.OrderId ?? 0;
                requestModel.UtcCreatedDate =
                    _dateTimeHelper.ConvertToUserTime(
                        unassignmentRequest.UtcCreatedDate,
                        TimeZoneInfo.Utc,
                        await _dateTimeHelper.GetCustomerTimeZoneAsync(await _workContext.GetCurrentCustomerAsync()));

                var customer = await _customerService.GetCustomerByIdAsync(unassignmentRequest.RequestedByCustomerId);

                requestModel.CustomerInfo =
                    customer != null && await _customerService.IsRegisteredAsync(customer)
                        ? customer.Email
                        : await _localizationService.GetResourceAsync("Admin.Customers.Guest");

                return requestModel;
            });
        });
        return model;
    }

    public async Task<NexportRedemptionUnassignmentRequestReasonListModel>
        PrepareRedemptionUnassignmentRequestReasonListModelAsync(NexportRedemptionUnassignmentRequestReasonSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var reasons =
            (await _nexportService.GetAllRedemptionUnassignmentRequestReasonsAsync()).ToPagedList(searchModel);

        var model = new NexportRedemptionUnassignmentRequestReasonListModel().PrepareToGrid(searchModel, reasons, () =>
        {
            return reasons.Select(reason => reason.ToModel<NexportRedemptionUnassignmentRequestReasonModel>());
        });

        return model;
    }

    public async Task<NexportRedemptionUnassignmentRequestReasonModel> PrepareRedemptionUnassignmentRequestReasonModelAsync(
        NexportRedemptionUnassignmentRequestReasonModel model,
        NexportRedemptionUnassignmentRequestReason cancellationRequestReason, bool excludeProperties = false)
    {
        Func<NexportRedemptionUnassignmentRequestReasonLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (cancellationRequestReason != null)
        {
            model ??= cancellationRequestReason.ToModel<NexportRedemptionUnassignmentRequestReasonModel>();

            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(
                    cancellationRequestReason,
                    entity => entity.Name,
                    languageId, false, false);
            };
        }

        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        return model;
    }

    public async Task<NexportRedemptionUnassignmentRequestModel> PrepareRedemptionUnassignmentRequestModelAsync(
        NexportRedemptionUnassignmentRequestModel model, NexportRedemptionUnassignmentRequest unassignmentRequest,
        bool excludeProperties = false)
    {
        if (unassignmentRequest == null)
            return model;

        //fill in model values from the entity
        model ??= new NexportRedemptionUnassignmentRequestModel
        {
            Id = unassignmentRequest.Id,
            RequestedByCustomerId = unassignmentRequest.RequestedByCustomerId,
        };

        var customer = await _customerService.GetCustomerByIdAsync(unassignmentRequest.RequestedByCustomerId);

        model.UtcCreatedDate = _dateTimeHelper.ConvertToUserTime(
            unassignmentRequest.UtcCreatedDate,
            TimeZoneInfo.Utc,
            await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));

        model.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
            ? customer.Email
            : await _localizationService.GetResourceAsync("Admin.Customers.Guest");

        model.InvoiceItemId = unassignmentRequest.InvoiceItemId;

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(model.InvoiceItemId);
        model.OrderId = invoiceItem?.OrderId ?? 0;

        if (excludeProperties)
            return model;

        model.ReasonForUnassignment = unassignmentRequest.ReasonForUnassignment;
        model.CustomerComments = unassignmentRequest.CustomerComments;
        model.StaffNotes = unassignmentRequest.StaffNotes;
        model.RequestStatus = unassignmentRequest.RequestStatus;

        return model;
    }

    public async Task<SubmitRedemptionUnassignmentRequestModel>
        PrepareSubmitRedemptionUnassignmentRequestModelAsync(SubmitRedemptionUnassignmentRequestModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportDefaults.RedemptionUnassignmentRequestReasonsCacheKey, workingLanguage.Id);


        model.AvailableUnassignmentReasons = await _cacheManager.GetAsync(cacheKey,
            async () =>
            {
                var requestReasons = await _nexportService
                    .GetAllRedemptionUnassignmentRequestReasonsAsync();
                return await requestReasons.SelectAwait(async reason =>
                    new NexportRedemptionUnassignmentRequestReasonModel
                    {
                        Id = reason.Id,
                        Name = await _localizationService.GetLocalizedAsync(reason, x => x.Name)
                    }).ToListAsync();
            });

        return model;
    }

    public virtual Task<NexportFundingPoolSearchModel> PrepareNexportFundingPoolSearchModelAsync(NexportFundingPoolSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public virtual async Task<NexportFundingPoolListModel> PrepareNexportFundingPoolListModelAsync(NexportFundingPoolSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var fundingPools =
            await _nexportWholesaleService.GetAllFundingPoolsPagination(searchModel.Name, searchModel.Code, searchModel.Description, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new NexportFundingPoolListModel().PrepareToGridAsync(searchModel,
            fundingPools, () =>
            {
                return fundingPools.SelectAwait(async fundingPool =>
                {
                    var requestModel = fundingPool.ToModel<NexportFundingPoolModel>();
                    return requestModel;
                });
            });

        return model;
    }

    public virtual async Task<NexportFundingPoolModel> PrepareNexportFundingPoolModelAsync(NexportFundingPoolModel model, NexportFundingPool fundingPool)
    {
        if (fundingPool != null)
        {
            model ??= fundingPool.ToModel<NexportFundingPoolModel>();
        }

        return model;
    }

    public virtual async Task<NexportReturnRequestSearchModel> PrepareNexportReturnRequestSearchModelAsync(
        NexportReturnRequestSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare available return request statuses
        await _baseAdminModelFactory.PrepareReturnRequestStatusesAsync(searchModel.ReturnRequestStatusList, false);

        //for some reason, the standard default value (0) for the "All" item is already used for the "Pending" status, so here we use -1
        searchModel.ReturnRequestStatusId = -1;
        searchModel.ReturnRequestStatusList.Insert(0, new SelectListItem
        {
            Value = "-1",
            Text = await _localizationService.GetResourceAsync("Admin.ReturnRequests.SearchReturnRequestStatus.All")
        });

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<NexportReturnRequestListModel> PrepareNexportReturnRequestListModelAsync(
        NexportReturnRequestSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter emails
        var startDateValue = !searchModel.StartDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var endDateValue = !searchModel.EndDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
        var returnRequestStatus = searchModel.ReturnRequestStatusId == -1 ? null : (ReturnRequestStatus?)searchModel.ReturnRequestStatusId;

        //get return requests
        var returnRequests = await _returnRequestService.SearchReturnRequestsAsync(customNumber: searchModel.CustomNumber,
            rs: returnRequestStatus,
            createdFromUtc: startDateValue,
            createdToUtc: endDateValue,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new NexportReturnRequestListModel().PrepareToGridAsync(searchModel, returnRequests, () =>
        {
            return returnRequests.SelectAwait(async returnRequest => await PrepareReturnRequestModelAsync(null, returnRequest));
        });

        return model;
    }

    public virtual async Task<NexportReturnRequestModel> PrepareReturnRequestModelAsync(ReturnRequestModel model,
        ReturnRequest returnRequest, bool excludeProperties = false)
    {
        if (returnRequest == null)
            return AutoMapperConfiguration.Mapper.Map<NexportReturnRequestModel>(model);

        var newModel = model == null
            ? returnRequest.ToModel<NexportReturnRequestModel>()
            : AutoMapperConfiguration.Mapper.Map<NexportReturnRequestModel>(model);

        var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

        newModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc);

        newModel.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
            ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
        newModel.UploadedFileGuid = (await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId))?.DownloadGuid ?? Guid.Empty;
        newModel.ReturnRequestStatusStr = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus);
        var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
        if (orderItem != null)
        {
            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            newModel.ProductId = product.Id;
            newModel.ProductName = product.Name;
            newModel.OrderId = order.Id;
            newModel.AttributeInfo = orderItem.AttributeDescription;
            newModel.CustomOrderNumber = order.CustomOrderNumber;

            var productMapping = await _nexportService.GetProductMappingByNopProductId(product.Id);
            if (productMapping != null)
            {
                newModel.IsNexportPurchase = true;
                var isWholesalePurchase = await _genericAttributeService.GetAttributeAsync<bool>(order, "IsWholesaleOrder", order.StoreId);
                newModel.IsNexportWholesalePurchase = isWholesalePurchase;
            }
        }

        if (excludeProperties)
            return newModel;

        newModel.ReasonForReturn = returnRequest.ReasonForReturn;
        newModel.RequestedAction = returnRequest.RequestedAction;
        newModel.CustomerComments = returnRequest.CustomerComments;
        newModel.StaffNotes = returnRequest.StaffNotes;
        newModel.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;

        return newModel;
    }

    public virtual async Task<NexportReturnRequestModel> PrepareNexportReturnRequestModelAsync(
        NexportReturnRequestModel model, ReturnRequest returnRequest, bool excludeProperties = false)
    {
        if (returnRequest == null)
            return model;

        //fill in model values from the entity
        model ??= returnRequest.ToModel<NexportReturnRequestModel>();

        var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

        model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc);

        model.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
            ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
        model.UploadedFileGuid = (await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId))?.DownloadGuid ?? Guid.Empty;
        model.ReturnRequestStatusStr = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus);
        var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
        if (orderItem != null)
        {
            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            model.ProductId = product.Id;
            model.ProductName = product.Name;
            model.OrderId = order.Id;
            model.AttributeInfo = orderItem.AttributeDescription;
            model.CustomOrderNumber = order.CustomOrderNumber;

            var productMapping = await _nexportService.GetProductMappingByNopProductId(product.Id);
            if (productMapping != null)
            {
                model.IsNexportPurchase = true;
                var isWholesalePurchase = await _genericAttributeService.GetAttributeAsync<bool>(order, "IsWholesaleOrder", order.StoreId);
                model.IsNexportWholesalePurchase = isWholesalePurchase;
            }
        }

        if (excludeProperties)
            return model;

        model.ReasonForReturn = returnRequest.ReasonForReturn;
        model.RequestedAction = returnRequest.RequestedAction;
        model.CustomerComments = returnRequest.CustomerComments;
        model.StaffNotes = returnRequest.StaffNotes;
        model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;

        return model;
    }
}