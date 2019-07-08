using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
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
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    public class NexportIntegrationController : BasePluginController,
        IConsumer<EntityInsertedEvent<Order>>, IConsumer<EntityUpdatedEvent<Order>>, IConsumer<OrderPlacedEvent>,
        IConsumer<EntityUpdatedEvent<Product>>, IConsumer<EntityDeletedEvent<Product>>
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
        public IActionResult GetOrganizations(string searchTerm, int? page = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return Configure();

            return _nexportService.GenerateOrganizationList(searchTerm, page);
        }

        #endregion

        #region Plugin Configuration Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
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
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                _nexportService.GenerateNewNexportToken(model);
            }

            return Configure();
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("setserverurl")]
        public IActionResult SetServerUrl(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            _settingService.SetSetting("nexportsettings.url", model.Url);

            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AdminAntiForgery]
        public IActionResult SetRootOrganization(Guid orgId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                _nexportSettings.RootOrganizationId = orgId;
                _settingService.SaveSetting(_nexportSettings);

                _settingService.ClearCache();
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot set the root organization", ex);
                _notificationService.ErrorNotification("Cannot set the root organization");
            }

            return Content(orgId.ToString());
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AdminAntiForgery]
        public IActionResult SetMerchantAccount(Guid merchantAccountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                _nexportSettings.MerchantAccountId = merchantAccountId;
                _settingService.SaveSetting(_nexportSettings);

                _settingService.ClearCache();
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot set the merchant account", ex);
                _notificationService.ErrorNotification("Cannot set the merchant account");
            }

            return Content(merchantAccountId.ToString());
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

        //[Area(AreaNames.Admin)]
        //[AuthorizeAdmin]
        //public IActionResult ConfigureDiscount(int discountId, int? discountRequirementId)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
        //        return Content("Access denied");

        //    //load the discount
        //    var discount = _discountService.GetDiscountById(discountId);
        //    if (discount == null)
        //        throw new ArgumentException("Discount could not be loaded");

        //    //check whether the discount requirement exists
        //    if (discountRequirementId.HasValue && discount.DiscountRequirements.All(requirement => requirement.Id != discountRequirementId.Value))
        //        return Content("Failed to load requirement.");

        //    //try to get previously saved restricted customer role identifier
        //    var settingKey = _settingService.GetSettingByKey<string>(string.Format(NexportDefaults.NexportDiscountSettingsKey, discountRequirementId ?? 0));

        //    var model = new AttributeDiscountRequirementModel
        //    {
        //        RequirementId = discountRequirementId ?? 0,
        //        DiscountId = discountId
        //    };

        //    if (!string.IsNullOrEmpty(settingKey))
        //    {
        //        model.ProductAttributeValues = settingKey.Split(',', StringSplitOptions.RemoveEmptyEntries);
        //    }

        //    var products = _productService.SearchProducts();
        //    foreach (var product in products)
        //    {
        //        var specAttributes = product.ProductSpecificationAttributes;
        //        foreach (var attr in specAttributes)
        //        {
        //            model.AvailableProductAttributeValues.Add(new SelectListItem
        //            {
        //                Text = $"{attr.SpecificationAttributeOption.SpecificationAttribute.Name} ({attr.SpecificationAttributeOption.Name})",
        //                Value = $"{product.Id} : {attr.SpecificationAttributeOption.Id}"
        //            });
        //        }
        //    }

        //    ////set the HTML field prefix
        //    ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(NexportDefaults.NexportDiscountHtmlFieldPrefix, discountRequirementId ?? 0);

        //    return View("~/Plugins/Misc.Nexport/Views/Discount/Configure.cshtml", model);
        //}

        //[Area(AreaNames.Admin)]
        //[AuthorizeAdmin]
        //[HttpPost]
        //[AdminAntiForgery]
        //public IActionResult ConfigureDiscount(int discountId, int? discountRequirementId, List<string> productAttributeValues)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
        //        return Content("Access denied");

        //    //load the discount
        //    var discount = _discountService.GetDiscountById(discountId);
        //    if (discount == null)
        //        throw new ArgumentException("Discount could not be loaded");

        //    //get the discount requirement
        //    var discountRequirement = discountRequirementId.HasValue
        //        ? discount.DiscountRequirements.FirstOrDefault(requirement => requirement.Id == discountRequirementId.Value) : null;

        //    //the discount requirement does not exist, so create a new one
        //    if (discountRequirement == null)
        //    {
        //        discountRequirement = new DiscountRequirement
        //        {
        //            DiscountRequirementRuleSystemName = NexportDefaults.NexportDiscountSystemName
        //        };
        //        discount.DiscountRequirements.Add(discountRequirement);
        //        _discountService.UpdateDiscount(discount);
        //    }

        //    //save restricted customer role identifier
        //    _settingService.SetSetting(string.Format(NexportDefaults.NexportDiscountSettingsKey, discountRequirement.Id), string.Join(",", productAttributeValues));

        //    return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        //}

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

        [HttpPost]
        [PublicAntiForgery]
        public IActionResult MapNexportUser(NexportUserMappingModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var customer = _workContext.CurrentCustomer;

            try
            {
                if (ModelState.IsValid)
                {
                    if (_nexportService.FindUserMappingByCustomerId(customer.Id) == null)
                    {
                        _nexportService.InsertUserMapping(new NexportUserMapping()
                        {
                            NexportUserId = model.NexportUserId.Value,
                            NopUserId = customer.Id
                        });
                    }

                    return MapNexportUser();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View("~/Plugins/Misc.Nexport/Views/Customer/NexportUserMapping.cshtml", model);
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
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GetProductMappings(Guid nexportProductId, NexportProductTypeEnum nexportProductType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return AccessDeniedDataTablesJson();

            var model = _nexportPluginModelFactory.PrepareNexportProductMappingListModel(new NexportProductMappingSearchModel(), nexportProductId, nexportProductType);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult EditMapping(NexportProductMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var productMapping = _nexportService.FindProductMappingById(model.Id)
                ?? throw new ArgumentException("No product mapping found with the specified id");

            // Fill entity from product
            productMapping = model.ToEntity(productMapping);
            _nexportService.UpdateMapping(productMapping);

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

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [HttpPost]
        public IActionResult DeleteMapping(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var mapping = _nexportService.FindProductMappingById(id) ??
                          throw new ArgumentException("No nexport mapping found with the specified id", nameof(id));

            _nexportService.DeleteMapping(mapping);

            return new NullJsonResult();
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
                    var catalogModel = _nexportService.FindCatalogDetails(nexportProductId);

                    return View("~/Plugins/Misc.Nexport/Views/Catalog/CatalogDetails.cshtml", catalogModel);

                case NexportProductTypeEnum.Section:
                    if (nexportSyllabusId != null)
                    {
                        var sectionModel = _nexportService.FindSectionDetails(nexportSyllabusId.Value);

                        return View("~/Plugins/Misc.Nexport/Views/Syllabus/Section/SectionDetails.cshtml", sectionModel);
                    }

                    throw new ArgumentNullException(nameof(nexportSyllabusId), "Syllabus Id cannot be null!");

                case NexportProductTypeEnum.TrainingPlan:
                    if (nexportSyllabusId != null)
                    {
                        var trainingPlanModel = _nexportService.FindTrainingPlanDetails(nexportSyllabusId.Value);

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

            var mapping = _nexportService.FindProductMappingById(mappingId);

            if (ModelState.IsValid)
            {
                product = model.ToEntity(product);

                switch (mapping.Type)
                {
                    case NexportProductTypeEnum.Catalog:
                        var catalogDetails = _nexportService.FindCatalogDetails(mapping.NexportCatalogId);
                        var catalogDescription = _nexportService.FindCatalogDescription(mapping.NexportCatalogId);
                        var catalogCreditHours = _nexportService.FindCatalogCreditHours(mapping.NexportCatalogId);

                        product.Name = catalogDetails.Name;
                        product.FullDescription = catalogDescription.Description;
                        mapping.CreditHours = catalogCreditHours.CreditHours;

                        break;

                    case NexportProductTypeEnum.Section:
                        if (!mapping.NexportSyllabusId.HasValue)
                        {
                            throw new Exception("Section Id cannot be null");
                        }

                        var sectionId = mapping.NexportSyllabusId.Value;

                        var sectionDetails = _nexportService.FindSectionDetails(sectionId);
                        var sectionDescription = _nexportService.FindSectionDescription(sectionId);
                        var sectionObjective = _nexportService.FindSectionObjectives(sectionId);

                        product.Name = sectionDetails.Title;
                        product.FullDescription = sectionDescription.Description;
                        product.ShortDescription = sectionObjective.Objectives;
                        product.Sku = sectionDetails.SectionNumber;
                        product.AvailableStartDateTimeUtc = sectionDetails.EnrollmentStart;
                        product.AvailableEndDateTimeUtc = sectionDetails.EnrollmentEnd;

                        mapping.CreditHours = sectionDetails.CreditHours;

                        break;

                    case NexportProductTypeEnum.TrainingPlan:
                        if (!mapping.NexportSyllabusId.HasValue)
                        {
                            throw new Exception("Training Plan Id cannot be null");
                        }

                        var trainingPlanId = mapping.NexportSyllabusId.Value;

                        var trainingPlanDetails = _nexportService.FindTrainingPlanDetails(trainingPlanId);
                        var trainingPlanDescription = _nexportService.FindTrainingPlanDescription(trainingPlanId);

                        product.Name = trainingPlanDetails.Title;
                        product.FullDescription = trainingPlanDescription.Description;
                        product.AvailableStartDateTimeUtc = trainingPlanDetails.EnrollmentStart;
                        product.AvailableEndDateTimeUtc = trainingPlanDetails.EnrollmentEnd;

                        mapping.CreditHours = trainingPlanDetails.CreditHours;

                        break;

                    default:
                        goto case NexportProductTypeEnum.Catalog;
                }

                _productService.UpdateProduct(product);

                mapping.IsSynchronized = true;
                mapping.UtcLastSynchronizationDate = DateTime.UtcNow;

                _nexportService.UpdateMapping(mapping);

                _customerActivityService.InsertActivity("EditProduct",
                    string.Format(_localizationService.GetResource("ActivityLog.EditProduct"), product.Name), product);

                _notificationService.SuccessNotification("The product has been synchronized successfully with Nexport data");
            }

            return RedirectToAction("Edit", "Product", new { id = mapping.NopProductId });
        }

        #region Event Handling

        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {
            var product = eventMessage.Entity;
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

        public void HandleEvent(EntityInsertedEvent<Order> eventMessage)
        {
        }

        public void HandleEvent(EntityUpdatedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;
            if (order.OrderStatus == OrderStatus.Complete)
            {
                ProcessNewRedemption(order);
            }
        }

        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;
            if (order.OrderStatus == OrderStatus.Complete)
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

                _nexportService.RedeemNexportOrder(nexportOrderInvoiceItem, redeemingUserId.Value);

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
                    $"Error occured during the redemption process for the order item invoice {orderInvoiceItem.InvoiceItemId}";

                _logger.Error(errorMsg, ex);
                _notificationService.ErrorNotification(errorMsg);
            }

            return new EmptyResult();
        }

        #endregion
    }
}
