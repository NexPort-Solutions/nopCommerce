using System;
using System.Linq;
using System.Reflection;
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
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Plugin.Misc.Nexport.Services;

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

        #endregion

        #region Constructors

        public ArchwayEmployeeRegistrationFieldController(
            IArchwayStudentEmployeeRegistrationFieldModelFactory archwayStudentEmployeeRegistrationFieldModelFactory,
            IArchwayStudentEmployeeRegistrationFieldService archwayStudentEmployeeRegistrationFieldService,
            NexportService nexportService,
            IPermissionService permissionService,
            INotificationService notificationService,
            INopFileProvider fileProvider,
            ILocalizationService localizationService)
        {
            _archwayStudentEmployeeRegistrationFieldModelFactory = archwayStudentEmployeeRegistrationFieldModelFactory;
            _archwayStudentEmployeeRegistrationFieldService = archwayStudentEmployeeRegistrationFieldService;
            _nexportService = nexportService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Configuration Actions

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(int fieldId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return Content("Access denied");

            var model = _archwayStudentEmployeeRegistrationFieldModelFactory.PrepareArchwayStudentEmployeeRegistrationFieldOptionModel(fieldId);

            return View("~/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/Customer/CustomRegistrationFieldDetails.cshtml", model);
        }

        public IActionResult CustomRender(int fieldId)
        {
            var model = _archwayStudentEmployeeRegistrationFieldModelFactory.PrepareArchwayStudentEmployeeRegistrationFieldModel(fieldId);

            ViewData.TemplateInfo.HtmlFieldPrefix = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HtmlFieldPrefix}";

            return PartialView("~/Plugins/Misc.Nexport.Archway/Views/RegistrationField/_CustomerFields.Control.CustomRender.cshtml", model);
        }

        [Area(AreaNames.Admin)]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Route("Admin/NexportIntegration/EditRegistrationField/{id}")]
        [HttpPost, ActionName("EditRegistrationField")]
        [FormValueRequired("savecustomregistrationfield_archway")]
        public IActionResult SaveCustomFieldOption(ArchwayStudentEmployeeRegistrationFieldOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var registrationField = _nexportService.GetNexportRegistrationFieldById(model.FieldId);
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
                            _archwayStudentEmployeeRegistrationFieldService
                                .InsertOrUpdateArchwayStudentRegistrationFieldKeyMapping(
                                    new ArchwayStudentRegistrationFieldKeyMapping
                                    {
                                        FieldControlName = propAttribute.ControlName,
                                        FieldKey = prop.GetValue(model, null)?.ToString()
                                    });
                        }
                    }
                }

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Customers.Nexport.RegistrationField.Fields.Updated"));

                ViewBag.RefreshPage = true;

                return RedirectToAction("EditRegistrationField", "NexportIntegration", new { id = registrationField.Id });
            }

            return RedirectToAction("EditRegistrationField", "NexportIntegration", new { id = registrationField.Id });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpsRequirement(SslRequirement.Yes)]
        [AdminAntiForgery(true)]
        [HttpPost]
        public IActionResult AsyncUploadStoreData()
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
                var fileResult = _archwayStudentEmployeeRegistrationFieldService.SaveUploadedStoreDataFile(httpPostedFile);
                _archwayStudentEmployeeRegistrationFieldService.ProcessUploadedStoreDataFile(fileResult);

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

        #endregion

        #region Custom Render View Actions

        [CheckAccessPublicStore(true)]
        public IActionResult GetArchwayStoreCitiesByState(string state, bool addSelectCityItem)
        {
            var model = _archwayStudentEmployeeRegistrationFieldModelFactory.GetArchwayStoreCitiesByState(state, addSelectCityItem);
            return Json(model);
        }

        [CheckAccessPublicStore(true)]
        public IActionResult GetArchwayStoreAddressesByCity(string city, bool addSelectAddressItem)
        {
            var model = _archwayStudentEmployeeRegistrationFieldModelFactory.GetArchwayStoreAddressesByCity(city, addSelectAddressItem);
            return Json(model);
        }

        [CheckAccessPublicStore(true)]
        public IActionResult GetArchwayStoreEmployeePositionsByStore(string storeNumber, bool addSelectPositionItem)
        {
            var model = _archwayStudentEmployeeRegistrationFieldModelFactory.GetArchwayStoreEmployeePositionsByStore(storeNumber, addSelectPositionItem);
            return Json(model);
        }

        #endregion
    }
}
