using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Archway.Extensions;
using Nop.Plugin.Misc.Nexport.Archway.Factories;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Nexport.Archway.Areas.Admin.Controllers;

public class EmployeeRegistrationFieldController : BaseAdminController
{
    private readonly INexportService _nexportService;
    private readonly IStudentEmployeeRegistrationFieldModelFactory _studentEmployeeRegistrationFieldModelFactory;
    private readonly IUploadedStoreDataFileService _uploadedStoreDataFileService;
    private readonly IStudentRegistrationFieldKeyMappingService _studentRegistrationFieldKeyMappingService;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IRegistrationFieldService _registrationField;

    public EmployeeRegistrationFieldController(
        IUploadedStoreDataFileService uploadedStoreDataFileService,
        IStudentRegistrationFieldKeyMappingService studentRegistrationFieldKeyMappingService,
        INexportService nexportService,
        IPermissionService permissionService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IStudentEmployeeRegistrationFieldModelFactory studentEmployeeRegistrationFieldModelFactory,
        IRegistrationFieldService registrationField)
    {
        _uploadedStoreDataFileService = uploadedStoreDataFileService;
        _studentRegistrationFieldKeyMappingService = studentRegistrationFieldKeyMappingService;
        _nexportService = nexportService;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _studentEmployeeRegistrationFieldModelFactory = studentEmployeeRegistrationFieldModelFactory;
        _registrationField = registrationField;
    }

    public async Task<ActionResult> Configure(int fieldId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return Content("Access denied");
        }
        var model = await _studentEmployeeRegistrationFieldModelFactory.StudentEmployeeRegistrationFieldOptionModel(fieldId);
        return View(model);
    }

    [HttpPost("{id}")]
    [FormValueRequired("savecustomregistrationfield_archway")]
    public async Task<IActionResult> SaveCustomFieldOption(StudentEmployeeRegistrationFieldOptionModel model, int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
        {
            return AccessDeniedView();
        }
        if (await _registrationField.GetById(id) is null)
        {
            return RedirectToAction(nameof(RegistrationFieldController.ListRegistrationField), ControllerUtilities.GetControllerName<RegistrationFieldController>());
        }
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(RegistrationFieldController.EditRegistrationField), ControllerUtilities.GetControllerName<RegistrationFieldController>(), new { id });
        }
        if (model.StoreStateFieldKey is { } storeState)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.StoreLocationState), storeState);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        if (model.StoreStateFieldKey is { } storeCity)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.StoreLocationCity), storeCity);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        if (model.StoreStateFieldKey is { } storeAddress)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.StoreLocationAddress), storeAddress);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        if (model.StoreStateFieldKey is { } storeId)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.StoreNumber), storeId);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        if (model.StoreStateFieldKey is { } storeType)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.StoreType), storeType);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        if (model.StoreStateFieldKey is { } employeeId)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.EmployeeId), employeeId);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        if (model.StoreStateFieldKey is { } employeePosition)
        {
            var fieldKeyMapping = new StudentRegistrationFieldKeyMapping(nameof(StudentEmployeeRegistrationFieldModel.EmployeePosition), employeePosition);
            await _studentRegistrationFieldKeyMappingService.InsertOrUpdate(fieldKeyMapping);
        }
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Updated"));
        ViewBag.RefreshPage = true;
        return RedirectToAction(nameof(RegistrationFieldController.EditRegistrationField), ControllerUtilities.GetControllerName<IntegrationController>(), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AsyncUploadStoreData()
    {
        if (Request.Form.Files is not [var httpPostedFile, ..])
        {
            var result = new
            {
                success = false,
                message = "No file uploaded",
            };
            return ErrorJson(result);
        }
        switch (await _uploadedStoreDataFileService.Save(httpPostedFile))
        {
            case Err<string> { Error: var error }:
                return ErrorJson(new { success = false, message = error });
            case Ok<string> { Okay: var ok }:
            {
                _ = _uploadedStoreDataFileService.Process(ok);
                var result = new
                {
                    success = true,
                    message = "Save data file successfully. Store data information is being processed.",
                };
                return Json(result);
            }
            default:
                return Absurd();
        }
    }

    public IActionResult EditCustomerRegistrationFieldAnswers(int customerId, int fieldId)
    {
        var model = _studentEmployeeRegistrationFieldModelFactory.EditStudentEmployeeRegistrationFieldModel(customerId, fieldId);
        ViewData.TemplateInfo.HtmlFieldPrefix = $"{Defaults.RegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HTML_FIELD_PREFIX}";
        return PartialView("~/Plugins/Misc.Nexport.Archway/Views/RegistrationField/EditCustomerRegistrationFieldAnswers.cshtml", model);
    }

    public async Task<ViewComponentResult> CustomRender(int fieldId)
    {
        if (await _registrationField.GetById(fieldId) is not { IsRequired: var isRequired })
        {
            return new ViewComponentResult();
        }
        // todo BB check
        ViewData.TemplateInfo.HtmlFieldPrefix = $"{Defaults.RegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HTML_FIELD_PREFIX}";
        return ViewComponent(typeof(Components.CustomerFields.Control.CustomRender), new { renderAdminView = true, isRequired });
    }
}
