using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Data;
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
using Nop.Services.Plugins;
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
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Infrastructure.CustomExceptions;
using Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using System.Threading.Tasks;

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
        IConsumer<EntityDeletedEvent<Store>>,
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>
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

        protected async Task UpdateNexportRegistrationFieldLocalesAsync(NexportRegistrationField registrationField, NexportRegistrationFieldModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(registrationField,
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
        public async Task<IActionResult> SearchNexportDirectory(string searchTerm, int? page = null)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await Configure();

            JsonResult jsonResult = null;

            try
            {
                var result = await _nexportService.SearchNexportDirectoryAsync(searchTerm, page);
                jsonResult = new JsonResult(result);
            }
            catch (Exception ex)
            {
                var errorMsg = "Cannot search the Nexport directory.";

                if (ex is ApiException exception)
                {
                    errorMsg += $" ({exception.Message})";
                }

                await _logger.ErrorAsync(errorMsg, ex);
            }

            return jsonResult;
        }

        #endregion

        #region Plugin Configuration Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [ImportModelState]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
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
        [AutoValidateAntiforgeryToken]
        [ExportModelState]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                try
                {
                    await _nexportService.GenerateNewNexportTokenAsync(model);
                }
                catch (Exception ex)
                {
                    var errorMsg = "Cannot generate new Nexport authentication token!";

                    if (ex is ApiException exception)
                    {
                        errorMsg += $" ({exception.Message})";
                    }

                    await _logger.ErrorAsync(errorMsg, ex);
                    _notificationService.ErrorNotification(errorMsg);
                }
            }

            return RedirectToAction("Configure");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setserverurl")]
        [ExportModelState]
        public async Task<IActionResult> SetServerUrl(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                if (model.Url.IsValidUrl())
                {
                    _nexportSettings.Url = model.Url;
                    await _settingService.SaveSettingAsync(_nexportSettings);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
                }
            }
            catch (Exception ex)
            {
                var errMsg = "Cannot set the server url!";
                await _logger.ErrorAsync(errMsg, ex);
                _notificationService.ErrorNotification(errMsg);
            }

            return RedirectToAction("Configure");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setrootorganizationid")]
        [ExportModelState]
        public async Task<IActionResult> SetRootOrganization(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                _nexportSettings.RootOrganizationId = model.RootOrganizationId;
                await _settingService.SaveSettingAsync(_nexportSettings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                var errMsg = "Cannot set the root organization!";
                await _logger.ErrorAsync(errMsg, ex);
                _notificationService.ErrorNotification(errMsg);
            }

            return RedirectToAction("Configure");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setmerchantaccountid")]
        [ExportModelState]
        public async Task<IActionResult> SetMerchantAccount(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            try
            {
                _nexportSettings.MerchantAccountId = model.MerchantAccountId;
                await _settingService.SaveSettingAsync(_nexportSettings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                var errMsg = "Cannot set the merchant account!";
                await _logger.ErrorAsync(errMsg, ex);
                _notificationService.ErrorNotification(errMsg);
            }

            return RedirectToAction("Configure");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [Route("Admin/Store/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("setsubscriptionorgid")]
        public async Task<IActionResult> SetNexportSubscriptionOrganizationId(StoreModel model, [FromForm(Name = "NexportSubscriptionOrgId")] Guid subOrgId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = await _storeService.GetStoreByIdAsync(model.Id);
            if (store == null)
                return RedirectToAction("List", "Store");

            await _genericAttributeService.SaveAttributeAsync(store, "NexportSubscriptionOrganizationId", subOrgId, store.Id);

            _notificationService.SuccessNotification("Success update Nexport subscription organization");

            return RedirectToAction("Edit", "Store", new { id = store.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [Route("Admin/Store/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("savenexportstoreconfig")]
        public async Task<IActionResult> SaveNexportStoreConfiguration(NexportStoreModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = await _storeService.GetStoreByIdAsync(model.Id);
            if (store == null)
                return RedirectToAction("List", "Store");

            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.NEXPORT_STORE_SALE_MODEL_SETTING_KEY,
                model.SaleModel, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY,
                model.AllowRepurchaseFailedCourses, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY,
                model.AllowRepurchasePassedCourses, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY,
                model.HideSectionCEUsInProductPage, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY,
                model.HideAddToCartForIneligibleProducts, store.Id);

            _notificationService.SuccessNotification("Success update Nexport store configuration");

            return RedirectToAction("Edit", "Store", new { id = store.Id });
        }

        #endregion

        #region User Configuration Actions

        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [Route("Admin/Customer/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("setnexportuserid")]
        public async Task<IActionResult> MapNexportUser(CustomerModel model, [FromForm(Name = "NexportUserId")] Guid nexportUserId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(model.Id);
            if (customer == null)
                return RedirectToAction("List", "Customer");

            if (nexportUserId != Guid.Empty)
            {
                var currentUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                if (currentUserMapping == null)
                {
                    await _nexportService.InsertUserMapping(new NexportUserMapping
                    {
                        NexportUserId = nexportUserId,
                        NopUserId = customer.Id
                    });
                }
                else
                {
                    currentUserMapping.NexportUserId = nexportUserId;

                    await _nexportService.UpdateUserMapping(currentUserMapping);
                }

                try
                {
                    await _nexportService.SynchronizeContactInfoFromNexportAsync(customer, nexportUserId);
                }
                catch (Exception ex)
                {
                    var errMsg = $"Cannot synchronize the contact info for customer {customer.Id} with Nexport user {nexportUserId}";
                    await _logger.ErrorAsync(errMsg, ex);
                }

                _notificationService.SuccessNotification("Success update Nexport user mapping");
            }

            return RedirectToAction("Edit", "Customer", new { id = customer.Id });
        }

        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportUserDetails(Guid nexportUserId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.AccessDenied.Description"));

            GetUserResponse nexportUser = null;

            if (nexportUserId != Guid.Empty)
            {
                try
                {
                    nexportUser = await _nexportService.GetNexportUserAsync(nexportUserId);
                }
                catch (Exception ex)
                {
                    var errMsg = $"Cannot get detail information of Nexport user {nexportUserId}";
                    await _logger.ErrorAsync(errMsg, ex);

                    _notificationService.ErrorNotification(errMsg);
                }
            }

            if (nexportUser != null)
            {
                return Json(new
                {
                    id = nexportUser.UserId,
                    firstName = nexportUser.FirstName,
                    lastName = nexportUser.LastName,
                    email = nexportUser.Email,
                    internalEmail = nexportUser.InternalEmail
                });
            }

            return Json(null);
        }

        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SetNexportUser(int customerId, Guid nexportUserId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.AccessDenied.Description"));

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("List", "Customer")
                });
            }

            if (nexportUserId != Guid.Empty)
            {
                GetUserResponse nexportUser = null;
                try
                {
                    nexportUser = await _nexportService.GetNexportUserAsync(nexportUserId);
                }
                catch (Exception ex)
                {
                    var errMsg = $"Cannot map customer {customerId} with Nexport user {nexportUserId}";
                    await _logger.ErrorAsync(errMsg, ex);

                    _notificationService.ErrorNotification(errMsg);
                }

                if (nexportUser != null)
                {
                    try
                    {
                        var currentUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                        if (currentUserMapping == null)
                        {
                            await _nexportService.InsertUserMapping(new NexportUserMapping
                            {
                                NexportUserId = nexportUserId,
                                NopUserId = customer.Id
                            });
                        }
                        else
                        {
                            currentUserMapping.NexportUserId = nexportUserId;

                            await _nexportService.UpdateUserMapping(currentUserMapping);
                        }

                        _notificationService.SuccessNotification("Success update Nexport user mapping");
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot map customer {customer.Id} with Nexport user Id {nexportUserId}", ex);

                        var errMsg = $"Cannot map current customer with Nexport user {nexportUserId}";
                        if (ex is NexportUserMappingException exception)
                        {
                            var customerEditUrl = Url.Action("Edit", "Customer", new { id = exception.ExistingUserMapping.NopUserId });
                            errMsg += $". {exception.Message}. Click <a href=\"{customerEditUrl}\">here</a> to view the existing customer.";
                        }

                        _notificationService.ErrorNotification(errMsg, false);
                    }

                    try
                    {
                        await _nexportService.SynchronizeContactInfoFromNexportAsync(customer, nexportUserId);
                    }
                    catch (Exception ex)
                    {
                        var errMsg = $"Cannot synchronize the contact info for customer {customer.Id} with Nexport user {nexportUserId}";
                        await _logger.ErrorAsync(errMsg, ex);
                    }
                }
            }

            return Json(new
            {
                redirectUrl = Url.Action("Edit", "Customer", new { id = customer.Id })
            });
        }

        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CanSetNexportUser(int customerId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.AccessDenied.Description"));

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return Json(null);
            }

            var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(customerId);
            if (nexportUserMapping != null)
            {
                var customerOrders = await _orderService.SearchOrdersAsync(customerId: customer.Id, pageSize: 10);
                return Json(!customerOrders.Any());
            }

            return Json(true);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [Route("Admin/Customer/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("syncnexportregistrationfields")]
        public async Task<IActionResult> SyncCustomerRegistrationFieldsWithNexport(CustomerModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(model.Id);
            if (customer == null)
                return RedirectToAction("List", "Customer");

            await _nexportService.InsertNexportRegistrationFieldSynchronizationQueueItem(new NexportRegistrationFieldSynchronizationQueueItem
            {
                CustomerId = model.Id,
                UtcDateCreated = DateTime.UtcNow
            });

            _notificationService.SuccessNotification("The customer registration fields has been scheduled to be synchronize with Nexport.");

            return RedirectToAction("Edit", "Customer", new { id = model.Id });
        }

        #endregion

        #region Product Mapping Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        //public async Task<IActionResult> GetCatalogList(Guid? orgId, int nopProductId)
        public async Task<IActionResult> GetCatalogList(NexportCatalogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            //var searchModel = new NexportCatalogSearchModel { OrgId = orgId, NopProductId = nopProductId };
            //searchModel.SetGridPageSize();

            return View("~/Plugins/Misc.Nexport/Views/MapNexportProductList.cshtml", searchModel);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CatalogList(NexportCatalogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportCatalogListModelAsync(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public async Task<IActionResult> SyllabusList(NexportSyllabusListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportSyllabusListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetCatalogs(NexportCatalogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportCatalogListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetSyllabuses(NexportSyllabusListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportSyllabusListModelAsync(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProductMappingDetailsPopup(int mappingId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedView();

            var productMapping = await _nexportService.GetProductMappingById(mappingId);

            var model = await _nexportPluginModelFactory.PrepareNexportProductMappingModelAsync(productMapping, true);

            return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetProductMappings(NexportProductMappingSearchModel searchModel, Guid? nexportProductId, NexportProductTypeEnum? nexportProductType, int? nopProductId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = new NexportProductMappingListModel();

            if (nexportProductId.HasValue)
            {
                model = await _nexportPluginModelFactory.PrepareNexportProductMappingListModelAsync(searchModel, nexportProductId.Value, nexportProductType.Value);
            }
            else if (nopProductId.HasValue)
            {
                model = await _nexportPluginModelFactory.PrepareNexportProductMappingListModelAsync(searchModel, nopProductId.Value);
            }

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditMapping(NexportProductMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedView();

            var productMapping = await _nexportService.GetProductMappingById(model.Id)
                                 ?? throw new ArgumentException("No product mapping found with the specified id");

            if (ModelState.IsValid)
            {
                try
                {
                    // Fill entity from product
                    productMapping = model.ToEntity(productMapping);

                    if (productMapping.NexportSubscriptionOrgId.HasValue)
                    {
                        if (string.IsNullOrWhiteSpace(model.NexportSubscriptionOrgName))
                        {
                            var organizationDetails =
                                await _nexportService.GetOrganizationDetailsAsync(productMapping.NexportSubscriptionOrgId.Value);
                            if (organizationDetails != null)
                            {
                                productMapping.NexportSubscriptionOrgName = organizationDetails.Name;
                                productMapping.NexportSubscriptionOrgShortName = organizationDetails.ShortName;
                            }
                        }
                    }
                    else
                    {
                        productMapping.NexportSubscriptionOrgName = null;
                        productMapping.NexportSubscriptionOrgShortName = null;
                    }

                    productMapping.UtcLastModifiedDate = DateTime.UtcNow;

                    await _nexportService.UpdateNexportProductMapping(productMapping);

                    var questionMappings =
                        await _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id);

                    var currentQuestionIds = questionMappings.Select(x => x.QuestionId).ToList();
                    var removalQuestionIds = currentQuestionIds.Except(model.SupplementalInfoQuestionIds);
                    var additionalQuestionIds = model.SupplementalInfoQuestionIds.Except(currentQuestionIds);

                    foreach (var questionId in additionalQuestionIds)
                    {
                        await _nexportService.InsertNexportSupplementalInfoQuestionMapping(
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
                            await _nexportService.DeleteNexportSupplementalInfoQuestionMapping(deletingMapping);
                        }
                    }

                    if (!continueEditing)
                    {
                        ViewBag.ClosePage = true;
                    }

                    ViewBag.RefreshPage = true;
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Cannot save the edited product mapping {model.Id} for product {model.NopProductId}.";

                    if (ex is ApiException exception)
                    {
                        errorMsg += $" ({exception.Message})";
                    }

                    await _logger.ErrorAsync(errorMsg, ex);
                    _notificationService.ErrorNotification(errorMsg);
                }
            }

            model = await _nexportPluginModelFactory.PrepareNexportProductMappingModelAsync(productMapping, true);

            return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteMapping(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
                return AccessDeniedView();

            var mapping = await _nexportService.GetProductMappingById(id)
                          ?? throw new Exception($"No nexport mapping found with the specified id {id}");

            await _nexportService.DeleteNexportProductMapping(mapping);

            var groupMembershipMappings = await _nexportService.GetProductGroupMembershipMappings(mapping.Id);
            foreach (var groupMembershipMapping in groupMembershipMappings)
            {
                await _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);
            }

            return new NullJsonResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> HasDefaultMapping(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return Json(await _nexportService.HasDefaultMapping(productId));
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteMappings(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var mapping = await _nexportService.GetProductMappingById(id);
                    if (mapping != null)
                    {
                        await _nexportService.DeleteNexportProductMapping(mapping);

                        var groupMembershipMappings =
                            await _nexportService.GetProductGroupMembershipMappings(mapping.Id);
                        foreach (var groupMembershipMapping in groupMembershipMappings)
                        {
                            await _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);
                        }
                    }
                }
            }

            return Json(new { Result = true });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetProductGroupMembershipMappings(NexportProductGroupMembershipMappingListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportProductMappingGroupMembershipListModelAsync(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddGroupMembershipMapping(int nexportProductMappingId, Guid nexportGroupId, string nexportGroupName, string nexportGroupShortName)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
                return AccessDeniedView();

            if (nexportGroupId == Guid.Empty)
                throw new ArgumentException("Group Id cannot be empty Guid", nameof(nexportGroupId));

            var productMapping = await _nexportService.GetProductMappingById(nexportProductMappingId);
            if (productMapping == null)
                throw new ArgumentException("No nexport mapping found with the specified id", nameof(nexportProductMappingId));

            await _nexportService.InsertNexportProductGroupMembershipMapping(new NexportProductGroupMembershipMapping
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

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteGroupMembershipMapping(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
                return AccessDeniedView();

            var groupMembershipMapping = await _nexportService.GetProductGroupMembershipMappingById(id);
            if (groupMembershipMapping == null)
                throw new Exception($"No nexport group membership mapping found with the specified id {id}");

            await _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> MapNexportProductPopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedView();

            return View("~/Plugins/Misc.Nexport/Views/MapNexportProduct.cshtml");
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [FormValueRequired("save")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> MapNexportProductPopup(MapNexportProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedView();

            dynamic result = new ExpandoObject();

            if (ModelState.IsValid)
            {
                try
                {
                    await _nexportService.MapNexportProduct(model);

                    ViewBag.RefreshPage = true;

                    ViewBag.ClosePage = false;

                    var newMapping = await _nexportService.GetProductMappingByNopProductId(model.NopProductId, model.StoreId);

                    result.MappingId = newMapping.Id;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(
                        $"Error occurred while mapping the product [{model.NopProductId}] with the Nexport product [{model.NexportProductId}]",
                        ex, await _workContext.GetCurrentCustomerAsync());

                    result.Error = $"Cannot map the product [{model.NopProductId}] with the Nexport product [{model.NexportProductId}].";

                    if (ex is ApiException exception)
                    {
                        if (exception.ErrorCode == (int)ApiErrorEntity.ErrorCodeEnum.UnknownError)
                        {
                            result.InnerError = $"Product [{model.NexportProductId}] is missing in Nexport.";
                        }
                    }

                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                result.Error = $"Cannot map the product [{model.NopProductId}] with the Nexport product [{model.NexportProductId}]";

                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        #endregion

        #region Product Management Actions

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [Route("Admin/Product/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("syncnexportproduct")]
        public async Task<IActionResult> SyncNexportProductWithNopProduct(ProductModel model, [FromForm(Name = "NexportMappingId")] int mappingId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping) ||
                string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedView();

            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List", "Product");

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                product = model.ToEntity(product);

                await _nexportService.SyncNexportProductAsync(mappingId, product);

                _notificationService.SuccessNotification("The product has been synchronized successfully with Nexport data");
            }

            return RedirectToAction("Edit", "Product", new { id = model.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> CopyProduct(ProductModel model, bool copyProductMapping = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = await _productService.GetProductByIdAsync(copyModel.Id);

                var currentVendor = await _workContext.GetCurrentVendorAsync();
                //a vendor should have access only to his products
                if (currentVendor != null && originalProduct.VendorId != currentVendor.Id)
                    return RedirectToAction("List", "Product");

                var newProduct =
                    await _copyProductService.CopyProductAsync(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyMultimedia);

                if (copyProductMapping)
                {
                    await _nexportService.CopyProductMappingsAsync(originalProduct, newProduct);
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));

                return RedirectToAction("Edit", "Product", new { id = newProduct.Id });
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                return RedirectToAction("Edit", "Product", new { id = copyModel.Id });
            }
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> DuplicateProductMapping(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = await _productService.GetProductByIdAsync(productId)
                        ?? throw new Exception($"No product found with the specified id {productId}");

            var model = await _nexportPluginModelFactory.PrepareDuplicateNexportProductMappingModel(product);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}Product/ProductMapping/DuplicateProductMapping.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> DuplicateProductMapping(int productId, DuplicateNexportProductMappingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = await _productService.GetProductByIdAsync(productId)
                          ?? throw new Exception($"No product found with the specified id {productId}");

            if (ModelState.IsValid)
            {
                try
                {
                    var productMapping = await _nexportService.GetProductMappingByNopProductId(product.Id, model.SourceStoreId);

                    if (productMapping != null)
                    {
                        foreach (var storeId in model.DestinationStoreIds)
                        {
                            await _nexportService.DuplicateProductMappingAsync(productMapping, storeId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errMsg =
                        $"Unable to duplicate Nexport product mapping for product {product.Id} based on store mapping";
                    await _logger.ErrorAsync($"Unable to duplicate Nexport product mapping for product {product.Id} based on store mapping", ex);
                }

                ViewBag.RefreshPage = true;

                ViewBag.ClosePage = true;
            }

            model = await _nexportPluginModelFactory.PrepareDuplicateNexportProductMappingModel(product);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}Product/ProductMapping/DuplicateProductMapping.cshtml", model);
        }

        #endregion

        #region Category Management Actions

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [Route("/Admin/Category/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("savenexportcategoryoptions")]
        public async Task<IActionResult> SaveNexportCategoryOptions(NexportCategoryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            if (category == null)
                return RedirectToAction("List", "Category");

            await _genericAttributeService.SaveAttributeAsync(category, NexportDefaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY,
                model.LimitSingleProductPurchase);

            await _genericAttributeService.SaveAttributeAsync(category, NexportDefaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY,
                model.AutoSwapProductPurchase);

            await _genericAttributeService.SaveAttributeAsync(category, NexportDefaults.ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT,
                model.AllowProductPurchaseInCategoryDuringEnrollment);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated"));

            return RedirectToAction("Edit", "Category", new { id = category.Id });
        }

        #endregion

        #region Order Management Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportOrderInvoiceItems(NexportOrderInvoiceItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportOrderInvoice))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportOrderInvoiceItemListModelAsync(searchModel, true);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ModifyNexportEnrollment(int id, int action)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportOrderInvoice))
                return AccessDeniedView();

            var orderInvoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(id);

            if (orderInvoiceItem == null)
                throw new ArgumentException("No Nexport order invoice item found with the specified id", nameof(id));

            var order = await _orderService.GetOrderByIdAsync(orderInvoiceItem.OrderId);

            if (order != null)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(orderInvoiceItem.OrderItemId);

                if (orderItem != null)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                    var productMapping = await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);
                    var nexportUserMapping =
                        await _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                    if (productMapping != null && nexportUserMapping != null)
                    {
                        await _nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(
                            new NexportOrderInvoiceRedemptionQueueItem
                            {
                                OrderInvoiceItemId = orderInvoiceItem.Id,
                                RedeemingUserId = nexportUserMapping.NexportUserId,
                                ProductMappingId = productMapping.Id,
                                OrderItemId = orderInvoiceItem.OrderItemId,
                                UtcDateCreated = DateTime.UtcNow,
                                ManualApprovalAction = action
                            });

                        return Json(new
                        {
                            success = true,
                            message = $"Nexport invoice {orderInvoiceItem.InvoiceItemId} redemption for customer {customer.Email} [{nexportUserMapping.NexportUserId}] has been scheduled. " +
                                      "Please check back later for new order status."
                        });
                    }
                }
            }

            return new NullJsonResult();
        }

        #endregion

        #region Supplemental Question

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> ListSupplementalInfoQuestion()
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            //prepare model
            var model = await _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoQuestionSearchModelAsync(new NexportSupplementalInfoQuestionSearchModel());

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/List.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ListSupplementalInfoQuestion(NexportSupplementalInfoQuestionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoQuestionListModelAsync(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> AddSupplementalInfoQuestion()
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionModelAsync(new NexportSupplementalInfoQuestionModel(), null);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Add.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddSupplementalInfoQuestion(NexportSupplementalInfoQuestionModel model,
            bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var question = model.ToEntity<NexportSupplementalInfoQuestion>();
                question.UtcDateCreated = DateTime.UtcNow;

                await _nexportService.InsertNexportSupplementalInfoQuestion(question);

                _notificationService.SuccessNotification("A new supplemental info question has been added.");

                if (!continueEditing)
                    return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

                return RedirectToAction("EditSupplementalInfoQuestion", "NexportIntegration", new { id = question.Id });
            }

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Add.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditSupplementalInfoQuestion(int id)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var supplementalInfoQuestion = await _nexportService.GetNexportSupplementalInfoQuestionById(id);
            if (supplementalInfoQuestion == null)
                return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

            var model =
                await _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionModelAsync(null, supplementalInfoQuestion);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EditSupplementalInfoQuestion(NexportSupplementalInfoQuestionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var supplementalInfoQuestion = await _nexportService.GetNexportSupplementalInfoQuestionById(model.Id);
            if (supplementalInfoQuestion == null)
                return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

            if (ModelState.IsValid)
            {
                supplementalInfoQuestion = model.ToEntity(supplementalInfoQuestion);
                await _nexportService.UpdateNexportSupplementalInfoQuestion(supplementalInfoQuestion);

                _notificationService.SuccessNotification("Successfully update supplemental info question");

                if (!continueEditing)
                    return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

                return RedirectToAction("EditSupplementalInfoQuestion", "NexportIntegration", new { id = supplementalInfoQuestion.Id });
            }

            //prepare model
            model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionModelAsync(model, supplementalInfoQuestion);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSupplementalInfoQuestion(int id)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var supplementalInfoQuestion = await _nexportService.GetNexportSupplementalInfoQuestionById(id);
            if (supplementalInfoQuestion == null)
                return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");

            await _nexportService.DeleteNexportSupplementalInfoQuestion(supplementalInfoQuestion);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Deleted"));

            return RedirectToAction("ListSupplementalInfoQuestion", "NexportIntegration");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelectedSupplementalInfoQuestion(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                await _nexportService.DeleteNexportSupplementalInfoQuestions(
                    await _nexportService.GetNexportSupplementalInfoQuestionsByIds(selectedIds.ToArray()));
            }

            return Json(new { Result = true });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> SupplementalInfoOptionList(NexportSupplementalInfoOptionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return await AccessDeniedDataTablesJson();

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(searchModel.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental info question found with the specified id");

            var model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionListModelAsync(searchModel, question);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> SupplementalInfoOptionCreatePopup(int questionId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(questionId)
                           ?? throw new ArgumentException("No Nexport supplemental info question found with the specified id",
                               nameof(questionId));

            var model = await _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoOptionModelAsync(new NexportSupplementalInfoOptionModel(), question, null);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> SupplementalInfoOptionCreatePopup(NexportSupplementalInfoOptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(model.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental question found with the specified id");

            if (ModelState.IsValid)
            {
                var option = model.ToEntity<NexportSupplementalInfoOption>();
                option.UtcDateCreated = DateTime.UtcNow;

                await _nexportService.InsertNexportSupplementalInfoOption(option);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
            }

            model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionModelAsync(model, question, null);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> SupplementalInfoOptionEditPopup(int optionId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var option = await _nexportService.GetNexportSupplementalInfoOptionById(optionId)
                         ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id", nameof(optionId));

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(option.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental info question found with the specified id");

            var model = await _nexportPluginModelFactory
                .PrepareNexportSupplementalInfoOptionModelAsync(null, question, option);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> SupplementalInfoOptionEditPopup(NexportSupplementalInfoOptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var option = await _nexportService.GetNexportSupplementalInfoOptionById(model.Id)
                         ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id");

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(option.QuestionId)
                           ?? throw new ArgumentException("No Nexport supplemental question found with the specified id");

            if (ModelState.IsValid)
            {
                option = model.ToEntity(option);

                await _nexportService.UpdateNexportSupplementalInfoOption(option);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
            }

            model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionModelAsync(model, question, option);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSupplementalInfoOption(int id)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var option = await _nexportService.GetNexportSupplementalInfoOptionById(id)
                         ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id", nameof(id));

            await _nexportService.DeleteNexportSupplementalInfoOption(option);

            return new NullJsonResult();
        }

        [Area("Admin")]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddSupplementalInfoOptionGroupAssociation(int optionId, Guid nexportGroupId, string nexportGroupName, string nexportGroupShortName)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            if (nexportGroupId == Guid.Empty)
                throw new ArgumentException("Group Id cannot be empty Guid", nameof(nexportGroupId));

            var supplementalInfoOption = await _nexportService.GetNexportSupplementalInfoOptionById(optionId)
                                         ?? throw new ArgumentException("No Nexport supplemental info option found with the specified id", nameof(optionId));

            await _nexportService.InsertNexportSupplementalInfoOptionGroupAssociation(new NexportSupplementalInfoOptionGroupAssociation
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
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteSupplementalInfoOptionGroupAssociation(int id)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var groupAssociation = await _nexportService.GetNexportSupplementalInfoOptionGroupAssociationById(id)
                                   ?? throw new Exception($"No Nexport supplemental info option group association found with the specified id {id}");

            await _nexportService.DeleteNexportSupplementalInfoOptionGroupAssociation(groupAssociation);

            return new NullJsonResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ChangeSupplementalInfoOptionGroupAssociationStatus(int id)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var groupAssociation = await _nexportService.GetNexportSupplementalInfoOptionGroupAssociationById(id)
                                   ?? throw new Exception($"No Nexport supplemental info option group association found with the specified id {id}");

            groupAssociation.IsActive = !groupAssociation.IsActive;
            groupAssociation.UtcDateModified = DateTime.UtcNow;

            await _nexportService.UpdateNexportSupplementalInfoOptionGroupAssociation(groupAssociation);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetSupplementalInfoOptionGroupAssociations(NexportSupplementalInfoOptionGroupAssociationListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return await AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoOptionGroupAssociationListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> AnswerSupplementalInfoQuestion(string returnUrl)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var store = await _storeContext.GetCurrentStoreAsync();

            var requirements = await _nexportService.GetNexportRequiredSupplementalInfos(customer.Id,
                store.Id);

            var model = await _nexportPluginModelFactory.PrepareNexportSupplementalInfoAnswerQuestionModelAsync(
                 requirements.Select(x => x.QuestionId).ToList(), customer,
                store);

            if (model.QuestionWithoutAnswerIds.Count == 0)
                return Redirect($"{store.Url}{returnUrl.TrimStart('/')}");

            model.ReturnUrl = returnUrl;

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/AnswerQuestions.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SaveSupplementalInfoAnswer(SaveSupplementalInfoAnswers request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var store = await _storeContext.GetCurrentStoreAsync();

            foreach (var answer in request.Answers)
            {
                foreach (var optionId in answer.Options)
                {
                    var newAnswer = new NexportSupplementalInfoAnswer
                    {
                        CustomerId = customer.Id,
                        StoreId = store.Id,
                        QuestionId = answer.QuestionId,
                        OptionId = optionId,
                        Status = NexportSupplementalInfoAnswerStatus.NotProcessed,
                        UtcDateCreated = DateTime.UtcNow
                    };

                    await _nexportService.InsertNexportSupplementalInfoAnswer(newAnswer);

                    await _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(new NexportSupplementalInfoAnswerProcessingQueueItem
                    {
                        AnswerId = newAnswer.Id,
                        UtcDateCreated = DateTime.UtcNow
                    });
                }

                var requiredSupplementalInfos = await _nexportService.GetNexportRequiredSupplementalInfos(
                    customer.Id, store.Id,
                    answer.QuestionId);

                foreach (var requirement in requiredSupplementalInfos)
                {
                    await _nexportService.DeleteNexportRequiredSupplementalInfo(requirement);
                }
            }

            return Json(new
            {
                Result = true
            });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetCustomerSupplementalInfoQuestions(NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return await AccessDeniedDataTablesJson();

            var model =
                await _nexportPluginModelFactory.PrepareNexportSupplementalInfoQuestionListModelAsync(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetCustomerSupplementalInfoAnswers(NexportSupplementalInfoAnswerListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return await AccessDeniedDataTablesJson();

            var model =
                await _nexportPluginModelFactory.PrepareNexportSupplementalInfoAnswerListModelAsync(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditCustomerSupplementalInfoAnsweredQuestion(int customerId, int storeId, int questionId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var store = await _storeService.GetStoreByIdAsync(storeId)
                        ?? throw new Exception($"No store found with the specified id {storeId}");

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(questionId)
                           ?? throw new Exception($"No Nexport supplemental info question found with the specified id {questionId}");

            var model = await _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(customer, store, question);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/EditCustomerSupplementalInfoAnsweredQuestion.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditCustomerSupplementalInfoAnsweredQuestion(int customerId, int storeId, EditSupplementInfoAnswerRequestModel editModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var store = await _storeService.GetStoreByIdAsync(storeId)
                        ?? throw new Exception($"No store found with the specified id {storeId}");

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(editModel.QuestionId)
                           ?? throw new Exception($"No Nexport supplemental info question found with the specified id {editModel.QuestionId}");

            var answers = await _nexportService.GetNexportSupplementalInfoAnswers(customerId, storeId, editModel.QuestionId)
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
                            await _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(updatingAnswer.Id);

                        updatingAnswer.OptionId = newOption;
                        updatingAnswer.Status = NexportSupplementalInfoAnswerStatus.Modified;
                        updatingAnswer.UtcDateModified = DateTime.UtcNow;

                        await _nexportService.UpdateNexportSupplementalInfoAnswer(updatingAnswer);

                        await _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                            new NexportSupplementalInfoAnswerProcessingQueueItem
                            {
                                AnswerId = updatingAnswer.Id,
                                UtcDateCreated = DateTime.UtcNow
                            });

                        foreach (var membership in memberships)
                        {
                            await _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = customerId,
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

                            await _nexportService.InsertNexportSupplementalInfoAnswer(newAnswer);

                            await _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
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
                            await _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(removingAnswer.Id);

                        var removingQuestionId = removingAnswer.QuestionId;
                        if (!removingQuestionIds.Contains(removingQuestionId))
                        {
                            removingQuestionIds.Add(removingAnswer.QuestionId);
                        }

                        await _nexportService.DeleteNexportSupplementalInfoAnswer(removingAnswer);

                        foreach (var membership in memberships)
                        {
                            await _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = customer.Id,
                                    NexportMembershipId = membership.NexportMembershipId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }

                    var questionWithoutAnswerIds =
                        await _nexportService.GetUnansweredQuestions(customerId, storeId, removingQuestionIds);
                    foreach (var questionId in questionWithoutAnswerIds)
                    {
                        await _nexportService.InsertNexportRequiredSupplementalInfo(
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

            var model =
                await _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(customer, store, question);

            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/EditCustomerSupplementalInfoAnsweredQuestion.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteCustomerSupplementalInfoAnswer(int answerId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo))
                return await AccessDeniedDataTablesJson();

            var answer = await _nexportService.GetNexportSupplementalInfoAnswerById(answerId)
                         ?? throw new Exception($"No Nexport supplemental info answer found with the specified id {answerId}");

            var customerId = answer.CustomerId;
            var storeId = answer.StoreId;
            var removingQuestionIds = new List<int> { answer.QuestionId };

            var memberships =
                await _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(answer.Id);

            await _nexportService.DeleteNexportSupplementalInfoAnswer(answer);

            foreach (var membership in memberships)
            {
                await _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                    new NexportGroupMembershipRemovalQueueItem
                    {
                        CustomerId = customerId,
                        NexportMembershipId = membership.NexportMembershipId,
                        UtcDateCreated = DateTime.UtcNow
                    });
            }

            var questionWithoutAnswerIds =
                await _nexportService.GetUnansweredQuestions(customerId, storeId, removingQuestionIds);
            foreach (var questionId in questionWithoutAnswerIds)
            {
                await _nexportService.InsertNexportRequiredSupplementalInfo(
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
        public async Task<IActionResult> ListRegistrationFieldCategory()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Select an appropriate panel
            SaveSelectedTabName("customersettings-nexportregistrationfields");

            // Redirect the user to the customer settings page
            return RedirectToAction("CustomerUser", "Setting");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ListRegistrationFieldCategory(NexportRegistrationFieldCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryListModelAsync(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> CreateRegistrationFieldCategory()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryModelAsync(
                new NexportRegistrationFieldCategoryModel(), null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateRegistrationFieldCategory(NexportRegistrationFieldCategoryModel model, bool continueEditing = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var registrationFieldCategory = model.ToEntity<NexportRegistrationFieldCategory>();
                await _nexportService.InsertNexportRegistrationFieldCategory(registrationFieldCategory);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Added"));

                if (!continueEditing)
                    return RedirectToAction("ListRegistrationFieldCategory");

                return RedirectToAction("EditRegistrationFieldCategory", new { id = registrationFieldCategory.Id });
            }

            model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryModelAsync(
                new NexportRegistrationFieldCategoryModel(), null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditRegistrationFieldCategory(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldCategory = await _nexportService.GetNexportRegistrationFieldCategoryById(id);
            if (registrationFieldCategory == null)
                return RedirectToAction("ListRegistrationFieldCategory");

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldCategoryModelAsync(null, registrationFieldCategory);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EditRegistrationFieldCategory(NexportRegistrationFieldCategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldCategory = await _nexportService.GetNexportRegistrationFieldCategoryById(model.Id);
            if (registrationFieldCategory == null)
                return RedirectToAction("ListRegistrationFieldCategory");

            if (!ModelState.IsValid)
                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Category/Edit.cshtml", model);

            registrationFieldCategory = model.ToEntity(registrationFieldCategory);
            await _nexportService.UpdateNexportRegistrationFieldCategory(registrationFieldCategory);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Updated"));

            if (!continueEditing)
                return RedirectToAction("ListRegistrationFieldCategory");

            return RedirectToAction("EditRegistrationFieldCategory", new { id = registrationFieldCategory.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        public virtual async Task<IActionResult> DeleteRegistrationFieldCategory(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldCategory = await _nexportService.GetNexportRegistrationFieldCategoryById(id);
            await _nexportService.DeleteNexportRegistrationFieldCategory(registrationFieldCategory);

            var registrationFieldsWithCategory = await _nexportService.GetNexportRegistrationFieldsByCategoryId(id);
            foreach (var field in registrationFieldsWithCategory)
            {
                field.FieldCategoryId = null;
                await _nexportService.UpdateNexportRegistrationField(field);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Deleted"));

            return RedirectToAction("ListRegistrationFieldCategory");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> ListRegistrationField()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Select an appropriate panel
            SaveSelectedTabName("customersettings-nexportregistrationfields");

            // Redirect the user to the customer settings page
            return RedirectToAction("CustomerUser", "Setting");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ListRegistrationField(NexportRegistrationFieldSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldListModelAsync(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> CreateRegistrationField()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldModelAsync(new NexportRegistrationFieldModel(), null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateRegistrationField(NexportRegistrationFieldModel model, bool continueEditing = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var registrationField = model.ToEntity<NexportRegistrationField>();
                await _nexportService.InsertNexportRegistrationField(registrationField);

                // Create registration field store mapping for each selected store
                foreach (var storeId in model.StoreMappingIds)
                {
                    await _nexportService.InsertNexportRegistrationFieldStoreMapping(
                        new NexportRegistrationFieldStoreMapping
                        {
                            FieldId = registrationField.Id,
                            StoreId = storeId
                        });
                }

                await UpdateNexportRegistrationFieldLocalesAsync(registrationField, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Added"));

                return !continueEditing
                    ? RedirectToAction("ListRegistrationField")
                    : RedirectToAction("EditRegistrationField", new { id = registrationField.Id });
            }

            model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldModelAsync(
                new NexportRegistrationFieldModel(), null, true);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditRegistrationField(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(id);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldModelAsync(null, registrationField);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditRegistrationField(NexportRegistrationFieldModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(model.Id);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            if (!ModelState.IsValid)
                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Edit.cshtml", model);

            if (model.Type == NexportRegistrationFieldType.CustomType)
            {
                if (await _nexportService.HasCustomRegistrationFieldRenderForStores(
                        registrationField.Id, model.StoreMappingIds, model.CustomFieldRender))
                {
                    _notificationService.ErrorNotification(
                        await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit"));

                    return RedirectToAction("EditRegistrationField", new { id = registrationField.Id });
                }
            }

            registrationField = model.ToEntity(registrationField);

            await _nexportService.UpdateNexportRegistrationField(registrationField);
            if (model.Type is NexportRegistrationFieldType.SelectCheckbox or NexportRegistrationFieldType.SelectDropDown)
            {
                if (model.Type == NexportRegistrationFieldType.SelectCheckbox)
                {
                    await _genericAttributeService.SaveAttributeAsync(registrationField,
                        nameof(model.AllowMultipleSelection), model.AllowMultipleSelection);
                }

                await _genericAttributeService.SaveAttributeAsync(registrationField,
                    nameof(model.DisplayOptionByAscendingOrder), model.DisplayOptionByAscendingOrder);
            }

            var storeMappings =
                await _nexportService.GetNexportRegistrationFieldStoreMappings(registrationField.Id);

            var currentStoreIds = storeMappings
                .Select(x => x.StoreId).ToList();
            var removalStoreIds = currentStoreIds.Except(model.StoreMappingIds);
            var additionalStoreIds = model.StoreMappingIds.Except(currentStoreIds);

            foreach (var storeId in additionalStoreIds)
            {
                await _nexportService.InsertNexportRegistrationFieldStoreMapping(
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
                    await _nexportService.DeleteNexportRegistrationFieldStoreMapping(deletingMapping);
            }

            await UpdateNexportRegistrationFieldLocalesAsync(registrationField, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Updated"));

            if (!continueEditing)
                return RedirectToAction("ListRegistrationField");

            return RedirectToAction("EditRegistrationField", new { id = registrationField.Id });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        public async Task<IActionResult> DeleteRegistrationField(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(id);
            var registrationFieldOptionSettings = await _genericAttributeService.GetAttributesForEntityAsync(registrationField.Id,
                registrationField.GetType().Name);

            await _nexportService.DeleteNexportRegistrationField(registrationField);

            // Delete the option of the registration field
            var registrationFieldOptions = await _nexportService.GetNexportRegistrationFieldOptions(id);
            foreach (var fieldOption in registrationFieldOptions)
            {
                await _nexportService.DeleteNexportRegistrationFieldOption(fieldOption);
            }

            // Delete the store mappings of the registration field
            var registrationFieldStoreMappings = await _nexportService.GetNexportRegistrationFieldStoreMappings(id);
            foreach (var fieldStoreMapping in registrationFieldStoreMappings)
            {
                await _nexportService.DeleteNexportRegistrationFieldStoreMapping(fieldStoreMapping);
            }

            // Delete related extra settings of the registration field
            await _genericAttributeService.DeleteAttributesAsync(registrationFieldOptionSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Deleted"));

            return RedirectToAction("ListRegistrationField");
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ListRegistrationFieldOptions(NexportRegistrationFieldOptionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(searchModel.RegistrationFieldId)
                                    ?? throw new ArgumentException("No Nexport registration field found with the specified id");

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionListModelAsync(searchModel, registrationField);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> CreateRegistrationFieldOption(int fieldId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(fieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModelAsync(
                new NexportRegistrationFieldOptionModel(), registrationField, null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateRegistrationFieldOption(NexportRegistrationFieldOptionModel model, bool continueEditing = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(model.FieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            if (ModelState.IsValid)
            {
                var registrationFieldOption = model.ToEntity<NexportRegistrationFieldOption>();
                await _nexportService.InsertNexportRegistrationFieldOption(registrationFieldOption);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Added"));

                ViewBag.RefreshPage = true;

                return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Create.cshtml", model);
            }

            model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModelAsync(
                new NexportRegistrationFieldOptionModel(), registrationField, null);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Create.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditRegistrationFieldOption(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldOption = await _nexportService.GetNexportRegistrationFieldOptionById(id);
            if (registrationFieldOption == null)
                return RedirectToAction("ListRegistrationField");

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(registrationFieldOption.FieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField");

            var model = await _nexportPluginModelFactory.PrepareNexportRegistrationFieldOptionModelAsync(null, registrationField, registrationFieldOption);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Option/Edit.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditRegistrationFieldOption(NexportRegistrationFieldOptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldOption = await _nexportService.GetNexportRegistrationFieldOptionById(model.Id);
            if (registrationFieldOption == null)
                return new NullJsonResult();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(registrationFieldOption.FieldId);
            if (registrationField == null)
                return new NullJsonResult();

            registrationFieldOption = model.ToEntity(registrationFieldOption);
            await _nexportService.UpdateNexportRegistrationFieldOption(registrationFieldOption);

            return new NullJsonResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [HttpPost]
        public async Task<IActionResult> DeleteRegistrationFieldOption(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationFieldOption =
                await _nexportService.GetNexportRegistrationFieldOptionById(id)
                ?? throw new ArgumentException("No Nexport registration field option found with the specified id", nameof(id));

            await _nexportService.DeleteNexportRegistrationFieldOption(registrationFieldOption);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Deleted"));

            return new NullJsonResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetRegistrationFieldCustomRenderOptionUrl(string systemName, int fieldId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var registrationFieldCustomRender =
                await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(systemName)
                ?? throw new ArgumentException("Registration field custom render could not be loaded");

            var url = registrationFieldCustomRender.GetRenderOptionUrl(fieldId);

            return Json(new { url });
        }

        public async Task<IActionResult> GetRegistrationFieldCustomRenderUrl(string systemName, int fieldId, bool renderAdminView)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var registrationFieldCustomRender =
                await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(systemName)
                ?? throw new ArgumentException("Registration field custom render could not be loaded");

            var url = await registrationFieldCustomRender.GetCustomRenderUrl(fieldId, renderAdminView);

            return Json(new { url });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAdminRegistrationFieldCustomRenderUrl(string systemName, int fieldId)
        {
            return await GetRegistrationFieldCustomRenderUrl(systemName, fieldId, true);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> LoadRegistrationFieldAnswersByStore(int storeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var store = await _storeService.GetStoreByIdAsync(storeId);

            if (store == null)
                return new EmptyResult();

            var model = await _nexportPluginModelFactory.PrepareNexportAddCustomerRegistrationFieldsModel(store);

            return PartialView($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Customer/_Create.RegistrationFieldAnswer.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetNexportRegistrationFieldsForCustomer(NexportCustomerRegistrationFieldWithAnswersListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportCustomerRegistrationFieldWithAnswersListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetNexportRegistrationFieldAnswersForCustomer(NexportCustomerRegistrationFieldAnswerListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportCustomerRegistrationFieldAnswerListModel(searchModel);

            return Json(model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetEditCustomerRegistrationFieldAnswersViewUrl(string systemName, int customerId, int fieldId)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var registrationFieldCustomRender = await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(systemName)
                                                ?? throw new ArgumentException("Registration field custom render could not be loaded");

            var url = registrationFieldCustomRender.GetEditCustomerRegistrationFieldAnswersViewUrl(customerId, fieldId);

            return Json(new { url });
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> AddCustomerRegistrationFieldAnswers(int customerId, int storeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var store = await _storeService.GetStoreByIdAsync(storeId)
                        ?? throw new Exception($"No store found with the specified id {storeId}");

            var model = await _nexportPluginModelFactory.PrepareNexportAddCustomerRegistrationFieldsModel(customer, store);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Customer/AddCustomerRegistrationFieldAnswers.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddCustomerRegistrationFieldAnswers(int customerId, int storeId, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var store = await _storeService.GetStoreByIdAsync(storeId)
                        ?? throw new Exception($"No store found with the specified id {storeId}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Parse Nexport registration fields and check for errors
                    var nexportRegistrationFields = await _nexportService.ParseRegistrationFieldsAsync(form, storeId);

                    // Parse Nexport registration fields with custom type and check for errors
                    var customRegistrationFields = await _nexportService.ParseCustomRegistrationFieldsAsync(form, storeId);

                    // Save Nexport registration fields
                    await _nexportService.SaveNexportRegistrationFields(customer, nexportRegistrationFields);

                    // Save Nexport custom registration fields
                    foreach (var customField in customRegistrationFields)
                    {
                        var registrationField = await _nexportService.GetNexportRegistrationFieldById(customField.Key);
                        if (registrationField != null)
                        {
                            var customRender = await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender);
                            await customRender?.SaveCustomRegistrationFields(customer, registrationField.Id, customField.Value);
                        }
                    }

                    // Schedule synchronization task with Nexport for registration fields
                    await _nexportService.InsertNexportRegistrationFieldSynchronizationQueueItem(new NexportRegistrationFieldSynchronizationQueueItem
                    {
                        CustomerId = customer.Id,
                        UtcDateCreated = DateTime.UtcNow
                    });

                    ViewBag.RefreshPage = true;

                    ViewBag.ClosePage = true;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Error occurred while creating Nexport registration fields", ex, customer);
                    _notificationService.ErrorNotification("Unable to create Nexport registration fields!");
                }
            }

            var model = await _nexportPluginModelFactory.PrepareNexportAddCustomerRegistrationFieldsModel(customer, store);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Customer/AddCustomerRegistrationFieldAnswers.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditCustomerRegistrationFieldAnswers(int customerId, int fieldId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var field = await _nexportService.GetNexportRegistrationFieldById(fieldId)
                           ?? throw new Exception($"No Nexport registration field found with the specified id {fieldId}");

            var model = await _nexportPluginModelFactory.PrepareNexportCustomerRegistrationFieldAnswersEditModel(customer, field);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Customer/EditCustomerRegistrationFieldAnswers.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditCustomerRegistrationFieldAnswers(int customerId, EditRegistrationFieldAnswerRequestModel editModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(customerId)
                           ?? throw new Exception($"No customer found with the specified id {customerId}");

            var field = await _nexportService.GetNexportRegistrationFieldById(editModel.FieldId)
                        ?? throw new Exception($"No Nexport registration field found with the specified id {editModel.FieldId}");

            if (ModelState.IsValid)
            {
                try
                {
                    NexportRegistrationFieldAnswer currentAnswer;
                    switch (field.Type)
                    {
                        case NexportRegistrationFieldType.Text:
                        case NexportRegistrationFieldType.Email:
                        case NexportRegistrationFieldType.Numeric:
                        case NexportRegistrationFieldType.Boolean:
                        case NexportRegistrationFieldType.DateOnly:
                        case NexportRegistrationFieldType.DateTime:
                        case NexportRegistrationFieldType.SelectDropDown:
                            currentAnswer = await _nexportService.GetNexportRegistrationFieldAnswerById(editModel.PreviousAnswers[0]);

                            if (currentAnswer != null)
                            {
                                if (field.Type == NexportRegistrationFieldType.Text ||
                                    field.Type == NexportRegistrationFieldType.Email)
                                    currentAnswer.TextValue = editModel.AnswerValue;
                                else if (field.Type == NexportRegistrationFieldType.Numeric)
                                    currentAnswer.NumericValue = int.Parse(editModel.AnswerValue);
                                else if (field.Type == NexportRegistrationFieldType.Boolean)
                                    currentAnswer.BooleanValue = bool.Parse(editModel.AnswerValue);
                                else if (field.Type == NexportRegistrationFieldType.DateOnly ||
                                            field.Type == NexportRegistrationFieldType.DateTime)
                                    currentAnswer.DateTimeValue = DateTime.Parse(editModel.AnswerValue);
                                else if (field.Type == NexportRegistrationFieldType.SelectDropDown)
                                {
                                    if (editModel.AnswerFieldOptions != null && editModel.AnswerFieldOptions.Count > 0)
                                    {
                                        var newOption = editModel.AnswerFieldOptions[0];
                                        currentAnswer.FieldOptionId = newOption == 0 ? null : (int?)newOption;
                                    }
                                    else
                                        currentAnswer.FieldOptionId = null;
                                }

                                currentAnswer.UtcDateModified = DateTime.UtcNow;
                                await _nexportService.UpdateNexportRegistrationFieldAnswer(currentAnswer);
                            }

                            break;

                        case NexportRegistrationFieldType.SelectCheckbox:
                            if (editModel.AllowMultipleSelection != null && editModel.AllowMultipleSelection.Value)
                            {
                                var currentAnswers = await _nexportService.GetNexportRegistrationFieldAnswers(customerId, editModel.FieldId);
                                var currentAnswersFieldOptions = currentAnswers
                                    .Where(x => x.FieldOptionId != null)
                                    .Select(x => x.FieldOptionId.Value).ToList();

                                var newOptions = editModel.AnswerFieldOptions.Except(currentAnswersFieldOptions);

                                var removingOptions = currentAnswersFieldOptions.Except(editModel.AnswerFieldOptions);

                                foreach (var newOption in newOptions)
                                {
                                    var newAnswer = new NexportRegistrationFieldAnswer
                                    {
                                        CustomerId = customerId,
                                        FieldId = editModel.FieldId,
                                        UtcDateCreated = DateTime.UtcNow,
                                        UtcDateModified = DateTime.UtcNow,
                                        FieldOptionId = newOption
                                    };

                                    await _nexportService.InsertNexportRegistrationFieldAnswer(newAnswer);
                                }

                                foreach (var removingOption in removingOptions)
                                {
                                    currentAnswer = await _nexportService.GetNexportRegistrationFieldAnswerByFieldOption(customerId, editModel.FieldId, removingOption);
                                    if (currentAnswer != null)
                                    {
                                        await _nexportService.DeleteNexportRegistrationFieldAnswer(currentAnswer);
                                    }
                                }
                            }
                            else
                            {
                                currentAnswer = await _nexportService.GetNexportRegistrationFieldAnswerById(editModel.PreviousAnswers[0]);

                                if (currentAnswer != null)
                                {
                                    if (editModel.AnswerFieldOptions is { Count: > 0 })
                                        currentAnswer.FieldOptionId = editModel.AnswerFieldOptions[0];
                                    else
                                        currentAnswer.FieldOptionId = null;

                                    currentAnswer.UtcDateModified = DateTime.UtcNow;
                                    await _nexportService.UpdateNexportRegistrationFieldAnswer(currentAnswer);
                                }
                            }

                            break;

                        case NexportRegistrationFieldType.CustomType:
                            var submittingFields = new Dictionary<string, string>();
                            var customFieldKeys = editModel.FormCollection.Keys.Where(x => x.StartsWith($"NexportCustomProfile-{field.Id}"));
                            foreach (var key in customFieldKeys)
                            {
                                editModel.FormCollection.TryGetValue(key, out var fieldValue);
                                submittingFields.Add(key, fieldValue);
                            }

                            var registrationFieldCustomRender = await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(field.CustomFieldRender);
                            await registrationFieldCustomRender?.UpdateCustomRegistrationFieldAnswers(customer.Id, field.Id, submittingFields);

                            break;


                        case NexportRegistrationFieldType.None:
                            break;
                    }

                    ViewBag.RefreshPage = true;

                    ViewBag.ClosePage = true;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Error occurred while saving Nexport registration field", ex, customer);
                    _notificationService.ErrorNotification("Unable to save Nexport registration field!");
                }
            }

            var model = _nexportPluginModelFactory.PrepareNexportCustomerRegistrationFieldAnswersEditModel(customer, field);

            return View($"{NexportDefaults.NexportPluginAdminViewBasePath}RegistrationField/Customer/EditCustomerRegistrationFieldAnswers.cshtml", model);
        }

        #endregion

        #region Event Handling

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            try
            {
                await _nexportService.CreateAndMapNewNexportUserAsync(eventMessage.Customer);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot create and map new Nexport user for customer {eventMessage.Customer.Id}", ex, eventMessage.Customer);
            }
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            foreach (var item in orderItems)
            {
                var mapping = await _nexportService.GetProductMappingByNopProductId(item.ProductId, order.StoreId)
                              ?? await _nexportService.GetProductMappingByNopProductId(item.ProductId);

                if (mapping != null)
                {
                    var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                    var storeModel = await _genericAttributeService.GetAttributeAsync<NexportStoreSaleModel>(store, "NexportStoreSaleModel", store.Id);
                    await _genericAttributeService.SaveAttributeAsync(item, $"StoreModel-{order.Id}-{item.Id}", JsonConvert.SerializeObject(storeModel), store.Id);

                    await _genericAttributeService.SaveAttributeAsync(item,
                        $"ProductMapping-{order.Id}-{item.Id}",
                        JsonConvert.SerializeObject(mapping), order.StoreId);

                    var groupMembershipMappings = await _nexportService.GetProductGroupMembershipMappings(mapping.Id);
                    foreach (var groupMembershipMapping in groupMembershipMappings)
                    {
                        await _genericAttributeService.SaveAttributeAsync(item,
                            $"ProductGroupMembershipMapping-{order.Id}-{item.Id}-{mapping.Id}",
                            JsonConvert.SerializeObject(groupMembershipMapping), order.StoreId);
                    }
                }
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            var product = eventMessage.Entity;

            var mappings = await _nexportService.GetProductMappings(product.Id);
            foreach (var mapping in mappings)
            {
                await _nexportService.DeleteNexportProductMapping(mapping);

                var groupMembershipMappings = await _nexportService.GetProductGroupMembershipMappings(mapping.Id);
                foreach (var groupMembershipMapping in groupMembershipMappings)
                {
                    await _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);
                }
            }
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;
            if (order.OrderStatus == OrderStatus.Processing && order.PaymentStatus == PaymentStatus.Paid)
            {
                await ProcessNewRedemption(order);
            }
        }

        public async Task ProcessNewRedemption(Order order)
        {
            await _nexportService.InsertNexportOrderProcessingQueueItem(new NexportOrderProcessingQueueItem
            {
                OrderId = order.Id,
                UtcDateCreated = DateTime.UtcNow
            });
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Customer> eventMessage)
        {
            var deletedCustomer = eventMessage.Entity;
            var userMapping = await _nexportService.FindUserMappingByCustomerId(deletedCustomer.Id);
            if (userMapping == null)
                return;

            await _nexportService.DeleteUserMapping(userMapping);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Store> eventMessage)
        {
            var store = eventMessage.Entity;

            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.NEXPORT_STORE_SALE_MODEL_SETTING_KEY,
                NexportStoreSaleModel.Retail, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY,
                true, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY,
                false, store.Id);
            await _genericAttributeService.SaveAttributeAsync(store, NexportDefaults.HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY,
                false, store.Id);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Store> eventMessage)
        {
            var deletedStore = eventMessage.Entity;

            // Find and remove all generic attributes that associated with this store
            var storeAttributes = await _genericAttributeService.GetAttributesForEntityAsync(deletedStore.Id, "Store");
            await _genericAttributeService.DeleteAttributesAsync(storeAttributes);

            // Find and remove product mappings that are associated with this store
            var mappings = await _nexportService.GetProductMappingsByStoreId(deletedStore.Id);
            foreach (var mapping in mappings)
            {
                await _nexportService.DeleteNexportProductMapping(mapping);
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            var category = eventMessage.Entity;

            await _genericAttributeService.SaveAttributeAsync(category,
                NexportDefaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY, false);
            await _genericAttributeService.SaveAttributeAsync(category,
                NexportDefaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY, true);
            await _genericAttributeService.SaveAttributeAsync(category,
                NexportDefaults.ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT, true);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            var deletedCategory = eventMessage.Entity;

            var categoryNexportAttributes = await _genericAttributeService.GetAttributesForEntityAsync(deletedCategory.Id, "Category");
            await _genericAttributeService.DeleteAttributesAsync(categoryNexportAttributes);
        }

        #endregion

        #region Customer Actions

        [HttpsRequirement]
        public async Task<IActionResult> ViewNexportOrderRedemption(NexportOrderInvoiceItem model)
        {
            return View("~/Plugins/Misc.Nexport/Views/ViewOrder.cshtml", model);
        }

        [HttpsRequirement]
        public async Task<IActionResult> ViewNexportTraining()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            try
            {
                var model = await _nexportPluginModelFactory.PrepareNexportTrainingListModelAsync(customer);

                var myTrainingViewLocationSetting =
                    await _settingService.GetSettingAsync("nexport.mytraining.view", (await _storeContext.GetCurrentStoreAsync()).Id, true);

                return View(myTrainingViewLocationSetting != null
                    ? myTrainingViewLocationSetting.Value
                    : "~/Plugins/Misc.Nexport/Views/NexportTrainings.cshtml",
                    model);
            }
            catch (Exception ex)
            {
                var errorMsg = "Cannot display training details.";
                await _logger.ErrorAsync(errorMsg, ex, customer);
                _notificationService.ErrorNotification(errorMsg);
            }

            return new EmptyResult();
        }

        [HttpsRequirement]
        public async Task<IActionResult> ViewSupplementalInfoAnswers()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            try
            {
                var model = await _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersModelAsync(
                        customer, await _storeContext.GetCurrentStoreAsync());

                return View("~/Plugins/Misc.Nexport/Views/NexportCustomer/NexportSupplementalInfoAnswers.cshtml", model);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, customer);
                _notificationService.ErrorNotification(ex.Message);
            }

            return new EmptyResult();
        }

        [HttpsRequirement]
        public async Task<IActionResult> EditSupplementalInfoAnswers(int questionId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(questionId);

            if (question == null)
                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

            var model = await _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(
                customer, await _storeContext.GetCurrentStoreAsync(), question);

            return View("~/Plugins/Misc.Nexport/Views/NexportCustomer/EditNexportSupplementalInfoAnswer.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditSupplementalInfoAnswers(EditSupplementInfoAnswerRequestModel editModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (editModel.OptionIds == null || editModel.OptionIds.Count == 0)
                throw new Exception("List of submission options cannot be null or empty.");

            var store = await _storeContext.GetCurrentStoreAsync();

            var question = await _nexportService.GetNexportSupplementalInfoQuestionById(editModel.QuestionId);

            if (question == null)
                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");

            var answers = await _nexportService.GetNexportSupplementalInfoAnswers(
                customer.Id, store.Id, editModel.QuestionId);

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
                            await _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(updatingAnswer.Id);

                        updatingAnswer.OptionId = newOption;
                        updatingAnswer.Status = NexportSupplementalInfoAnswerStatus.Modified;
                        updatingAnswer.UtcDateModified = DateTime.UtcNow;

                        await _nexportService.UpdateNexportSupplementalInfoAnswer(updatingAnswer);

                        await _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                            new NexportSupplementalInfoAnswerProcessingQueueItem
                            {
                                AnswerId = updatingAnswer.Id,
                                UtcDateCreated = DateTime.UtcNow
                            });

                        foreach (var membership in memberships)
                        {
                            await _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = customer.Id,
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
                                CustomerId = customer.Id,
                                StoreId = store.Id,
                                OptionId = newOption,
                                QuestionId = editModel.QuestionId,
                                Status = NexportSupplementalInfoAnswerStatus.NotProcessed,
                                UtcDateCreated = DateTime.UtcNow
                            };

                            await _nexportService.InsertNexportSupplementalInfoAnswer(newAnswer);

                            await _nexportService.InsertNexportSupplementalInfoAnswerProcessingQueueItem(
                                new NexportSupplementalInfoAnswerProcessingQueueItem
                                {
                                    AnswerId = newAnswer.Id,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }

                    var removingAnswers =
                        answers.Where(a => !editModel.OptionIds.Contains(a.OptionId));
                    foreach (var removingAnswer in removingAnswers)
                    {
                        var memberships =
                            await _nexportService.GetNexportSupplementalInfoAnswerMembershipsByAnswerId(removingAnswer.Id);

                        await _nexportService.DeleteNexportSupplementalInfoAnswer(removingAnswer);

                        foreach (var membership in memberships)
                        {
                            await _nexportService.InsertNexportGroupMembershipRemovalQueueItem(
                                new NexportGroupMembershipRemovalQueueItem
                                {
                                    CustomerId = customer.Id,
                                    NexportMembershipId = membership.NexportMembershipId,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }
                }

                return RedirectToRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers");
            }

            var model = await _nexportPluginModelFactory.PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(
                customer, store, question);

            return View("~/Plugins/Misc.Nexport/Views/NexportCustomer/EditNexportSupplementalInfoAnswer.cshtml", model);
        }

        #endregion

        #region Redeeming Product Actions

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RedeemNexportOrderInvoiceItem(int orderItemInvoiceId, Guid? redeemingUserId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            try
            {
                if (redeemingUserId == null)
                {
                    var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                    redeemingUserId = userMapping.NexportUserId;
                }

                var nexportOrderInvoiceItem =
                    await _nexportService.FindNexportOrderInvoiceItemById(orderItemInvoiceId);

                var order = await _orderService.GetOrderByIdAsync(nexportOrderInvoiceItem.OrderId);

                try
                {
                    await _nexportService.RedeemNexportInvoiceItemAsync(nexportOrderInvoiceItem, redeemingUserId.Value);

                    await _nexportService.AddOrderNoteAsync(order,
                        $"Nexport invoice item {nexportOrderInvoiceItem.InvoiceItemId} has been redeemed for user {redeemingUserId}");
                }
                catch (Exception e)
                {
                    await _nexportService.AddOrderNoteAsync(order,
                        $"Nexport invoice item {nexportOrderInvoiceItem.InvoiceItemId} cannot be redeemed for user {redeemingUserId}");

                    var errorMsg = string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser"),
                        nexportOrderInvoiceItem.InvoiceItemId, redeemingUserId);

                    await _logger.ErrorAsync(errorMsg, e, customer);
                    _notificationService.ErrorNotification(errorMsg);

                    return new EmptyResult();
                }

                return Json(nexportOrderInvoiceItem);
            }
            catch (Exception ex)
            {
                var errorMsg = string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure"), orderItemInvoiceId);

                await _logger.ErrorAsync(errorMsg, ex);
                _notificationService.ErrorNotification(errorMsg);
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> GoToNexport(int orderInvoiceItemId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (orderInvoiceItemId < 1)
                return Content("");

            var orderInvoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(orderInvoiceItemId);

            if (orderInvoiceItem == null)
                throw new Exception("Order invoice does not existed");

            if (orderInvoiceItem.UtcDateRedemption == null)
                throw new Exception("Order invoice has not been redeemed. Unable to access Nexport.");

            dynamic result = new ExpandoObject();

            try
            {
                result.RedirectUrl = _nexportService.SignInNexportAsync(orderInvoiceItem);
            }
            catch (Exception ex)
            {
                var errorMsg = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
                await _logger.ErrorAsync(errorMsg, ex, customer);

                result.Error = errorMsg;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        public async Task<IActionResult> GoToNexportClassroom(Guid enrollmentId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (enrollmentId == Guid.Empty)
                throw new Exception("Enrollment Id cannot be empty.");

            dynamic result = new ExpandoObject();

            try
            {
                result.RedirectUrl = _nexportService.SignInNexportClassroomAsync(enrollmentId);
            }
            catch (Exception ex)
            {
                var errorMsg = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
                await _logger.ErrorAsync(errorMsg, ex, customer);

                result.Error = errorMsg;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        public async Task<IActionResult> GoToNexportOrg(Guid orgId, Guid userId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (orgId == Guid.Empty)
                throw new Exception("Organization Id cannot be empty.");

            dynamic result = new ExpandoObject();

            try
            {
                result.RedirectUrl = _nexportService.SignInNexportAsync(orgId, userId);
            }
            catch (Exception ex)
            {
                var errorMsg = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
                await _logger.ErrorAsync(errorMsg, ex, customer);

                result.Error = errorMsg;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(result);
        }

        #endregion
    }
}
