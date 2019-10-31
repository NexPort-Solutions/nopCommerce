using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
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
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    [ResponseCache(Duration = 0, NoStore = true)]
    public class NexportIntegrationController : BasePluginController,
        IConsumer<CustomerRegisteredEvent>,
        IConsumer<EntityUpdatedEvent<Order>>,
        IConsumer<EntityDeletedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Customer>>
    {

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<NexportProductMapping> _nexportProductRepository;

        private readonly NexportSettings _nexportSettings;
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IOrderModelFactory _orderModelFactory;

        private readonly IWorkContext _workContext;

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductTagService _productTagService;

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
            ICustomerActivityService customerActivityService,
            IProductService productService,
            IStoreService storeService,
            IOrderService orderService,
            ICustomerService customerService,
            IDiscountService discountService,
            ISettingService settingService,
            IPermissionService permissionService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IGenericAttributeService genericAttributeService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
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

            _customerActivityService = customerActivityService;
            _productService = productService;
            _storeService = storeService;
            _orderService = orderService;
            _customerService = customerService;
            _discountService = discountService;

            _nexportSettings = nexportSettings;
            _nexportService = nexportService;
            _settingService = settingService;
            _permissionService = permissionService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _genericAttributeService = genericAttributeService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;

            _logger = logger;
            _webHelper = webHelper;

            _vendorSettings = vendorSettings;
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
        public IActionResult SetNexportSubscriptionOrganizationId(StoreModel model, [FromForm(Name = "NexportSubscriptionOrgId")]Guid subOrgId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(model.Id, false);
            if (store == null)
                return RedirectToAction("List", "Store");

            _genericAttributeService.SaveAttribute(store, "NexportSubscriptionOrganizationId", subOrgId, store.Id);

            _notificationService.SuccessNotification("Success update nexport subscription organization");

            return RedirectToAction("Edit", "Store", new { id = store.Id });
        }

        #endregion

        #region User Configuration Actions

        [HttpsRequirement(SslRequirement.Yes)]
        public IActionResult MapNexportUser()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _nexportPluginModelFactory.PrepareNexportUserMappingModel(_workContext.CurrentCustomer);

            return View("~/Plugins/Misc.Nexport/Views/Customer/NexportUserMapping.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [HttpPost]
        [PublicAntiForgery]
        [Route("Admin/Customer/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("setnexportuserid")]
        public IActionResult MapNexportUser(CustomerModel model, [FromForm(Name = "NexportUserId")]Guid nexportUserId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List", "Customer");

            if (nexportUserId != Guid.Empty)
            {
                _nexportService.InsertUserMapping(new NexportUserMapping()
                {
                    NexportUserId = nexportUserId,
                    NopUserId = customer.Id
                });

                _notificationService.SuccessNotification("Success update Nexport user mapping");
            }

            return RedirectToAction("Edit", "Customer", new { id = customer.Id });
        }

        #endregion

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

            var model = new NexportCatalogSearchModel() { OrgId = orgId };

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

            var model = _nexportPluginModelFactory.PrepareNexportSyllabusListMode(searchModel);

            return Json(model);
        }

        #region Mapping Actions

        [AuthorizeAdmin]
        [Area("Admin")]
        [AdminAntiForgery]
        public IActionResult ProductMappingDetailsPopup(int mappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productMapping = _nexportService.GetProductMappingById(mappingId);

            var model = productMapping.ToModel<NexportProductMappingModel>();
            model.Editable = true;

            return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GetProductMappings(NexportProductMappingSearchModel searchModel, Guid nexportProductId, NexportProductTypeEnum nexportProductType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportProductMappingListModel(searchModel, nexportProductId, nexportProductType);

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

            _nexportService.UpdateMapping(productMapping);

            if (!continueEditing)
            {
                ViewBag.ClosePage = true;
            }

            ViewBag.RefreshPage = true;

            model.UtcLastModifiedDate = productMapping.UtcLastModifiedDate;
            model.Editable = true;

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

            _nexportService.DeleteMapping(mapping);

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

            _nexportService.InsertNexportProductGroupMembershipMapping(new NexportProductGroupMembershipMapping()
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
        public IActionResult MapProductPopup(Guid nexportProductId, NexportProductTypeEnum nexportProductType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            // Prepare model
            var model =
                _nexportPluginModelFactory.PrepareNexportProductMappingSearchModel(
                    new NexportProductMappingSearchModel());

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

        #endregion

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

        public IActionResult ViewNexportOrderRedemption(NexportOrderInvoiceItem model)
        {
            return View("~/Plugins/Misc.Nexport/Views/ViewOrder.cshtml", model);
        }

        public IActionResult ViewNexportTraining()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            try
            {
                var model = _nexportPluginModelFactory.PrepareNexportTrainingListModel(_workContext.CurrentCustomer);

                return View("~/Plugins/Misc.Nexport/Views/NexportTrainings.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, _workContext.CurrentCustomer);
                _notificationService.ErrorNotification(ex.Message);
            }

            return new EmptyResult();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Route("Admin/Product/Edit/{id}")]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("syncnexportproduct")]
        public IActionResult SyncNexportProductWithNopProduct(ProductModel model, [FromForm(Name = "NexportMappingId")]int mappingId)
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

        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            var product = eventMessage.Entity;

            var mapping = _nexportService.GetProductMappingByNopProductId(product.Id);
            if (mapping != null)
            {
                _nexportService.DeleteMapping(mapping);
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
            _nexportService.InsertNexportOrderProcessingQueueItem(new NexportOrderProcessingQueueItem()
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

                    var errorMsg = $"Failed to redeem Nexport invoice item {nexportOrderInvoiceItem.InvoiceItemId} for user {redeemingUserId}";

                    _logger.Error(errorMsg, e, customer);
                    _notificationService.ErrorNotification(errorMsg);

                    return new EmptyResult();
                }

                return Json(nexportOrderInvoiceItem);
            }
            catch (Exception ex)
            {
                var errorMsg =
                    $"Error occured during the redemption process for the order item invoice {orderItemInvoiceId}";

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

            try
            {
                var signInUrl = _nexportService.SignInNexport(orderInvoiceItem);

                return RedirectPermanent(signInUrl);
            }
            catch (Exception ex)
            {
                var errorMsg =
                    "Error occured during the transferring to Nexport. You might not have an active subscription in Nexport Campus. " +
                    "Please contact customer service for further assistance.";
                _logger.Error(errorMsg, ex);

                return Content(errorMsg);
            }
        }

        public IActionResult GoToNexportOrg(Guid orgId, Guid userId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (orgId == Guid.Empty)
                return Content("");

            try
            {
                return RedirectPermanent(_nexportService.SignInNexport(orgId, userId));
            }
            catch (Exception ex)
            {
                var errorMsg =
                    "Error occured during the transferring to Nexport organization. You might not have an active subscription in Nexport Campus. " +
                    "Please contact customer service for further assistance.";
                _logger.Error(errorMsg, ex);

                return Content(errorMsg);
            }
        }

        #endregion
    }
}
