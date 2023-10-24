using System.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Controllers;
using static Nop.Plugin.Misc.Nexport.LogType;
using Nop.Plugin.Misc.Nexport.Factories;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
internal class ProductMappingController : BaseAdminController
{
    private readonly Settings _settings;
    private readonly ICustomerActivityService _customerActivity;
    private readonly IPluginModelFactory _model;
    private readonly IProductMappingService _productMapping;
    private readonly IProductService _product;
    private readonly IStoreService _store;
    private readonly IWorkContext _workContext;
    private readonly IPermissionService _permission;
    private readonly INotificationService _notification;
    private readonly ILogger _logger;
    private readonly IOrganizationService _organization;
    private readonly ISupplementalInfoService _supplementalInfo;
    private readonly IProductGroupMembershipService _productGroupMembership;

    public ProductMappingController(
        Settings settings,
        ICustomerActivityService customerActivity,
        IPluginModelFactory model,
        IProductMappingService productMapping,
        IProductService product,
        IStoreService store,
        IWorkContext workContext,
        IPermissionService permission,
        INotificationService notification,
        ILogger logger,
        IOrganizationService organization,
        ISupplementalInfoService supplementalInfo,
        IProductGroupMembershipService productGroupMembership)
    {
        _settings = settings;
        _customerActivity = customerActivity;
        _model = model;
        _productMapping = productMapping;
        _product = product;
        _store = store;
        _workContext = workContext;
        _permission = permission;
        _notification = notification;
        _logger = logger;
        _organization = organization;
        _supplementalInfo = supplementalInfo;
        _productGroupMembership = productGroupMembership;
    }

    [HttpGet]
    public async Task<IActionResult> GetCatalogList(CatalogSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }
        return View("~/Plugins/Misc.Nexport/Views/MapProductList.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> CatalogList(CatalogSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        if (model.OrgId is not Guid orgId)
        {
            return BadRequest($"{nameof(model.OrgId)} must have a value.");
        }

        return Json(_model.CatalogListModel(orgId, model));
    }

    [HttpPost]
    public async Task<IActionResult> SyllabusList(SyllabusListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.SyllabusListModel(model));
    }

    [HttpPost]
    public async Task<IActionResult> GetCatalogs(CatalogSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        if (model.OrgId is not Guid orgId)
        {
            return BadRequest($"{nameof(model.OrgId)} must have a value.");
        }

        return Json(_model.CatalogListModel(orgId, model));
    }

    [HttpPost]
    public async Task<IActionResult> GetSyllabuses(SyllabusListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.SyllabusListModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> ProductMappingDetailsPopup(int mappingId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return AccessDeniedView();
        }
        if (await _productMapping.GetById(mappingId) is not { } productMapping)
        {
            return BadRequest("Product mapping not found.");
        }
        var model = await _model.ProductMappingModel(productMapping, true);
        return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> GetProductMappings(ProductMappingListSearchModel model, int? nopProductId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        if (nopProductId is null)
        {
            return Json(new ProductMappingListModel());
        }

        return Json(await _model.ProductMappingListModel(model, nopProductId.Value));
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> EditMapping(ProductMappingModel model, bool continueEditing)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return AccessDeniedView();
        }
        if (await _productMapping.GetById(model.Id) is not { } productMapping)
        {
            return BadRequest("No product newMapping found with the specified newMapping");
        }
        if (!ModelState.IsValid)
        {
            model = await _model.ProductMappingModel(productMapping, true);
        }
        try
        {
            await Initialize(model, productMapping);
            await DoEdit(model, continueEditing, productMapping);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Cannot save the edited product newMapping {model.Id} for product {model.NopProductId}.";
            if (exception is ApiException)
            {
                errorMessage += $" ({exception.Message})";
            }
            await _logger.ErrorAsync(errorMessage, exception);
            _notification.ErrorNotification(errorMessage);
        }
        return View("~/Plugins/Misc.Nexport/Views/ProductMappingDetailsPopup.cshtml", model);
    }

    private async Task Initialize(ProductMappingModel model, ProductMapping productMapping)
    {
        productMapping = model.ToEntity(productMapping);
        if (productMapping.SubscriptionOrgId is null)
        {
            productMapping.SubscriptionOrgName = null;
            productMapping.SubscriptionOrgShortName = null;
        }
        else if (string.IsNullOrWhiteSpace(model.SubscriptionOrgName)
            && await _organization.GetOrganizationDetails(productMapping.SubscriptionOrgId.Value) is { } organizationDetails)
        {
            productMapping.SubscriptionOrgName = organizationDetails.Name;
            productMapping.SubscriptionOrgShortName = organizationDetails.ShortName;
        }
        productMapping.UtcLastModifiedDate = DateTime.UtcNow;
    }

    private async Task DoEdit(ProductMappingModel model, bool continueEditing, ProductMapping productMapping)
    {
        await _productMapping.Update(productMapping);
        var nopProduct = await _product.GetProductByIdAsync(model.NopProductId);
        var storeText = productMapping.StoreId is { } storeId && await _store.GetStoreByIdAsync(storeId) is { Id: var id, Name: var name }
            ? $"Store ID: {id}, Name: {name}" : "Store Name: Default";
        var commentContext = getComment(productMapping, nopProduct, storeText);
        await _customerActivity.InsertActivityAsync(EDIT_PRODUCT_MAPPING, "Edited " + commentContext, productMapping);
        var questionMappings = await _supplementalInfo.GetSupplementalInfoQuestionMappings(productMapping.Id);
        var currentQuestionIds = questionMappings.ConvertAll(supplementalInfoQuestionMapping => supplementalInfoQuestionMapping.QuestionId);
        var removalQuestionIds = currentQuestionIds.Except(model.SupplementalInfoQuestionIds);
        await InsertMissingWantedMappings(model, productMapping, commentContext, currentQuestionIds);
        await DeleteUnwantedMappings(productMapping, commentContext, questionMappings, removalQuestionIds);
        ViewBag.ClosePage = !continueEditing;

        static string getComment(ProductMapping productMapping, Product nopProduct, string storeText)
            => $"product newMapping ({storeText}, "
                + $"NexPort product ID: {productMapping.CatalogSyllabusLinkId}, "
                + $"Name: {productMapping.ProductName}) "
                + $"in product (ID: {nopProduct.Id}, Name: {nopProduct.Name})";
    }

    private async Task InsertMissingWantedMappings(ProductMappingModel model, ProductMapping productMapping, string commentContext, List<int> currentQuestionIds)
    {
        foreach (var newMapping in model.SupplementalInfoQuestionIds
            .Except(currentQuestionIds)
            .Select(questionId => new QuestionMapping
                {
                    ProductMappingId = productMapping.Id,
                    QuestionId = questionId,
                    UtcDateCreated = DateTime.UtcNow,
                    UtcDateModified = DateTime.UtcNow,
                }))
        {
            await _supplementalInfo.InsertSupplementalInfoQuestionMapping(newMapping);
            var comment = $"Inserted question (ID:{newMapping.QuestionId}) for " + commentContext;
            await _customerActivity.InsertActivityAsync(INSERT_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING, comment, productMapping);
        }
    }

    private async Task DeleteUnwantedMappings(ProductMapping productMapping, string commentContext, List<QuestionMapping> questionMappings, IEnumerable<int> removalQuestionIds)
    {
        foreach (var unwantedMapping in questionMappings.IntersectBy(removalQuestionIds, mapping => mapping.QuestionId))
        {
            await _supplementalInfo.DeleteSupplementalInfoQuestionMapping(unwantedMapping);
            var comment = $"Deleted question (ID:{unwantedMapping.Id}) from " + commentContext;
            await _customerActivity.InsertActivityAsync(DELETE_SUPPLEMENTAL_INFO_QUESTION_PRODUCT_MAPPING, comment, productMapping);
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMapping(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping))
        {
            return AccessDeniedView();
        }
        if (await _productMapping.GetById(id) is not { } mapping)
        {
            return BadRequest($"No product mapping found with the specified id: {id}");
        }
        await _productMapping.Delete(mapping);
        var groupMembershipMappings = await _productGroupMembership.GetProductGroupMembershipMappings(mapping.Id);
        foreach (var groupMembershipMapping in groupMembershipMappings)
        {
            await _productGroupMembership.DeleteGroupMembershipMapping(groupMembershipMapping);
        }
        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> HasDefaultMapping(int productId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
        {
            return AccessDeniedView();
        }

        return Json(await _productMapping.HasDefaultMapping(productId));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMappings(ICollection<int> selectedIds)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping))
        {
            return AccessDeniedView();
        }
        foreach (var id in selectedIds)
        {
            if (await _productMapping.GetById(id) is not { } mapping)
            {
                continue;
            }
            await _productMapping.Delete(mapping);
            var nopProduct = await _product.GetProductByIdAsync(mapping.NopProductId);
            var storeText = string.Empty;
            if (mapping.StoreId is { } storeId
                && await _store.GetStoreByIdAsync(storeId) is { } store)
            {
                storeText = $"Store ID: {store.Id}, Name: {store.Name}";
            }
            var comment = $"Deleted NexPort product mapping ({storeText}, "
                + $"NexPort Product ID: {mapping.CatalogSyllabusLinkId}, "
                + $"Name: {mapping.ProductName}) in product "
                + $"(ID: {nopProduct.Id}, Name: {nopProduct.Name})";
            await _customerActivity.InsertActivityAsync(DELETE_PRODUCT_MAPPING, comment, mapping);
            var groupMembershipMappings = await _productGroupMembership.GetProductGroupMembershipMappings(mapping.Id);
            foreach (var groupMembershipMapping in groupMembershipMappings)
            {
                await _productGroupMembership.DeleteGroupMembershipMapping(groupMembershipMapping);
                var comment1 = $"Deleted NexPort group membership mapping (Group ID: {groupMembershipMapping.GroupId}, "
                    + $"Group Name: {groupMembershipMapping.GroupName}) "
                    + $"from product (ID: {mapping.CatalogSyllabusLinkId}, Name: {mapping.ProductName}) "
                    + $"in product (ID: {nopProduct.Id}, Name: {nopProduct.Name})";
                await _customerActivity.InsertActivityAsync(DELETE_GROUP_MEMBERSHIP_MAPPING, comment1, mapping);
            }
        }
        return Json(new { Result = true });
    }

    [HttpPost]
    public async Task<IActionResult> GetProductGroupMembershipMappings(ProductGroupMembershipMappingListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.ProductMappingGroupMembershipListModel(model));
    }

    [HttpPost]
    public async Task<IActionResult> AddGroupMembershipMapping(int productMappingId, Guid groupId, string groupName, string groupShortName)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping))
        {
            return AccessDeniedView();
        }
        if (await _productMapping.GetById(productMappingId) is not { } productMapping)
        {
            return BadRequest($"No nexport product mapping found with the specified id: {productMappingId}");
        }
        await _productGroupMembership.InsertProductGroupMembershipMapping(new ProductGroupMembershipMapping
            {
                GroupId = groupId,
                GroupName = groupName,
                GroupShortName = groupShortName,
                ProductMappingId = productMapping.Id,
            });
        return Json(new { Result = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGroupMembershipMapping(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping))
        {
            return AccessDeniedView();
        }
        if (await _productGroupMembership.GetProductGroupMembershipMappingById(id) is not { } groupMembershipMapping)
        {
            return BadRequest($"No group membership mapping found with the specified id: {id}");
        }
        await _productGroupMembership.DeleteGroupMembershipMapping(groupMembershipMapping);
        return new NullJsonResult();
    }

    [HttpGet]
    public async Task<IActionResult> MapProductPopup()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return AccessDeniedView();
        }

        return View("~/Plugins/Misc.Nexport/Views/MapProduct.cshtml");
    }

    [HttpPost]
    [FormValueRequired("save")]
    public async Task<IActionResult> MapProductPopup(MapProductModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return AccessDeniedView();
        }
        if (!ModelState.IsValid)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Json(new { Error = $"Cannot map the product [{model.NopProductId}] with the NexPort product [{model.ProductId}]" });
        }
        try
        {
            dynamic result = new ExpandoObject();
            var oldMapping = await _productMapping.GetByNopProductId(model.NopProductId, model.StoreId);
            await _productMapping.MapProduct(model);
            ViewBag.RefreshPage = true;
            ViewBag.ClosePage = false;
            if (await _productMapping.GetByNopProductId(model.NopProductId, model.StoreId) is not { } newMapping)
            {
                return BadRequest($"Product {model.NopProductId} on store {model.StoreId} not found.");
            }
            if (newMapping is { Id: var mappingId })
            {
                result.MappingId = mappingId;
            }
            var nopProduct = await _product.GetProductByIdAsync(model.NopProductId);
            var storeText = "Store Name: Default";
            if (newMapping is { StoreId: not null }
                && await _store.GetStoreByIdAsync(newMapping.StoreId.Value) is { Id: var storeId, Name: var name })
            {
                storeText = $"Store ID: {storeId}, Name: {name}";
            }
            var comment = oldMapping switch
            {
                not null => $"Modified NexPort product newMapping from ({storeText}, "
                    + $"NexPort product ID: {oldMapping.CatalogSyllabusLinkId}, "
                    + $"Name: {oldMapping.ProductName}, "
                    + $"Type: {oldMapping.Type}, "
                    + $"Catalog ID: {oldMapping.CatalogId}, "
                    + $"Syllabus ID: {oldMapping.SyllabusId}) "
                    + $"to ({storeText}, "
                    + $"NexPort product ID: {newMapping.CatalogSyllabusLinkId}, "
                    + $"Name: {newMapping.ProductName}, "
                    + $"Type: {newMapping.Type}, "
                    + $"Catalog ID: {newMapping.CatalogId}, "
                    + $"Syllabus ID: {newMapping.SyllabusId}) "
                    + $"in product (ID: {nopProduct.Id}, Name: {nopProduct.Name})",
                _ => "Created NexPort product newMapping - "
                    + $"NexPort product ID: {newMapping.CatalogSyllabusLinkId}, "
                    + $"Name: {newMapping.ProductName}, "
                    + $"Type: {newMapping.Type}, "
                    + $"Catalog ID: {newMapping.CatalogId}, "
                    + $"Syllabus ID: {newMapping.SyllabusId}) "
                    + $"in product (ID: {nopProduct.Id}, Name: {nopProduct.Name})",
            };
            await _customerActivity.InsertActivityAsync(MODIFY_PRODUCT_MAPPING, comment, newMapping);
            return Json(result);
        }
        catch (Exception exception)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var logMessage = $"Error occurred while newMapping the product [{model.NopProductId}] with the NexPort product [{model.ProductId}]";
            await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());
            var errorMessage = $"Cannot map the product [{model.NopProductId}] with the NexPort product [{model.ProductId}].";
            if (exception is ApiException { ErrorCode: (int)ApiErrorEntity.ErrorCodeEnum.UnknownError })
            {
                var innerError = $"Product [{model.ProductId}] is missing in Nexport.";
                return Json(new { Error = errorMessage, InnerError = innerError });
            }
            return Json(new { Error = errorMessage });
        }
    }

    [HttpGet]
    public async Task<IActionResult> DuplicateProductMapping(int productId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        {
            return AccessDeniedView();
        }
        if (await _product.GetProductByIdAsync(productId) is not { } product)
        {
            return BadRequest($"No product found with the specified id: {productId}");
        }
        var model = await _model.DuplicateProductMappingModel(product);
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}Product/ProductMapping/DuplicateProductMapping.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> DuplicateProductMapping(int productId, DuplicateProductMappingModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        {
            return AccessDeniedView();
        }
        if (await _product.GetProductByIdAsync(productId) is not { } product)
        {
            return BadRequest($"No product found with the specified id: {productId}");
        }
        if (!ModelState.IsValid)
        {
            model = await _model.DuplicateProductMappingModel(product);
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}Product/ProductMapping/DuplicateProductMapping.cshtml", model);
        }
        try
        {
            if (await _productMapping.GetByNopProductId(product.Id, model.SourceStoreId) is not { } productMapping)
            {
                ViewBag.RefreshPage = true;
                ViewBag.ClosePage = true;
                model = await _model.DuplicateProductMappingModel(product);
                return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}Product/ProductMapping/DuplicateProductMapping.cshtml", model);
            }
            var nopProduct = await _product.GetProductByIdAsync(productMapping.NopProductId);
            var storeText = productMapping.StoreId is not null && await _store.GetStoreByIdAsync(productMapping.StoreId.Value) is { Name: var name, Id: var id }
                ? $"Store ID: {id}, Name: {name}"
                : "Store Name: Default";
            foreach (var storeId in model.DestinationStoreIds)
            {
                await _productMapping.DuplicateAsync(productMapping, storeId);
                var newStore = await _store.GetStoreByIdAsync(storeId);
                var comment = $"Duplicated NexPort product newMapping from ({storeText}, "
                    + $"NexPort product ID: {productMapping.CatalogSyllabusLinkId}, "
                    + $"Name: {productMapping.ProductName}) for Store (ID: {newStore.Id}, "
                    + $"Name: {newStore.Name}) "
                    + $"in Product (ID: {nopProduct.Id}, Name: {nopProduct.Name}) ";
                await _customerActivity.InsertActivityAsync(DUPLICATE_PRODUCT_MAPPING, comment, productMapping);
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync($"Unable to duplicate NexPort product mapping for product {product.Id} based on store", exception);
        }
        ViewBag.RefreshPage = true;
        ViewBag.ClosePage = true;
        model = await _model.DuplicateProductMappingModel(product);
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}Product/ProductMapping/DuplicateProductMapping.cshtml", model);
    }
}
