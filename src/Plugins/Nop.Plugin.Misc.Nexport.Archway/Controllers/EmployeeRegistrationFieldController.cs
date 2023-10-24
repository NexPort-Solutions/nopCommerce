using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Archway.Factories;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Archway.Controllers;

public class EmployeeRegistrationFieldController : BasePluginController
{
    #region Fields

    private readonly IStudentEmployeeRegistrationFieldModelFactory _studentEmployeeRegistrationFieldModelFactory;
    private readonly IRegistrationFieldService _registrationField;

    #endregion Fields

    #region Constructors

    public EmployeeRegistrationFieldController(
        IStudentEmployeeRegistrationFieldModelFactory studentEmployeeRegistrationFieldModelFactory,
        IRegistrationFieldService registrationField)
    {
        _studentEmployeeRegistrationFieldModelFactory = studentEmployeeRegistrationFieldModelFactory;
        _registrationField = registrationField;
    }

    #endregion Constructors

    public async Task<ViewComponentResult> CustomRender(int fieldId)
    {
        if (await _registrationField.GetById(fieldId) is not { IsRequired: var isRequired })
        {
            return new ViewComponentResult();
        }
        ViewData.TemplateInfo.HtmlFieldPrefix = $"{Defaults.RegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HTML_FIELD_PREFIX}";
        return ViewComponent(typeof(Components.CustomerFields.Control.CustomRender), new { renderAdminView = false, isRequired });
    }

    [CheckAccessPublicStore(true)]
    public async Task<JsonResult> GetStoreCitiesByState(string state)
        => Json(await _studentEmployeeRegistrationFieldModelFactory.GetStoreCities(state));

    [CheckAccessPublicStore(true)]
    public async Task<JsonResult> GetStoreAddressesByCity(string city, string state)
        => Json(await _studentEmployeeRegistrationFieldModelFactory.GetStoreAddresses(city, state));

    [CheckAccessPublicStore(true)]
    public async Task<JsonResult> GetStoreEmployeePositionsByStore(int storeNumber)
        => Json(await _studentEmployeeRegistrationFieldModelFactory.GetStoreEmployeePositions(storeNumber));
}
