using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Controllers;
using IStoreService = Nop.Plugin.Misc.Nexport.Services.IStoreService;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using static Nop.Plugin.Misc.Nexport.Defaults;
using Type = Nop.Plugin.Misc.Nexport.Domain.RegistrationField.RegistrationField.RegistrationFieldType;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
public class RegistrationFieldController : BaseAdminController
{
    private readonly Factories.IPluginModelFactory _model;
    private readonly IStoreService _store;
    private readonly IRegistrationFieldService _registrationField;
    private readonly ICustomerService _customer;
    private readonly IPermissionService _permission;
    private readonly ILocalizationService _localization;
    private readonly ILocalizedEntityService _localizedEntity;
    private readonly INotificationService _notification;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IPluginManager<IRegistrationFieldCustomRender> _customRenderPluginManager;
    private readonly ILogger _logger;

    public RegistrationFieldController(
        Factories.IPluginModelFactory pluginModelFactory,
        IStoreService storeService,
        ICustomerService customerService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IGenericAttributeService genericAttributeService,
        IPluginManager<IRegistrationFieldCustomRender> customRenderPluginManager,
        ILogger logger,
        IRegistrationFieldService registrationField)
    {
        _model = pluginModelFactory;
        _store = storeService;
        _customer = customerService;
        _permission = permissionService;
        _localization = localizationService;
        _localizedEntity = localizedEntityService;
        _notification = notificationService;
        _genericAttribute = genericAttributeService;
        _customRenderPluginManager = customRenderPluginManager;
        _logger = logger;
        _registrationField = registrationField;
    }

    [HttpGet]
    public async Task<IActionResult> ListCategory()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        SaveSelectedTabName("customersettings-nexportregistrationfields");
        return RedirectToAction("CustomerUser", "Setting");
    }

    [HttpPost]
    public async Task<IActionResult> ListCategory(CategorySearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.RegistrationField.CategoryList(model));
    }

    [HttpGet]
    public async Task<IActionResult> CreateCategory()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Category/Create.cshtml", new CategoryModel());
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public async Task<IActionResult> CreateCategory(CategoryModel model, bool continueEditing = false)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (ModelState.IsValid)
        {
            var registrationFieldCategory = model.ToEntity<Domain.RegistrationField.Category>();
            await _registrationField.InsertCategory(registrationFieldCategory);
            _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Added"));
            return continueEditing
                ? RedirectToAction(nameof(EditCategory), new { id = registrationFieldCategory.Id })
                : RedirectToAction(nameof(ListCategory));
        }
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Category/Create.cshtml", new CategoryModel());
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetCategoryById(id) is not { } registrationFieldCategory)
        {
            return RedirectToAction(nameof(ListCategory));
        }
        var model = await _model.RegistrationField.Category(registrationFieldCategory);
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Category/Edit.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public virtual async Task<IActionResult> EditCategory(CategoryModel model, bool continueEditing)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetCategoryById(model.Id) is not { } registrationFieldCategory)
        {
            return RedirectToAction(nameof(ListCategory));
        }
        if (!ModelState.IsValid)
        {
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Category/Edit.cshtml", model);
        }
        registrationFieldCategory = model.ToEntity(registrationFieldCategory);
        await _registrationField.UpdateCategory(registrationFieldCategory);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Updated"));
        return continueEditing
            ? RedirectToAction(nameof(EditCategory), new { id = registrationFieldCategory.Id })
            : RedirectToAction(nameof(ListCategory));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteCategory(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetCategoryById(id) is not { } registrationFieldCategory)
        {
            return BadRequest($"Could not find registration field category with id {id}.");
        }
        await _registrationField.DeleteCategory(registrationFieldCategory);
        var registrationFieldsWithCategory = await _registrationField.GetAllByCategoryId(id);
        foreach (var field in registrationFieldsWithCategory)
        {
            field.FieldCategoryId = null;
            await _registrationField.Update(field);
        }
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Categories.Deleted"));
        return RedirectToAction(nameof(ListCategory));
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        SaveSelectedTabName("customersettings-nexportregistrationfields");
        return RedirectToAction(nameof(SettingController.CustomerUser), ViewUtilities.GetControllerName<SettingController>());
    }

    [HttpPost]
    public async Task<IActionResult> List(SearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.RegistrationField.List(model));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Create.cshtml", new RegistrationFieldModel());
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public async Task<IActionResult> Create(RegistrationFieldModel model, bool continueEditing = false)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (!ModelState.IsValid)
        {
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Create.cshtml", new RegistrationFieldModel());
        }
        var registrationField = model.ToEntity<RegistrationField>();
        await _registrationField.Insert(registrationField);
        foreach (var storeId in model.StoreMappingIds)
        {
            var storeMapping = new StoreMapping
            {
                FieldId = registrationField.Id,
                StoreId = storeId,
            };
            await _registrationField.InsertStoreMapping(storeMapping);
        }
        await UpdateLocales(registrationField, model);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Added"));
        return continueEditing
            ? RedirectToAction(nameof(Edit), new { id = registrationField.Id })
            : RedirectToAction(nameof(List));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetById(id) is not { } registrationField)
        {
            return RedirectToAction(nameof(List));
        }
        var model = await _model.RegistrationField.RegistrationFieldModel(null, registrationField);
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Edit.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public async Task<IActionResult> Edit(RegistrationFieldModel model, bool continueEditing)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetById(model.Id) is not { } registrationField)
        {
            return RedirectToAction(nameof(List));
        }
        if (!ModelState.IsValid)
        {
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Edit.cshtml", model);
        }
        if (model.Type is Type.CustomType
            && await _registrationField.HasCustomRenderForStores(registrationField.Id, model.StoreMappingIds, model.CustomFieldRender))
        {
            _notification.ErrorNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.CustomRenderStoreLimit"));
            return RedirectToAction(nameof(Edit), new { registrationField.Id });
        }
        registrationField = model.ToEntity(registrationField);
        await _registrationField.Update(registrationField);
        if (model.Type is Type.SelectCheckbox or Type.SelectDropDown)
        {
            if (model.Type is Type.SelectCheckbox)
            {
                await _genericAttribute.SaveAttributeAsync(registrationField, nameof(model.AllowMultipleSelection), model.AllowMultipleSelection);
            }
            await _genericAttribute.SaveAttributeAsync(registrationField, nameof(model.DisplayOptionByAscendingOrder), model.DisplayOptionByAscendingOrder);
        }
        var storeMappings = await _registrationField.GetStoreMappings(registrationField.Id);
        var currentStoreIds = storeMappings.ConvertAll(registrationFieldStoreMapping => registrationFieldStoreMapping.StoreId);
        var removalStoreIds = currentStoreIds.Except(model.StoreMappingIds);
        var additionalStoreIds = model.StoreMappingIds.Except(currentStoreIds);
        foreach (var storeId in additionalStoreIds)
        {
            await _registrationField.InsertStoreMapping(new StoreMapping
                {
                    FieldId = registrationField.Id,
                    StoreId = storeId,
                });
        }
        foreach (var unwantedStoreId in removalStoreIds)
        {
            if (storeMappings.Find(storeMapping => storeMapping.StoreId == unwantedStoreId) is { } unwantedMapping)
            {
                await _registrationField.DeleteStoreMapping(unwantedMapping);
            }
        }
        await UpdateLocales(registrationField, model);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Updated"));
        if (!continueEditing)
        {
            return RedirectToAction(nameof(List));
        }
        return RedirectToAction(nameof(Edit), new { id = registrationField.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetById(id) is not { } registrationField)
        {
            return BadRequest($"Registration field with id {id} not found.");
        }
        await _registrationField.Delete(registrationField);
        foreach (var fieldOption in await _registrationField.GetOptions(id))
        {
            await _registrationField.DeleteOption(fieldOption);
        }
        foreach (var fieldStoreMapping in await _registrationField.GetStoreMappings(id))
        {
            await _registrationField.DeleteStoreMapping(fieldStoreMapping);
        }
        var registrationFieldOptionSettings = await _genericAttribute.GetAttributesForEntityAsync(registrationField.Id, registrationField.GetType().Name);
        await _genericAttribute.DeleteAttributesAsync(registrationFieldOptionSettings);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Deleted"));
        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public async Task<IActionResult> ListOptions(OptionSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return await AccessDeniedDataTablesJson();
        }
        if (await _registrationField.GetById(model.RegistrationFieldId) is not { } registrationField)
        {
            return BadRequest($"Registration field with id {model.RegistrationFieldId} not found.");
        }
        return Json(await _model.RegistrationField.ListOptionsModel(model, registrationField));
    }

    [HttpGet]
    public async Task<IActionResult> CreateOption(int fieldId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetById(fieldId) is not { } registrationField)
        {
            return RedirectToAction(nameof(List));
        }
        return View(new OptionModel { FieldId = fieldId });
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public async Task<IActionResult> CreateOption(OptionModel model, bool continueEditing = false)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetById(model.FieldId) is not { } registrationField)
        {
            return RedirectToAction(nameof(List));
        }
        if (ModelState.IsValid)
        {
            var option = model.ToEntity<Option>();
            await _registrationField.InsertOption(option);
            _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Added"));
            ViewBag.RefreshPage = true;
            return View(model);
        }
        return View(new OptionModel());
    }

    [HttpGet]
    public async Task<IActionResult> EditOption(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetOptionById(id) is not { } option)
        {
            return RedirectToAction(nameof(List));
        }
        return View(option.ToModel<OptionModel>());
    }

    [HttpPost]
    public async Task<IActionResult> EditOption(OptionModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetOptionById(model.Id) is not { } option)
        {
            return BadRequest($"Registration field option with id {model.Id} not found.");
        }
        option = model.ToEntity(option);
        await _registrationField.UpdateOption(option);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteOption(int id)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetOptionById(id) is not { } option)
        {
            return BadRequest($"Registration field option with id {id} not found.");
        }
        await _registrationField.DeleteOption(option);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Options.Deleted"));
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomRenderOptionUrl(string systemName, int fieldId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _customRenderPluginManager.LoadPluginBySystemNameAsync(systemName) is not { } customRender)
        {
            return BadRequest($"Registration field custom render with for system name {systemName} not found.");
        }
        var url = customRender.GetRenderOptionUrl(fieldId);
        return Json(new { url });
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomRenderUrl(string systemName, int fieldId, bool renderAdminView)
    {
        if (await _customRenderPluginManager.LoadPluginBySystemNameAsync(systemName) is not { } registrationFieldCustomRender)
        {
            return BadRequest($"Registration field custom render with for system name {systemName} not found.");
        }
        var url = await registrationFieldCustomRender.GetCustomRenderUrl(fieldId, renderAdminView);
        return Json(new { url });
    }

    [HttpGet]
    public Task<IActionResult> GetAdminCustomRenderUrl(string systemName, int fieldId) => GetCustomRenderUrl(systemName, fieldId, true);

    [HttpGet]
    public async Task<IActionResult> LoadAnswersByStore(int storeId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _store.GetStoreByIdAsync(storeId) is not { } store)
        {
            return BadRequest($"Store with id {storeId} not found.");
        }
        var model = await _model.RegistrationField.AddCustomerAnswers(store);
        return PartialView($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Customer/_Create.Answer.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ListCustomerAnswers(ListCustomerAnswersSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.RegistrationField.ListCustomerAnswers(model));
    }

    [HttpPost]
    public async Task<IActionResult> GetAnswersForCustomer(CustomerAnswersListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.RegistrationField.CustomerWithAnswersList(model.CustomerId, model));
    }

    [HttpGet]
    public async Task<IActionResult> GetEditCustomerAnswersViewUrl(string systemName, int customerId, int fieldId)
    {
        var registrationFieldCustomRender = await _customRenderPluginManager.LoadPluginBySystemNameAsync(systemName)
            ?? throw new ArgumentException("Registration field custom render could not be loaded");
        var url = registrationFieldCustomRender.GetEditCustomerRegistrationFieldAnswersViewUrl(customerId, fieldId);
        return Json(new { url });
    }

    [HttpGet]
    public async Task<IActionResult> AddCustomerAnswers(int customerId, int storeId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return BadRequest($"No customer found with the specified id {customerId}");
        }
        if (await _store.GetStoreByIdAsync(storeId) is not { } store)
        {
            return BadRequest($"No store found with the specified id {storeId}");
        }
        var model = await _model.RegistrationField.AddCustomerAnswers(customer, store);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddCustomerAnswers(int customerId, int storeId, IFormCollection form)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return BadRequest($"No customer found with the specified id {customerId}");
        }
        if (await _store.GetStoreByIdAsync(storeId) is not { } store)
        {
            return BadRequest($"No store found with the specified id {storeId}");
        }
        if (!ModelState.IsValid)
        {
            var model = await _model.RegistrationField.AddCustomerAnswers(customer, store);
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Customer/AddCustomerAnswers.cshtml", model);
        }
        try
        {
            var registrationFields = await _registrationField.ParseMany(form, storeId);
            var customRegistrationFields = await _registrationField.Parse(form, storeId);
            await _registrationField.SaveMany(customer, registrationFields);
            foreach (var customField in customRegistrationFields)
            {
                if (await _registrationField.GetById(customField.Key) is not { } registrationField)
                {
                    continue;
                }
                if (await _customRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender) is { } customRender)
                {
                    await customRender.SaveCustoms(customer, registrationField.Id, customField.Value);
                }
            }
            await _registrationField.InsertSynchronizationQueueItem(new SynchronizationQueueItem
                {
                    CustomerId = customer.Id,
                    UtcDateCreated = DateTime.UtcNow,
                });
            ViewBag.RefreshPage = true;
            ViewBag.ClosePage = true;
            var model = await _model.RegistrationField.AddCustomerAnswers(customer, store);
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Customer/AddCustomerAnswers.cshtml", model);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Error occurred while creating NexPort registration fields", exception, customer);
            _notification.ErrorNotification("Unable to create NexPort registration fields!");
            var model = await _model.RegistrationField.AddCustomerAnswers(customer, store);
            return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Customer/AddCustomerAnswers.cshtml", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditCustomerAnswers(int customerId, int fieldId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return BadRequest($"No customer found with the specified newMapping {customerId}");
        }
        if (await _registrationField.GetById(fieldId) is not { } field)
        {
            return BadRequest($"No NexPort registration field found with the specified newMapping {fieldId}");
        }
        var model = await _model.RegistrationField.EditCustomerAnswers(customer, field);
        return View($"{NEXPORT_PLUGIN_ADMIN_VIEW_BASE_PATH}RegistrationField/Customer/EditCustomerAnswers.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditCustomerAnswers(int customerId, EditCustomerAnswersRequest editModel)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return BadRequest($"No customer found with the specified newMapping {customerId}");
        }
        if (await _registrationField.GetById(editModel.FieldId) is not { } field)
        {
            return BadRequest($"No NexPort registration field found with the specified newMapping {editModel.FieldId}");
        }
        if (!ModelState.IsValid)
        {
            var model = await _model.RegistrationField.EditCustomerAnswers(customer, field);
            return View(model);
        }
        try
        {
            if (editModel.PreviousAnswers is [var answerId, ..]
                && await _registrationField.GetAnswerById(answerId) is { } currentAnswer)
            {
                return await EditSimpleAnswer(editModel, customer, field, currentAnswer);
            }
            if (field.Type is Type.SelectCheckbox
                && editModel.AllowMultipleSelection is not null
                && editModel.AllowMultipleSelection.Value)
            {
                await EditSelectCheckbox(customerId, editModel, field);
            }
            else if (field.Type is Type.CustomType)
            {
                await EditCustomType(editModel, customer, field);
            }
            ViewBag.RefreshPage = true;
            ViewBag.ClosePage = true;
            return View(await _model.RegistrationField.EditCustomerAnswers(customer, field));
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Error occurred while saving NexPort registration field", exception, customer);
            _notification.ErrorNotification("Unable to save NexPort registration field!");
            var model = await _model.RegistrationField.EditCustomerAnswers(customer, field);
            return View(model);
        }
    }

    private async Task EditCustomType(EditCustomerAnswersRequest editModel, Customer customer, RegistrationField field)
    {
        var fields = editModel.FormCollection.Keys
            .Where(key => key.StartsWith($"CustomProfile-{field.Id}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(key => editModel.FormCollection.TryGetValue(key, out var values) ? values.FilterMap(value => value is not null ? (key, value) : default) : Array.Empty<(string, string)>())
            .ToDictionary(kvp => kvp.key, kvp => kvp.value);
        if (await _customRenderPluginManager.LoadPluginBySystemNameAsync(field.CustomFieldRender) is not { } plugin)
        {
            return;
        }
        await plugin.UpdateCustomAnswers(customer.Id, field.Id, fields);
    }

    private async Task EditSelectCheckbox(int customerId, EditCustomerAnswersRequest editModel, RegistrationField field)
    {
        var currentAnswersFieldOptions = (await _registrationField.GetAnswers(customerId, editModel.FieldId))
            .FilterMap(registrationFieldAnswer => registrationFieldAnswer.FieldOptionId)
            .ToList();
        var newAnswers = editModel.AnswerFieldOptions
            .Except(currentAnswersFieldOptions)
            .Select(newOption => new Answer
                {
                    CustomerId = customerId,
                    FieldId = editModel.FieldId,
                    UtcDateCreated = DateTime.UtcNow,
                    UtcDateModified = DateTime.UtcNow,
                    FieldOptionId = newOption,
                    IsCustomField = field.Type is Type.CustomType,
                });
        foreach (var newAnswer in newAnswers)
        {
            await _registrationField.InsertAnswer(newAnswer);
        }
        var currents = currentAnswersFieldOptions
            .Except(editModel.AnswerFieldOptions)
            .SelectAwait(async removingOption => await _registrationField.GetAnswerByFieldOption(customerId, editModel.FieldId, removingOption))
            .WhereNotNull();
        await foreach (var current in currents)
        {
            await _registrationField.DeleteAnswer(current);
        }
    }

    private async Task<IActionResult> EditSimpleAnswer(EditCustomerAnswersRequest editModel, Customer customer, RegistrationField field, Answer currentAnswer)
    {
        var newAnswer = new Answer
        {
            Id = currentAnswer.Id,
            CustomerId = customer.Id,
            FieldId = field.Id,
            TextValue = field.Type is Type.Text or Type.Email ? editModel.AnswerValue : null,
            NumericValue = field.Type is Type.Numeric && int.TryParse(editModel.AnswerValue, out var number) ? number : null,
            DateTimeValue = field.Type is Type.DateOnly or Type.DateTime && DateTime.TryParse(editModel.AnswerValue, out var date) ? date : null,
            BooleanValue = field.Type is Type.Boolean && bool.TryParse(editModel.AnswerValue, out var boolean) ? boolean : null,
            FieldOptionId = TryGetFieldOptionId(editModel, field),
            IsCustomField = field.Type is Type.CustomType,
            UtcDateCreated = currentAnswer.UtcDateCreated,
            UtcDateModified = DateTime.UtcNow,
        };
        await _registrationField.UpdateAnswer(newAnswer);
        ViewBag.RefreshPage = true;
        ViewBag.ClosePage = true;
        return View(await _model.RegistrationField.EditCustomerAnswers(customer, field));

        static int? TryGetFieldOptionId(EditCustomerAnswersRequest editModel, RegistrationField field)
            => field.Type is Type.SelectDropDown or Type.SelectCheckbox
                && editModel.AnswerFieldOptions is [var option, ..]
                && option is not default(int) ? option : null;
    }

    protected async Task UpdateLocales(RegistrationField registrationField, RegistrationFieldModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntity.SaveLocalizedValueAsync(registrationField, field => field.Name, localized.Name, localized.LanguageId);
        }
    }
}
