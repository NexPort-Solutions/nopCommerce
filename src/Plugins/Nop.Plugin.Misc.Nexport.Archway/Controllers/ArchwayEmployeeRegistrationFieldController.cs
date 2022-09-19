using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Archway.Extensions;
using Nop.Plugin.Misc.Nexport.Archway.Factories;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Plugin.Misc.Nexport.Archway.Models.Plugins;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.Nexport.Archway.Controllers
{
    public class ArchwayEmployeeRegistrationFieldController : BasePluginController
    {
        #region Fields

        private readonly NexportService _nexportService;
        private readonly IArchwayStudentEmployeeRegistrationFieldModelFactory _archwayStudentEmployeeRegistrationFieldModelFactory;
        private readonly IArchwayStudentEmployeeRegistrationFieldService _archwayStudentEmployeeRegistrationFieldService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Constructors

        public ArchwayEmployeeRegistrationFieldController(
            IArchwayStudentEmployeeRegistrationFieldModelFactory archwayStudentEmployeeRegistrationFieldModelFactory,
            IArchwayStudentEmployeeRegistrationFieldService archwayStudentEmployeeRegistrationFieldService,
            NexportService nexportService,
            IPermissionService permissionService,
            INotificationService notificationService,
            INopFileProvider fileProvider,
            ILocalizationService localizationService,
            ISettingService settingService)
        {
            _archwayStudentEmployeeRegistrationFieldModelFactory = archwayStudentEmployeeRegistrationFieldModelFactory;
            _archwayStudentEmployeeRegistrationFieldService = archwayStudentEmployeeRegistrationFieldService;
            _nexportService = nexportService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        #endregion

        #region General Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetModifiedLocaleResources(ArchwayPluginResourceListSearchModel searchModel, string friendlyName)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return await AccessDeniedDataTablesJson();

            var model = await _archwayStudentEmployeeRegistrationFieldModelFactory
                .PrepareArchwayPluginResourceListModelAsync(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> OverrideResources(ICollection<int> selectedIds, bool allChecked)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (selectedIds != null && selectedIds.Count != 0)
            {
                foreach (var id in selectedIds)
                {
                    var localeStringResourceById = await _localizationService.GetLocaleStringResourceByIdAsync(id);

                    if (localeStringResourceById != null)
                    {
                        var archwayLocaleResources = ArchwayPluginService.GetLocaleResource();
                        var overridingResourceValue = archwayLocaleResources.Where(l => l.Key.ToLower() == localeStringResourceById.ResourceName.ToLower())
                            .Select(l => l.Value)
                            .First();

                        if (overridingResourceValue != null)
                        {
                            localeStringResourceById.ResourceValue = overridingResourceValue;

                            await _localizationService.UpdateLocaleStringResourceAsync(localeStringResourceById);
                        }
                    }
                }
                if (allChecked)
                {
                    var archwaySetting = await _settingService.GetSettingAsync("Plugin.Misc.Nexport.Archway.HasModifiedLocaleResources");

                    if (archwaySetting != null)
                    {
                        await _settingService.DeleteSettingAsync(archwaySetting);
                    }
                }
            }
            else
            {
                return NoContent();
            }

            return Json(new { success = true });
        }

        #endregion

        #region Configuration Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(int fieldId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return Content("Access denied");

            var model = await _archwayStudentEmployeeRegistrationFieldModelFactory.PrepareArchwayStudentEmployeeRegistrationFieldOptionModelAsync(fieldId);

            return View("~/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/Customer/CustomRegistrationFieldDetails.cshtml", model);
        }

        public async Task<IActionResult> CustomRender(int fieldId, bool renderAdminView)
        {
            var model = await _archwayStudentEmployeeRegistrationFieldModelFactory.PrepareArchwayStudentEmployeeRegistrationFieldModelAsync(fieldId, renderAdminView);

            ViewData.TemplateInfo.HtmlFieldPrefix = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HtmlFieldPrefix}";

            return PartialView("~/Plugins/Misc.Nexport.Archway/Views/RegistrationField/_CustomerFields.Control.CustomRender.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AutoValidateAntiforgeryToken]
        [Route("Admin/NexportIntegration/EditRegistrationField/{id}")]
        [HttpPost, ActionName("EditRegistrationField")]
        [FormValueRequired("savecustomregistrationfield_archway")]
        public async Task<IActionResult> SaveCustomFieldOption(ArchwayStudentEmployeeRegistrationFieldOptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = await _nexportService.GetNexportRegistrationFieldById(model.FieldId);
            if (registrationField == null)
                return RedirectToAction("ListRegistrationField", "NexportIntegration");

            if (ModelState.IsValid)
            {
                foreach (var prop in model.GetType().GetProperties())
                {
                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (type == typeof(string))
                    {
                        var propAttribute = prop.GetCustomAttribute<ArchwayStudentRegistrationFieldControlAttribute>();
                        if (propAttribute != null)
                        {
                            await _archwayStudentEmployeeRegistrationFieldService
                                .InsertOrUpdateArchwayStudentRegistrationFieldKeyMapping(
                                    new ArchwayStudentRegistrationFieldKeyMapping
                                    {
                                        FieldControlName = propAttribute.ControlName,
                                        FieldKey = prop.GetValue(model, null)?.ToString()
                                    });
                        }
                    }
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Nexport.RegistrationField.Fields.Updated"));

                ViewBag.RefreshPage = true;

                return RedirectToAction("EditRegistrationField", "NexportIntegration", new { id = registrationField.Id });
            }

            return RedirectToAction("EditRegistrationField", "NexportIntegration", new { id = registrationField.Id });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpsRequirement]
        [HttpPost]
        public async Task<IActionResult> AsyncUploadStoreData()
        {
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded"
                });
            }

            try
            {
                var fileResult = await _archwayStudentEmployeeRegistrationFieldService.SaveUploadedStoreDataFile(httpPostedFile);
                await _archwayStudentEmployeeRegistrationFieldService.ProcessUploadedStoreDataFile(fileResult);

                return Json(new
                {
                    success = true,
                    message = "Save data file successfully. Store data information is being processed."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Fail to save data file!",
                    exception = $"{ex.InnerException?.Message}"
                });
            }
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult EditCustomerRegistrationFieldAnswers(int customerId, int fieldId)
        {
            var model = _archwayStudentEmployeeRegistrationFieldModelFactory.PrepareEditArchwayStudentEmployeeRegistrationFieldModel(customerId, fieldId);

            ViewData.TemplateInfo.HtmlFieldPrefix = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HtmlFieldPrefix}";

            return PartialView("~/Plugins/Misc.Nexport.Archway/Views/RegistrationField/EditCustomerRegistrationFieldAnswers.cshtml", model);
        }

        #endregion

        #region Custom Render View Actions

        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> GetArchwayStoreCitiesByState(string state, bool addSelectCityItem)
        {
            var model = await _archwayStudentEmployeeRegistrationFieldModelFactory.GetArchwayStoreCitiesByState(state, addSelectCityItem);
            return Json(model);
        }

        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> GetArchwayStoreAddressesByCity(string city, string state, bool addSelectAddressItem)
        {
            var model = await _archwayStudentEmployeeRegistrationFieldModelFactory.GetArchwayStoreAddressesByCity(city, state, addSelectAddressItem);
            return Json(model);
        }

        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> GetArchwayStoreEmployeePositionsByStore(string storeNumber, bool addSelectPositionItem)
        {
            var model = await _archwayStudentEmployeeRegistrationFieldModelFactory.GetArchwayStoreEmployeePositionsByStore(storeNumber, addSelectPositionItem);
            return Json(model);
        }

        #endregion
    }
}
