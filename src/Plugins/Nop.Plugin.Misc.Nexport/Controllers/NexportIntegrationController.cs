using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Data.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    [ResponseCache(Duration = 0, NoStore = true)]
    public class NexportIntegrationController : BasePluginController,
        IConsumer<CustomerRegisteredEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<EntityUpdatedEvent<Order>>,
        IConsumer<EntityDeletedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Customer>>,
        IConsumer<EntityInsertedEvent<Store>>,
        IConsumer<EntityDeletedEvent<Store>>
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<NexportProductMapping> _nexportProductRepository;

        private readonly NexportSettings _nexportSettings;
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IOrderModelFactory _orderModelFactory;

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly ICopyProductService _copyProductService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICategoryService _categoryService;

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductTagService _productTagService;
        private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;

        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;

        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Constructor

        public NexportIntegrationController(
            NexportSettings nexportSettings,
            NexportService nexportService,
            IRepository<NexportProductMapping> nexportProductRepository,
            IRepository<Product> productRepository,
            INexportPluginModelFactory nexportPluginModelFactory,
            IProductModelFactory productModelFactory,
            IOrderModelFactory orderModelFactory,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerActivityService customerActivityService,
            IProductService productService,
            IStoreService storeService,
            IOrderService orderService,
            ICategoryService categoryService,
            ICustomerService customerService,
            IDiscountService discountService,
            ICopyProductService copyProductService,
            IShoppingCartService shoppingCartService,
            ISettingService settingService,
            IPermissionService permissionService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IGenericAttributeService genericAttributeService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
            IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
            ILogger logger,
            IWebHelper webHelper,
            VendorSettings vendorSettings)
        {
            _productRepository = productRepository;
            _nexportProductRepository = nexportProductRepository;
            _productModelFactory = productModelFactory;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _orderModelFactory = orderModelFactory;

            _workContext = workContext;
            _storeContext = storeContext;

            _customerActivityService = customerActivityService;
            _productService = productService;
            _storeService = storeService;
            _orderService = orderService;
            _categoryService = categoryService;
            _customerService = customerService;
            _discountService = discountService;
            _copyProductService = copyProductService;
            _shoppingCartService = shoppingCartService;

            _nexportSettings = nexportSettings;
            _nexportService = nexportService;
            _settingService = settingService;
            _permissionService = permissionService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _genericAttributeService = genericAttributeService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;
            _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;

            _logger = logger;
            _webHelper = webHelper;

            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        protected virtual void UpdateNexportRegistrationFieldLocales(NexportRegistrationField registrationField, NexportRegistrationFieldModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(registrationField,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        #endregion

        #region General Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpGet]
        public IActionResult SearchNexportDirectory(string searchTerm, int? page = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return Configure();

            var result = _nexportService.SearchNexportDirectory(searchTerm, page);

            return new JsonResult(result);
        }

        #endregion

        #region Plugin Configuration Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [ImportModelState]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                Url = _nexportSettings.Url,
                NexportAuthenticationToken = _nexportSettings.AuthenticationToken,
                UtcExpirationDate = _nexportSettings.UtcExpirationDate,
                RootOrganizationId = _nexportSettings.RootOrganizationId,
                MerchantAccountId = _nexportSettings.MerchantAccountId
            };

            return View("~/Plugins/Misc.Nexport/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        [ExportModelState]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                _nexportService.GenerateNewNexportToken(model);
            }

            return RedirectToAction("Configure");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setserverurl")]
        [ExportModelState]
        public IActionResult SetServerUrl(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                if (model.Url.IsValidUrl())
                {
                    _nexportSettings.Url = model.Url;
                    _settingService.SaveSetting(_nexportSettings);

                    _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
                }
            }
            catch (Exception ex)
            {
                var errMsg = "Cannot set the server url!";
                _logger.Error(errMsg, ex);
                _notificationService.ErrorNotification(errMsg);
            }

            return RedirectToAction("Configure");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AdminAntiForgery]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setrootorganizationid")]
        [ExportModelState]
        public IActionResult SetRootOrganization(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                _nexportSettings.RootOrganizationId = model.RootOrganizationId;
                _settingService.SaveSetting(_nexportSettings);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                var errMsg = "Cannot set the root organization!";
                _logger.Error(errMsg, ex);
                _notificationService.ErrorNotification(errMsg);
            }

            return RedirectToAction("Configure");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AdminAntiForgery]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setmerchantaccountid")]
        [ExportModelState]
        public IActionResult SetMerchantAccount(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                _nexportSettings.MerchantAccountId = model.MerchantAccountId;
                _settingService.SaveSetting(_nexportSettings);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                var errMsg = "Cannot set the merchant account!";
                _logger.Error(errMsg, ex);
                _notificationService.ErrorNotification(errMsg);
            }

            return RedirectToAction("Configure");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Route("Admin/Store/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("setsubscriptionorgid")]
        public IActionResult SetNexportSubscriptionOrganizationId(StoreModel model, [FromForm(Name = "NexportSubscriptionOrgId")] Guid subOrgId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(model.Id, false);
            if (store == null)
                return RedirectToAction("List", "Store");

            _genericAttributeService.SaveAttribute(store, "NexportSubscriptionOrganizationId", subOrgId, store.Id);

            _notificationService.SuccessNotification("Success update Nexport subscription organization");

            return RedirectToAction("Edit", "Store", new { id = store.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Route("Admin/Store/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("savenexportstoreconfig")]
        public IActionResult SaveNexportStoreConfiguration(NexportStoreModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(model.Id, false);
            if (store == null)
                return RedirectToAction("List", "Store");

            _genericAttributeService.SaveAttribute(store, NexportDefaults.NEXPORT_STORE_SALE_MODEL_SETTING_KEY,
                model.SaleModel, store.Id);
            _genericAttributeService.SaveAttribute(store, NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY,
                model.AllowRepurchaseFailedCourses, store.Id);
            _genericAttributeService.SaveAttribute(store, NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY,
                model.AllowRepurchasePassedCourses, store.Id);
            _genericAttributeService.SaveAttribute(store, NexportDefaults.HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY,
                model.HideSectionCEUsInProductPage, store.Id);

            _notificationService.SuccessNotification("Success update Nexport store configuration");

            return RedirectToAction("Edit", "Store", new { id = store.Id });
        }

        #endregion

        #region User Configuration Actions

        [Area(AreaNames.Admin)]
        [HttpPost]
        [PublicAntiForgery]
        [Route("Admin/Customer/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("setnexportuserid")]
        public IActionResult MapNexportUser(CustomerModel model, [FromForm(Name = "NexportUserId")] Guid nexportUserId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List", "Customer");

            if (nexportUserId != Guid.Empty)
            {
                var currentUserMapping = _nexportService.FindUserMappingByCustomerId(customer.Id);
                if (currentUserMapping == null)
                {
                    _nexportService.InsertUserMapping(new NexportUserMapping
                    {
                        NexportUserId = nexportUserId,
                        NopUserId = customer.Id
                    });
                }
                else
                {
                    currentUserMapping.NexportUserId = nexportUserId;

                    _nexportService.UpdateUserMapping(currentUserMapping);
                }

                _notificationService.SuccessNotification("Success update Nexport user mapping");
            }

            return RedirectToAction("Edit", "Customer", new { id = customer.Id });
        }

        #endregion

        #region Product Mapping Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult GetCatalogList(Guid? orgId, int nopProductId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts) || string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = new NexportCatalogSearchModel { OrgId = orgId, NopProductId = nopProductId };

            model.SetGridPageSize();

            return View("~/Plugins/Misc.Nexport/Views/MapNexportProductList.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult CatalogList(NexportCatalogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts) || string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportCatalogListModel(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public IActionResult SyllabusList(NexportSyllabusListSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts) || string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportSyllabusListModel(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult ListCatalogs()
        {
            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedView();

            return View("~/Plugins/Misc.Nexport/Views/Catalog/ManageCatalogs.cshtml");
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult CatalogList(Guid? orgId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = new NexportCatalogSearchModel { OrgId = orgId };

            model.SetGridPageSize();

            return View("~/Plugins/Misc.Nexport/Views/Catalog/CatalogList.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AdminAntiForgery]
        public IActionResult GetCatalogs(NexportCatalogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportCatalogListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AdminAntiForgery]
        public IActionResult GetSyllabuses(NexportSyllabusListSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportSyllabusListModel(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        [AdminAntiForgery]
        public IActionResult ProductMappingDetailsPopup(int mappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productMapping = _nexportService.GetProductMappingById(mappingId);

            var model = _nexportPluginModelFactory.PrepareNexportProductMappingModel(productMapping, true);

            return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GetProductMappings(NexportProductMappingSearchModel searchModel, Guid? nexportProductId, NexportProductTypeEnum? nexportProductType, int? nopProductId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = new NexportProductMappingListModel();

            if (nexportProductId.HasValue)
            {
                model = _nexportPluginModelFactory.PrepareNexportProductMappingListModel(searchModel, nexportProductId.Value, nexportProductType.Value);
            }
            else if (nopProductId.HasValue)
            {
                model = _nexportPluginModelFactory.PrepareNexportProductMappingListModel(searchModel, nopProductId.Value);
            }

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [AdminAntiForgery]
        public IActionResult EditMapping(NexportProductMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var productMapping = _nexportService.GetProductMappingById(model.Id)
                ?? throw new ArgumentException("No product mapping found with the specified id");

            if (ModelState.IsValid)
            {
                // Fill entity from product
                productMapping = model.ToEntity(productMapping);

                if (productMapping.NexportSubscriptionOrgId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(model.NexportSubscriptionOrgName))
                    {
                        var organizationDetails = _nexportService.GetOrganizationDetails(productMapping.NexportSubscriptionOrgId.Value);
                        productMapping.NexportSubscriptionOrgName = organizationDetails.Name;
                        productMapping.NexportSubscriptionOrgShortName = organizationDetails.ShortName;
                    }
                }
                else
                {
                    productMapping.NexportSubscriptionOrgName = null;
                    productMapping.NexportSubscriptionOrgShortName = null;
                }

                productMapping.UtcLastModifiedDate = DateTime.UtcNow;

                _nexportService.UpdateNexportProductMapping(productMapping);

                var questionMappings =
                    _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id);

                var currentQuestionIds = questionMappings.Select(x => x.QuestionId).ToList();
                var removalQuestionIds = currentQuestionIds.Except(model.SupplementalInfoQuestionIds);
                var additionalQuestionIds = model.SupplementalInfoQuestionIds.Except(currentQuestionIds);

                foreach (var questionId in additionalQuestionIds)
                {
                    _nexportService.InsertNexportSupplementalInfoQuestionMapping(
                                new NexportSupplementalInfoQuestionMapping
                                {
                                    ProductMappingId = productMapping.Id,
                                    QuestionId = questionId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                }

                foreach (var questionId in removalQuestionIds)
                {
                    var deletingMapping = questionMappings.FirstOrDefault(x => x.QuestionId == questionId);
                    if (deletingMapping != null)
                    {
                        _nexportService.DeleteNexportSupplementalInfoQuestionMapping(deletingMapping);
                    }
                }

                if (!continueEditing)
                {
                    ViewBag.ClosePage = true;
                }

                ViewBag.RefreshPage = true;

                model = _nexportPluginModelFactory.PrepareNexportProductMappingModel(productMapping, true);
            }

            return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult DeleteMapping(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var mapping = _nexportService.GetProductMappingById(id) ??
                          throw new Exception($"No nexport mapping found with the specified id {id}");

            _nexportService.DeleteNexportProductMapping(mapping);

            var groupMembershipMappings = _nexportService.GetProductGroupMembershipMappings(mapping.Id);
            foreach (var groupMembershipMapping in groupMembershipMappings)
            {
                _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);
            }

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GetProductGroupMembershipMappings(int nexportProductMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportProductMappingGroupMembershipListModel(
                new NexportProductGroupMembershipMappingSearchModel(), nexportProductMappingId);

            return Json(model);
        }

        [Area("Admin")]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult AddGroupMembershipMapping(int nexportProductMappingId, Guid nexportGroupId, string nexportGroupName, string nexportGroupShortName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (nexportGroupId == Guid.Empty)
                throw new ArgumentException("Group Id cannot be empty Guid", nameof(nexportGroupId));

            var productMapping = _nexportService.GetProductMappingById(nexportProductMappingId);
            if (productMapping == null)
                throw new ArgumentException("No nexport mapping found with the specified id", nameof(nexportProductMappingId));

            _nexportService.InsertNexportProductGroupMembershipMapping(new NexportProductGroupMembershipMapping
            {
                NexportGroupId = nexportGroupId,
                NexportGroupName = nexportGroupName,
                NexportGroupShortName = nexportGroupShortName,
                NexportProductMappingId = productMapping.Id
            });

            return Json(new
            {
                Result = true
            });
        }

        [Area("Admin")]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult DeleteGroupMembershipMapping(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var groupMembershipMapping = _nexportService.GetProductGroupMembershipMappingById(id);
            if (groupMembershipMapping == null)
                throw new Exception($"No nexport group membership mapping found with the specified id {id}");

            _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult MapNexportProductPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View("~/Plugins/Misc.Nexport/Views/MapNexportProduct.cshtml");
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [FormValueRequired("save")]
        [AdminAntiForgery]
        public IActionResult MapNexportProductPopup(MapNexportProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            dynamic result = new ExpandoObject();

            try
            {
                _nexportService.MapNexportProduct(model);

                ViewBag.RefreshPage = true;

                ViewBag.ClosePage = false;

                var newMapping = _nexportService.GetProductMappingByNopProductId(model.NopProductId, model.StoreId);

                result.MappingId = newMapping.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Error occurred while mapping the product [{model.NexportProductId}] with the Nexport product [{model.NexportProductId}]",
                    ex, _workContext.CurrentCustomer);

                result.Error = $"Cannot map the product [{model.NexportProductId}]  with the Nexport product [{model.NexportProductId}]";

                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult MapProductPopup(Guid nexportProductId, NexportProductTypeEnum nexportProductType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            // Prepare model
            var model = _nexportPluginModelFactory.PrepareNexportProductMappingSearchModel(new NexportProductMappingSearchModel());

            return View("~/Plugins/Misc.Nexport/Views/MapProductWithNexport.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [FormValueRequired("save")]
        [AdminAntiForgery]
        public IActionResult MapProductPopup(MapProductToNexportProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            _nexportService.MapNexportProduct(model);

            ViewBag.RefreshPage = true;

            ViewBag.ClosePage = true;

            return View("~/Plugins/Misc.Nexport/Views/MapProductWithNexport.cshtml", new NexportProductMappingSearchModel());
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult MapProductPopupList(NexportProductMappingSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            // Prepare model
            var model = _nexportPluginModelFactory.PrepareMapProductToNexportProductListModel(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult MapNewProductPopup(Guid nexportProductId, NexportProductTypeEnum nexportProductType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            // Prepare model
            var model = _productModelFactory.PrepareProductModel(new ProductModel(), null);

            return View("~/Plugins/Misc.Nexport/Views/AddNewNopProductWithMapping.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult ShowProductDetails(Guid nexportProductId, Guid nexportCatalogId, Guid? nexportSyllabusId, NexportProductTypeEnum nexportProductType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            switch (nexportProductType)
            {
                case NexportProductTypeEnum.Catalog:
                    var catalogModel = _nexportService.GetCatalogDetails(nexportProductId);

                    return View("~/Plugins/Misc.Nexport/Views/Catalog/CatalogDetails.cshtml", catalogModel);

                case NexportProductTypeEnum.Section:
                    if (nexportSyllabusId != null)
                    {
                        var sectionModel = _nexportService.GetSectionDetails(nexportSyllabusId.Value);

                        return View("~/Plugins/Misc.Nexport/Views/Syllabus/Section/SectionDetails.cshtml", sectionModel);
                    }

                    throw new ArgumentNullException(nameof(nexportSyllabusId), "Syllabus Id cannot be null!");

                case NexportProductTypeEnum.TrainingPlan:
                    if (nexportSyllabusId != null)
                    {
                        var trainingPlanModel = _nexportService.GetTrainingPlanDetails(nexportSyllabusId.Value);

                        return View("~/Plugins/Misc.Nexport/Views/Syllabus/TrainingPlan/TrainingPlanDetails.cshtml", trainingPlanModel);
                    }

                    throw new ArgumentNullException(nameof(nexportSyllabusId), "Syllabus Id cannot be null!");

                default:
                    goto case NexportProductTypeEnum.Catalog;
            }
        }

        #endregion

        #region Product Management Actions

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Route("Admin/Product/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("syncnexportproduct")]
        public IActionResult SyncNexportProductWithNopProduct(ProductModel model, [FromForm(Name = "NexportMappingId")] int mappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List", "Product");

            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                product = model.ToEntity(product);

                _nexportService.SyncNexportProduct(mappingId, product);

                _notificationService.SuccessNotification("The product has been synchronized successfully with Nexport data");
            }

            return RedirectToAction("Edit", "Product", new { id = model.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult CopyProduct(ProductModel model, bool copyProductMapping = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(copyModel.Id);

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && originalProduct.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List", "Product");

                var newProduct = _copyProductService.CopyProduct(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyImages);

                if (copyProductMapping)
                {
                    _nexportService.CopyProductMappings(originalProduct, newProduct);
                }

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Copied"));

                return RedirectToAction("Edit", "Product", new { id = newProduct.Id });
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                return RedirectToAction("Edit", "Product", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Category Management Actions

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Route("/Admin/Category/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("savenexportcategoryoptions")]
        public IActionResult SaveNexportCategoryOptions(NexportCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(model.Id);
            if (category == null)
                return RedirectToAction("List", "Category");

            _genericAttributeService.SaveAttribute(category, NexportDefaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY,
                model.LimitSingleProductPurchase);

            _genericAttributeService.SaveAttribute(category, NexportDefaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY,
                model.AutoSwapProductPurchase);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Updated"));

            return RedirectToAction("Edit", "Category", new { id = category.Id });
        }

        #endregion

        #region Supplemental Question

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult ListSupplementalInfoQuestion()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoQuestionSearchModel(new NexportSupplementalInfoQuestionSearchModel());

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/List.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult ListSupplementalInfoQuestion(NexportSupplementalInfoQuestionSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoQuestionListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult AddSupplementalInfoQuestion()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionModel(new NexportSupplementalInfoQuestionModel(), null);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Add.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [AdminAntiForgery]
        public IActionResult AddSupplementalInfoQuestion(NexportSupplementalInfoQuestionModel model,
            bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var question = model.ToEntity<NexportSupplementalInfoQuestion>();
                question.UtcDateCreated = DateTime.UtcNow;

                _nexportService.InsertNexportSupplementalInfoQuestion(question);

                _notificationService.SuccessNotification("A new supplemental info question has been added.");

                if (!continueEditing)
                    return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

                return RedirectToAction("EditSupplementalInfoQuestion", "NexportIntegration", new { id = question.Id });
            }

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Add.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult EditSupplementalInfoQuestion(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var supplementalInfoQuestion = _nexportService.GetNexportSupplementalInfoQuestionById(id);
            if (supplementalInfoQuestion == null)
                return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionModel(null, supplementalInfoQuestion);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult EditSupplementalInfoQuestion(NexportSupplementalInfoQuestionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var supplementalInfoQuestion = _nexportService.GetNexportSupplementalInfoQuestionById(model.Id);
            if (supplementalInfoQuestion == null)
                return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

            if (ModelState.IsValid)
            {
                supplementalInfoQuestion = model.ToEntity(supplementalInfoQuestion);
                _nexportService.UpdateNexportSupplementalInfoQuestion(supplementalInfoQuestion);

                _notificationService.SuccessNotification("Successfully update supplemental info question");

                if (!continueEditing)
                    return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

                return RedirectToAction("EditSupplementalInfoQuestion", "NexportIntegration", new { id = supplementalInfoQuestion.Id });
            }

            //prepare model
            model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionModel(model, supplementalInfoQuestion);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public virtual IActionResult DeleteSupplementalInfoQuestion(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var supplementalInfoQuestion = _nexportService.GetNexportSupplementalInfoQuestionById(id);
            if (supplementalInfoQuestion == null)
                return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

            _nexportService.DeleteNexportSupplementalInfoQuestion(supplementalInfoQuestion);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Deleted"));

            return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public virtual IActionResult DeleteSelectedSupplementalInfoQuestion(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                _nexportService.DeleteNexportSupplementalInfoQuestions(_nexportService.GetNexportSupplementalInfoQuestionsByIds(selectedIds.ToArray()));
            }

            return Json(new { Result = true });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public virtual IActionResult SupplementalInfoOptionList(NexportSupplementalInfoOptionSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(searchModel.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental info question found with the specified id");

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionListModel(searchModel, question);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public virtual IActionResult SupplementalInfoOptionCreatePopup(int questionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(questionId)
                           ?? throw new ArgumentException("No Nexport supplemental info question found with the specified id",
                               nameof(questionId));

            var model = _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoOptionModel(new NexportSupplementalInfoOptionModel(), question, null);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public virtual IActionResult SupplementalInfoOptionCreatePopup(NexportSupplementalInfoOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(model.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental question found with the specified id");

            if (ModelState.IsValid)
            {
                var option = model.ToEntity<NexportSupplementalInfoOption>();
                option.UtcDateCreated = DateTime.UtcNow;

                _nexportService.InsertNexportSupplementalInfoOption(option);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
            }

            model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionModel(model, question, null);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public virtual IActionResult SupplementalInfoOptionEditPopup(int optionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var option = _nexportService.GetNexportSupplementalInfoOptionById(optionId)
                ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id", nameof(optionId));

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(option.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental info question found with the specified id");

            var model = _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoOptionModel(null, question, option);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public virtual IActionResult SupplementalInfoOptionEditPopup(NexportSupplementalInfoOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var option = _nexportService.GetNexportSupplementalInfoOptionById(model.Id)
                         ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id");

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(option.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental question found with the specified id");

            if (ModelState.IsValid)
            {
                option = model.ToEntity(option);

                _nexportService.UpdateNexportSupplementalInfoOption(option);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
            }

            model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionModel(model, question, option);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public virtual IActionResult DeleteSupplementalInfoOption(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var option = _nexportService.GetNexportSupplementalInfoOptionById(id)
                         ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id", nameof(id));

            _nexportService.DeleteNexportSupplementalInfoOption(option);

            return new NullJsonResult();
        }

        [Area("Admin")]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult AddSupplementalInfoOptionGroupAssociation(int optionId, Guid nexportGroupId, string nexportGroupName, string nexportGroupShortName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (nexportGroupId == Guid.Empty)
                throw new ArgumentException("Group Id cannot be empty Guid", nameof(nexportGroupId));

            var supplementalInfoOption = _nexportService.GetNexportSupplementalInfoOptionById(optionId)
                ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id", nameof(optionId));

            _nexportService.InsertNexportSupplementalInfoOptionGroupAssociation(new NexportSupplementalInfoOptionGroupAssociation
            {
                NexportGroupId = nexportGroupId,
                NexportGroupName = nexportGroupName,
                NexportGroupShortName = nexportGroupShortName,
                OptionId = supplementalInfoOption.Id,
                IsActive = true,
                UtcDateCreated = DateTime.UtcNow
            });

            return Json(new
            {
                Result = true
            });
        }

        [Area("Admin")]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult DeleteSupplementalInfoOptionGroupAssociation(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var groupAssociation = _nexportService.GetNexportSupplementalInfoOptionGroupAssociationById(id)
                    ?? throw new Exception($"No Nexport supplemental info option group association found with the specified id {id}");

            _nexportService.DeleteNexportSupplementalInfoOptionGroupAssociation(groupAssociation);

            return new NullJsonResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult ChangeSupplementalInfoOptionGroupAssociationStatus(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var groupAssociation = _nexportService.GetNexportSupplementalInfoOptionGroupAssociationById(id)
                                   ?? throw new Exception($"No Nexport supplemental info option group association found with the specified id {id}");

            groupAssociation.IsActive = !groupAssociation.IsActive;
            groupAssociation.UtcDateModified = DateTime.UtcNow;

            _nexportService.UpdateNexportSupplementalInfoOptionGroupAssociation(groupAssociation);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GetSupplementalInfoOptionGroupAssociations(int optionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionGroupAssociationListModel(
                new NexportSupplementalInfoOptionGroupAssociationSearchModel(), optionId);

            return Json(model);
        }

        public IActionResult AnswerSupplementalInfoQuestion(string returnUrl)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var requirements = _nexportService.GetNexportRequiredSupplementalInfos(_workContext.CurrentCustomer.Id,
                _storeContext.CurrentStore.Id);

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoAnswerQuestionModel(
                 requirements.Select(x => x.QuestionId).ToList(), _workContext.CurrentCustomer,
                _storeContext.CurrentStore);

            if (model.QuestionWithoutAnswerIds.Count == 0)
                return Redirect($"{_storeContext.CurrentStore.Url}{returnUrl.TrimStart('/')}");

            model.ReturnUrl = returnUrl;

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/AnswerQuestions.cshtml", model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public IActionResult SaveSupplementalInfoAnswer(SaveSupplementalInfoAnswers request)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            foreach (var answer in request.Answers)
            {
                foreach (var optionId in answer.Options)
                {
                    var newAnswer = new NexportSupplementalInfoAnswer
                    {
                        CustomerId = _workContext.CurrentCustomer.Id,
                        StoreId = _storeContext.CurrentStore.Id,
                        QuestionId = answer.QuestionId,
                        OptionId = optionId,
                        Status = NexportSupplementalInfoAnswerStatus.NotProcessed,
                        UtcDateCreated = DateTime.UtcNow
                    };

                    _nexportService.InsertNexportSupplementalInfoAnswer(newAnswer);

                    _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(new NexportSupplementalInfoAnswerProcessingQueueItem
                    {
                        AnswerId = newAnswer.Id,
                        UtcDateCreated = DateTime.UtcNow
                    });
                }

                var requiredSupplementalInfos = _nexportService.GetNexportRequiredSupplementalInfos(
                    _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id,
                    answer.QuestionId);

                foreach (var requirement in requiredSupplementalInfos)
                {
                    _nexportService.DeleteNexportRequiredSupplementalInfo(requirement);
                }
            }

            return Json(new
            {
                Result = true
            });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult GetCustomerSupplementalInfoQuestions(NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult GetCustomerSupplementalInfoAnswers(NexportSupplementalInfoAnswerListSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportSupplementalInfoAnswerListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult EditCustomerSupplementalInfoAnsweredQuestion(int customerId, int storeId, int questionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var store = _storeService.GetStoreById(storeId)
                           ?? throw new Exception($"No store found with the specified id {storeId}");

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(questionId)
                           ?? throw new Exception($"No Nexport supplemental info question found with the specified id {questionId}");

            var model = _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModel(customer, store, question);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/EditCustomerSupplementalInfoAnsweredQuestion.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult EditCustomerSupplementalInfoAnsweredQuestion(int customerId, int storeId, EditSupplementInfoAnswerRequestModel editModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var store = _storeService.GetStoreById(storeId)
                        ?? throw new Exception($"No store found with the specified id {storeId}");

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(editModel.QuestionId)
                           ?? throw new Exception($"No Nexport supplemental info question found with the specified id {editModel.QuestionId}");

            var answers = _nexportService.GetNexportSupplementalInfoAnswers(customerId, storeId, editModel.QuestionId)
                          ?? throw new Exception($"No Nexport supplemental info answers found for the question with id {editModel.QuestionId}");

            if (ModelState.IsValid)
            {
                if (question.Type == NexportSupplementalInfoQuestionType.SingleOption)
                {
                    var updatingAnswer = answers.First();
                    var newOption = editModel.OptionIds[0];
                    if (updatingAnswer.OptionId != newOption)
                    {
                        var memberships =
                            _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(updatingAnswer.Id);

                        updatingAnswer.OptionId = newOption;
                        updatingAnswer.Status = NexportSupplementalInfoAnswerStatus.Modified;
                        updatingAnswer.UtcDateModified = DateTime.UtcNow;

                        _nexportService.UpdateNexportSupplementalInfoAnswer(updatingAnswer);

                        _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                            new NexportSupplementalInfoAnswerProcessingQueueItem
                            {
                                AnswerId = updatingAnswer.Id,
                                UtcDateCreated = DateTime.UtcNow
                            });

                        foreach (var membership in memberships)
                        {
                            _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    NexportMembershipId = membership.NexportMembershipId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }
                }
                else if (question.Type == NexportSupplementalInfoQuestionType.MultipleOptions)
                {
                    var newOptionIds = editModel.OptionIds ?? new List<int>();
                    foreach (var newOption in newOptionIds)
                    {
                        var newAnswer = answers.FirstOrDefault(a => a.OptionId == newOption);
                        if (newAnswer == null)
                        {
                            newAnswer = new NexportSupplementalInfoAnswer
                            {
                                CustomerId = customerId,
                                StoreId = storeId,
                                OptionId = newOption,
                                QuestionId = editModel.QuestionId,
                                Status = NexportSupplementalInfoAnswerStatus.NotProcessed,
                                UtcDateCreated = DateTime.UtcNow
                            };

                            _nexportService.InsertNexportSupplementalInfoAnswer(newAnswer);

                            _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                                new NexportSupplementalInfoAnswerProcessingQueueItem
                                {
                                    AnswerId = newAnswer.Id,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }

                    var removingQuestionIds = new List<int>();
                    var removingAnswers = answers.Where(a => !newOptionIds.Contains(a.OptionId));
                    foreach (var removingAnswer in removingAnswers)
                    {
                        var memberships =
                            _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(removingAnswer.Id);

                        var removingQuestionId = removingAnswer.QuestionId;
                        if (!removingQuestionIds.Contains(removingQuestionId))
                        {
                            removingQuestionIds.Add(removingAnswer.QuestionId);
                        }

                        _nexportService.DeleteNexportSupplementalInfoAnswer(removingAnswer);

                        foreach (var membership in memberships)
                        {
                            _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = customer.Id,
                                    NexportMembershipId = membership.NexportMembershipId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }

                    var questionWithoutAnswerIds =
                        _nexportService.GetUnansweredQuestions(customerId, storeId, removingQuestionIds);
                    foreach (var questionId in questionWithoutAnswerIds)
                    {
                        _nexportService.InsertNexportRequiredSupplementalInfo(
                            new NexportRequiredSupplementalInfo
                            {
                                CustomerId = customerId,
                                StoreId = storeId,
                                QuestionId = questionId,
                                UtcDateCreated = DateTime.UtcNow
                            });
                    }
                }
            }

            ViewBag.RefreshPage = true;

            ViewBag.ClosePage = true;

            var model = _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModel(customer, store, question);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/EditCustomerSupplementalInfoAnsweredQuestion.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult DeleteCustomerSupplementalInfoAnswer(int answerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var answer = _nexportService.GetNexportSupplementalInfoAnswerById(answerId)
                         ?? throw new Exception($"No Nexport supplemental info answer found with the specified id {answerId}");

            var customerId = answer.CustomerId;
            var storeId = answer.StoreId;
            var removingQuestionIds = new List<int> { answer.QuestionId };

            var memberships = _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(answer.Id);

            _nexportService.DeleteNexportSupplementalInfoAnswer(answer);

            foreach (var membership in memberships)
            {
                _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                    new NexportGroupMembershipRemovalQueueItem
                    {
                        CustomerId = customerId,
                        NexportMembershipId = membership.NexportMembershipId,
                        UtcDateCreated = DateTime.UtcNow
                    });
            }

            var questionWithoutAnswerIds =
                _nexportService.GetUnansweredQuestions(customerId, storeId, removingQuestionIds);
            foreach (var questionId in questionWithoutAnswerIds)
            {
                _nexportService.InsertNexportRequiredSupplementalInfo(
                    new NexportRequiredSupplementalInfo
                    {
                        CustomerId = customerId,
                        StoreId = storeId,
                        QuestionId = questionId,
                        UtcDateCreated = DateTime.UtcNow
                    });
            }

            return new NullJsonResult();
        }

        #endregion

        #region Registration Field

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult ListRegistrationFieldCategory()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Select an appropriate panel
            SaveSelectedPanelName("customersettings-nexportregistrationfields");

            // Redirect the user to the customer settings page
            return RedirectToAction("CustomerUser", "Setting");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult ListRegistrationFieldCategory(NexportRegistrationFieldCategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult CreateRegistrationFieldCategory()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryModel(
                new NexportRegistrationFieldCategoryModel(), null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateRegistrationFieldCategory(NexportRegistrationFieldCategoryModel model, bool continueEditing = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var registrationFieldCategory = model.ToEntity<NexportRegistrationFieldCategory>();
                _nexportService.InsertNexportRegistrationFieldCategory(registrationFieldCategory);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Categories.Added"));

                if (!continueEditing)
                    return RedirectToAction("ListRegistrationFieldCategory");

                return RedirectToAction("EditRegistrationFieldCategory", new { id = registrationFieldCategory.Id });
            }

            model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryModel(
                new NexportRegistrationFieldCategoryModel(), null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult EditRegistrationFieldCategory(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldCategory = _nexportService.GetNexportRegistrationFieldCategoryById(id);
            if (registrationFieldCategory == null)
                return RedirectToAction("ListRegistrationFieldCategory");

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryModel(null, registrationFieldCategory);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult EditRegistrationFieldCategory(NexportRegistrationFieldCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldCategory = _nexportService.GetNexportRegistrationFieldCategoryById(model.Id);
            if (registrationFieldCategory == null)
                return RedirectToAction("ListRegistrationFieldCategory");

            if (!ModelState.IsValid)
                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Edit.cshtml", model);

            registrationFieldCategory = model.ToEntity(registrationFieldCategory);
            _nexportService.UpdateNexportRegistrationFieldCategory(registrationFieldCategory);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Categories.Updated"));

            if (!continueEditing)
                return RedirectToAction("ListRegistrationFieldCategory");

            return RedirectToAction("EditRegistrationFieldCategory", new { id = registrationFieldCategory.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        public virtual IActionResult DeleteRegistrationFieldCategory(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldCategory = _nexportService.GetNexportRegistrationFieldCategoryById(id);
            _nexportService.DeleteNexportRegistrationFieldCategory(registrationFieldCategory);

            var registrationFieldsWithCategory = _nexportService.GetNexportRegistrationFieldsByCategoryId(id);
            foreach (var field in registrationFieldsWithCategory)
            {
                field.FieldCategoryId = null;
                _nexportService.UpdateNexportRegistrationField(field);
            }

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Categories.Deleted"));

            return RedirectToAction("ListRegistrationFieldCategory");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult ListRegistrationField()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Select an appropriate panel
            SaveSelectedPanelName("customersettings-nexportregistrationfields");

            // Redirect the user to the customer settings page
            return RedirectToAction("CustomerUser", "Setting");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult ListRegistrationField(NexportRegistrationFieldSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult CreateRegistrationField()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldModel(new NexportRegistrationFieldModel(), null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateRegistrationField(NexportRegistrationFieldModel model, bool continueEditing = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var registrationField = model.ToEntity<NexportRegistrationField>();
                _nexportService.InsertNexportRegistrationField(registrationField);

                // Create registration field store mapping for each selected store
                foreach (var storeId in model.StoreMappingIds)
                {
                    _nexportService.InsertNexportRegistrationFieldStoreMapping(
                        new NexportRegistrationFieldStoreMapping
                        {
                            FieldId = registrationField.Id,
                            StoreId = storeId
                        });
                }

                UpdateNexportRegistrationFieldLocales(registrationField, model);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Fields.Added"));

                return !continueEditing
                    ? RedirectToAction("ListRegistrationField")
                    : RedirectToAction("EditRegistrationField", new { id = registrationField.Id });
            }

            model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldModel(
                new NexportRegistrationFieldModel(), null, true);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult EditRegistrationField(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(id);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldModel(null, registrationField);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditRegistrationField(NexportRegistrationFieldModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(model.Id);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            if (!ModelState.IsValid)
                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Edit.cshtml", model);

            if (model.Type == NexportRegistrationFieldType.CustomType)
            {
                if (_nexportService.HasCustomRegistrationFieldRenderForStores(registrationField.Id, model.StoreMappingIds, model.CustomFieldRender))
                {
                    _notificationService.ErrorNotification(
                        _localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit"));

                    return RedirectToAction("EditRegistrationField", new { id = registrationField.Id });
                }
            }

            registrationField = model.ToEntity(registrationField);

            _nexportService.UpdateNexportRegistrationField(registrationField);
            if (model.Type == NexportRegistrationFieldType.SelectCheckbox ||
                model.Type == NexportRegistrationFieldType.SelectDropDown)
            {
                if (model.Type == NexportRegistrationFieldType.SelectCheckbox)
                {
                    _genericAttributeService.SaveAttribute(registrationField,
                        nameof(model.AllowMultipleSelection), model.AllowMultipleSelection);
                }

                _genericAttributeService.SaveAttribute(registrationField,
                    nameof(model.DisplayOptionByAscendingOrder), model.DisplayOptionByAscendingOrder);
            }

            var storeMappings = _nexportService.GetNexportRegistrationFieldStoreMappings(registrationField.Id);

            var currentStoreIds = storeMappings
                .Select(x => x.StoreId).ToList();
            var removalStoreIds = currentStoreIds.Except(model.StoreMappingIds);
            var additionalStoreIds = model.StoreMappingIds.Except(currentStoreIds);

            foreach (var storeId in additionalStoreIds)
            {
                _nexportService.InsertNexportRegistrationFieldStoreMapping(
                    new NexportRegistrationFieldStoreMapping
                    {
                        FieldId = registrationField.Id,
                        StoreId = storeId
                    });
            }

            foreach (var storeId in removalStoreIds)
            {
                var deletingMapping = storeMappings.FirstOrDefault(x => x.StoreId == storeId);
                if (deletingMapping != null)
                    _nexportService.DeleteNexportRegistrationFieldStoreMapping(deletingMapping);
            }

            UpdateNexportRegistrationFieldLocales(registrationField, model);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Fields.Updated"));

            if (!continueEditing)
                return RedirectToAction("ListRegistrationField");

            return RedirectToAction("EditRegistrationField", new { id = registrationField.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        public IActionResult DeleteRegistrationField(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(id);
            var registrationFieldOptionSettings = _genericAttributeService.GetAttributesForEntity(registrationField.Id,
                registrationField.GetUnproxiedEntityType().Name);

            _nexportService.DeleteNexportRegistrationField(registrationField);

            // Delete the option of the registration field
            var registrationFieldOptions = _nexportService.GetNexportRegistrationFieldOptions(id);
            foreach (var fieldOption in registrationFieldOptions)
            {
                _nexportService.DeleteNexportRegistrationFieldOption(fieldOption);
            }

            // Delete the store mappings of the registration field
            var registrationFieldStoreMappings = _nexportService.GetNexportRegistrationFieldStoreMappings(id);
            foreach (var fieldStoreMapping in registrationFieldStoreMappings)
            {
                _nexportService.DeleteNexportRegistrationFieldStoreMapping(fieldStoreMapping);
            }

            // Delete related extra settings of the registration field
            _genericAttributeService.DeleteAttributes(registrationFieldOptionSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Fields.Deleted"));

            return RedirectToAction("ListRegistrationField");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult ListRegistrationFieldOptions(NexportRegistrationFieldOptionSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedDataTablesJson();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(searchModel.RegistrationFieldId)
                                    ?? throw new ArgumentException("No Nexport registration field found with the specified id");

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionListModel(searchModel, registrationField);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult CreateRegistrationFieldOption(int fieldId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(fieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModel(
                new NexportRegistrationFieldOptionModel(), registrationField, null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateRegistrationFieldOption(NexportRegistrationFieldOptionModel model, bool continueEditing = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(model.FieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            if (ModelState.IsValid)
            {
                var registrationFieldOption = model.ToEntity<NexportRegistrationFieldOption>();
                _nexportService.InsertNexportRegistrationFieldOption(registrationFieldOption);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Options.Added"));

                ViewBag.RefreshPage = true;

                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Create.cshtml", model);
            }

            model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModel(
                new NexportRegistrationFieldOptionModel(), registrationField, null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult EditRegistrationFieldOption(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldOption = _nexportService.GetNexportRegistrationFieldOptionById(id);
            if (registrationFieldOption == null)
                return RedirectToAction("ListRegistrationField");

            var registrationField = _nexportService.GetNexportRegistrationFieldById(registrationFieldOption.FieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            var model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModel(null, registrationField, registrationFieldOption);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditRegistrationFieldOption(NexportRegistrationFieldOptionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldOption = _nexportService.GetNexportRegistrationFieldOptionById(model.Id);
            if (registrationFieldOption == null)
                return RedirectToAction("ListRegistrationField");

            var registrationField = _nexportService.GetNexportRegistrationFieldById(registrationFieldOption.FieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            if (ModelState.IsValid)
            {
                registrationFieldOption = model.ToEntity(registrationFieldOption);
                _nexportService.UpdateNexportRegistrationFieldOption(registrationFieldOption);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Options.Updated"));

                ViewBag.RefreshPage = true;

                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Edit.cshtml", model);
            }

            model = _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModel(null, registrationField, registrationFieldOption);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        public IActionResult DeleteRegistrationFieldOption(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldOption = _nexportService.GetNexportRegistrationFieldOptionById(id)
                ?? throw new ArgumentException("No Nexport registration field option found with the specified id", nameof(id));

            _nexportService.DeleteNexportRegistrationFieldOption(registrationFieldOption);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Options.Deleted"));

            return new NullJsonResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public IActionResult GetRegistrationFieldCustomRenderOptionUrl(string systemName, int fieldId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var registrationFieldCustomRender = _registrationFieldCustomRenderPluginManager.LoadPluginBySystemName(systemName)
                                                ?? throw new ArgumentException("Registration field custom render could not be loaded");

            var url = registrationFieldCustomRender.GetRenderOptionUrl(fieldId);

            return Json(new { url });
        }

        public IActionResult GetRegistrationFieldCustomRenderUrl(string systemName, int fieldId)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var registrationFieldCustomRender = _registrationFieldCustomRenderPluginManager.LoadPluginBySystemName(systemName)
                                                ?? throw new ArgumentException("Registration field custom render could not be loaded");

            var url = registrationFieldCustomRender.GetCustomRenderUrl(fieldId);

            return Json(new { url });
        }

        #endregion

        #region Event Handling

        public void HandleEvent(CustomerRegisteredEvent eventMessage)
        {
            try
            {
                _nexportService.CreateAndMapNewNexportUser(eventMessage.Customer);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot create and map new Nexport user", ex, eventMessage.Customer);
            }
        }

        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;

            foreach (var item in order.OrderItems)
            {
                var mapping = _nexportService.GetProductMappingByNopProductId(item.ProductId, order.StoreId) ??
                    _nexportService.GetProductMappingByNopProductId(item.ProductId);

                if (mapping != null)
                {
                    var store = _storeService.GetStoreById(order.StoreId);
                    var storeModel = _genericAttributeService.GetAttribute<NexportStoreSaleModel>(store, "NexportStoreSaleModel", store.Id);
                    _genericAttributeService.SaveAttribute(item, $"StoreModel-{order.Id}-{item.Id}", JsonConvert.SerializeObject(storeModel), store.Id);

                    _genericAttributeService.SaveAttribute(item,
                        $"ProductMapping-{order.Id}-{item.Id}",
                        JsonConvert.SerializeObject(mapping), order.StoreId);

                    var groupMembershipMappings = _nexportService.GetProductGroupMembershipMappings(mapping.Id);
                    foreach (var groupMembershipMapping in groupMembershipMappings)
                    {
                        _genericAttributeService.SaveAttribute(item,
                            $"ProductGroupMembershipMapping-{order.Id}-{item.Id}-{mapping.Id}",
                            JsonConvert.SerializeObject(groupMembershipMapping), order.StoreId);
                    }
                }
            }
        }

        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            var product = eventMessage.Entity;

            var mappings = _nexportService.GetProductMappings(product.Id);
            foreach (var mapping in mappings)
            {
                _nexportService.DeleteNexportProductMapping(mapping);

                var groupMembershipMappings = _nexportService.GetProductGroupMembershipMappings(mapping.Id);
                foreach (var groupMembershipMapping in groupMembershipMappings)
                {
                    _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);
                }
            }
        }

        public void HandleEvent(EntityUpdatedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;
            if (order.OrderStatus == OrderStatus.Processing && order.PaymentStatus == PaymentStatus.Paid)
            {
                ProcessNewRedemption(order);
            }
        }

        public void ProcessNewRedemption(Order order)
        {
            _nexportService.InsertNexportOrderProcessingQueueItem(new NexportOrderProcessingQueueItem
            {
                OrderId = order.Id,
                UtcDateCreated = DateTime.UtcNow
            });
        }

        public void HandleEvent(EntityDeletedEvent<Customer> eventMessage)
        {
            var deletedCustomer = eventMessage.Entity;
            var userMapping = _nexportService.FindUserMappingByCustomerId(deletedCustomer.Id);
            if (userMapping == null)
                return;

            _nexportService.DeleteUserMapping(userMapping);
        }

        public void HandleEvent(EntityInsertedEvent<Store> eventMessage)
        {
            var store = eventMessage.Entity;

            _genericAttributeService.SaveAttribute(store, NexportDefaults.NEXPORT_STORE_SALE_MODEL_SETTING_KEY,
                NexportStoreSaleModel.Retail, store.Id);
            _genericAttributeService.SaveAttribute(store, NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY,
                true, store.Id);
            _genericAttributeService.SaveAttribute(store, NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY,
                false, store.Id);
            _genericAttributeService.SaveAttribute(store, NexportDefaults.HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY,
                false, store.Id);
        }

        public void HandleEvent(EntityDeletedEvent<Store> eventMessage)
        {
            var deletedStore = eventMessage.Entity;

            // Find and remove all generic attributes that associated with this store
            var storeAttributes = _genericAttributeService.GetAttributesForEntity(deletedStore.Id, "Store");
            _genericAttributeService.DeleteAttributes(storeAttributes);

            // Find and remove product mappings that are associated with this store
            var mappings = _nexportService.GetProductMappingsByStoreId(deletedStore.Id);
            foreach (var mapping in mappings)
            {
                _nexportService.DeleteNexportProductMapping(mapping);
            }
        }

        #endregion

        #region Customer Actions

        [HttpsRequirement(SslRequirement.Yes)]
        public IActionResult ViewNexportOrderRedemption(NexportOrderInvoiceItem model)
        {
            return View("~/Plugins/Misc.Nexport/Views/ViewOrder.cshtml", model);
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public IActionResult ViewNexportTraining()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            try
            {
                var model = _nexportPluginModelFactory.PrepareNexportTrainingListModel(_workContext.CurrentCustomer);

                var myTrainingViewLocationSetting =
                    _settingService.GetSetting("nexport.mytraining.view", _storeContext.CurrentStore.Id, true);

                return View(myTrainingViewLocationSetting != null
                    ? myTrainingViewLocationSetting.Value
                    : "~/Plugins/Misc.Nexport/Views/NexportTrainings.cshtml",
                    model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                _notificationService.ErrorNotification(ex.Message);
            }

            return new EmptyResult();
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public IActionResult ViewSupplementalInfoAnswers()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            try
            {
                var model = _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersModel(
                    _workContext.CurrentCustomer, _storeContext.CurrentStore);

                return View("~/Plugins/Misc.Nexport/Views/NexportCustomer/NexportSupplementalInfoAnswers.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                _notificationService.ErrorNotification(ex.Message);
            }

            return new EmptyResult();
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public IActionResult EditSupplementalInfoAnswers(int questionId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(questionId);

            if (question == null)
                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

            var model = _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModel(
                _workContext.CurrentCustomer, _storeContext.CurrentStore, question);

            return View("~/Plugins/Misc.Nexport/Views/NexportCustomer/EditNexportSupplementalInfoAnswer.cshtml", model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public IActionResult EditSupplementalInfoAnswers(EditSupplementInfoAnswerRequestModel editModel)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (editModel.OptionIds == null || editModel.OptionIds.Count == 0)
                throw new Exception("List of submission options cannot be null or empty.");

            var question = _nexportService.GetNexportSupplementalInfoQuestionById(editModel.QuestionId);

            if (question == null)
                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

            var answers = _nexportService.GetNexportSupplementalInfoAnswers(
                _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, editModel.QuestionId);

            if (answers == null || answers.Count == 0)
                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

            if (ModelState.IsValid)
            {
                if (question.Type == NexportSupplementalInfoQuestionType.SingleOption)
                {
                    if (editModel.OptionIds.Count > 1)
                        return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

                    var updatingAnswer = answers.First();
                    var newOption = editModel.OptionIds[0];
                    if (updatingAnswer.OptionId != newOption)
                    {
                        var memberships =
                            _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(updatingAnswer.Id);

                        updatingAnswer.OptionId = newOption;
                        updatingAnswer.Status = NexportSupplementalInfoAnswerStatus.Modified;
                        updatingAnswer.UtcDateModified = DateTime.UtcNow;

                        _nexportService.UpdateNexportSupplementalInfoAnswer(updatingAnswer);

                        _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                            new NexportSupplementalInfoAnswerProcessingQueueItem
                            {
                                AnswerId = updatingAnswer.Id,
                                UtcDateCreated = DateTime.UtcNow
                            });

                        foreach (var membership in memberships)
                        {
                            _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    NexportMembershipId = membership.NexportMembershipId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }
                }
                else if (question.Type == NexportSupplementalInfoQuestionType.MultipleOptions)
                {
                    if (editModel.OptionIds.Count < 1)
                        return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

                    foreach (var newOption in editModel.OptionIds)
                    {
                        var newAnswer = answers.FirstOrDefault(a => a.OptionId == newOption);
                        if (newAnswer == null)
                        {
                            newAnswer = new NexportSupplementalInfoAnswer
                            {
                                CustomerId = _workContext.CurrentCustomer.Id,
                                StoreId = _storeContext.CurrentStore.Id,
                                OptionId = newOption,
                                QuestionId = editModel.QuestionId,
                                Status = NexportSupplementalInfoAnswerStatus.NotProcessed,
                                UtcDateCreated = DateTime.UtcNow
                            };

                            _nexportService.InsertNexportSupplementalInfoAnswer(newAnswer);

                            _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                                new NexportSupplementalInfoAnswerProcessingQueueItem
                                {
                                    AnswerId = newAnswer.Id,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }

                    var removingAnswers = answers.Where(a => !editModel.OptionIds.Contains(a.OptionId));
                    foreach (var removingAnswer in removingAnswers)
                    {
                        var memberships =
                            _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(removingAnswer.Id);

                        _nexportService.DeleteNexportSupplementalInfoAnswer(removingAnswer);

                        foreach (var membership in memberships)
                        {
                            _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    NexportMembershipId = membership.NexportMembershipId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }
                }

                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");
            }

            var model = _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModel(
                _workContext.CurrentCustomer, _storeContext.CurrentStore, question);

            return View("~/Plugins/Misc.Nexport/Views/NexportCustomer/EditNexportSupplementalInfoAnswer.cshtml", model);
        }

        #endregion

        #region Redeeming Product Actions

        [HttpPost]
        [PublicAntiForgery]
        public IActionResult RedeemNexportOrderInvoiceItem(int orderItemInvoiceId, Guid? redeemingUserId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            try
            {
                if (redeemingUserId == null)
                {
                    var userMapping = _nexportService.FindUserMappingByCustomerId(customer.Id);
                    redeemingUserId = userMapping.NexportUserId;
                }

                var nexportOrderInvoiceItem =
                    _nexportService.FindNexportOrderInvoiceItemById(orderItemInvoiceId);

                var order = _orderService.GetOrderById(nexportOrderInvoiceItem.OrderId);

                try
                {
                    _nexportService.RedeemNexportInvoiceItem(nexportOrderInvoiceItem, redeemingUserId.Value);

                    _nexportService.AddOrderNote(order,
                        $"Nexport invoice item {nexportOrderInvoiceItem.InvoiceItemId} has been redeemed for user {redeemingUserId}", updateOrder: true);
                }
                catch (Exception e)
                {
                    _nexportService.AddOrderNote(order,
                        $"Nexport invoice item {nexportOrderInvoiceItem.InvoiceItemId} cannot be redeemed for user {redeemingUserId}", updateOrder: true);

                    var errorMsg = string.Format(_localizationService.GetResource("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser"),
                        nexportOrderInvoiceItem.InvoiceItemId, redeemingUserId);

                    _logger.Error(errorMsg, e, customer);
                    _notificationService.ErrorNotification(errorMsg);

                    return new EmptyResult();
                }

                return Json(nexportOrderInvoiceItem);
            }
            catch (Exception ex)
            {
                var errorMsg = string.Format(_localizationService.GetResource("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure"), orderItemInvoiceId);

                _logger.Error(errorMsg, ex);
                _notificationService.ErrorNotification(errorMsg);
            }

            return new EmptyResult();
        }

        public IActionResult GoToNexport(int orderInvoiceItemId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (orderInvoiceItemId < 1)
                return Content("");

            var orderInvoiceItem = _nexportService.FindNexportOrderInvoiceItemById(orderInvoiceItemId);

            if (orderInvoiceItem == null)
                throw new Exception("Order invoice does not existed");

            if (orderInvoiceItem.UtcDateRedemption == null)
                throw new Exception("Order invoice has not been redeemed. Unable to access Nexport.");

            dynamic result = new ExpandoObject();

            try
            {
                result.RedirectUrl = _nexportService.SignInNexport(orderInvoiceItem);
            }
            catch (Exception ex)
            {
                var errorMsg = _localizationService.GetResource("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
                _logger.Error(errorMsg, ex, _workContext.CurrentCustomer);

                result.Error = errorMsg;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        public IActionResult GoToNexportOrg(Guid orgId, Guid userId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (orgId == Guid.Empty)
                throw new Exception("Organization Id cannot be empty.");

            dynamic result = new ExpandoObject();

            try
            {
                result.RedirectUrl = _nexportService.SignInNexport(orgId, userId);
            }
            catch (Exception ex)
            {
                var errorMsg = _localizationService.GetResource("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
                _logger.Error(errorMsg, ex, _workContext.CurrentCustomer);

                result.Error = errorMsg;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        #endregion
    }
}
