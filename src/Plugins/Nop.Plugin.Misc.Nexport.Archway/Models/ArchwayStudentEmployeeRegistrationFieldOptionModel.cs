using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Archway.Extensions;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Archway.Models
{
    public class ArchwayStudentEmployeeRegistrationFieldOptionModel : BaseNopModel
    {
        [FromForm(Name = "Id")]
        public int FieldId { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.StoreLocationState))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey")]
        public string StoreStateFieldKey { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.StoreLocationCity))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey")]
        public string StoreCityFieldKey { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.StoreLocationAddress))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey")]
        public string StoreAddressFieldKey { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.StoreNumber))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey")]
        public string StoreIdFieldKey { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.StoreType))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey")]
        public string StoreTypeFieldKey { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.EmployeeId))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey")]
        public string EmployeeIdFieldKey { get; set; }

        [ArchwayStudentRegistrationFieldControl(nameof(ArchwayStudentEmployeeRegistrationFieldModel.EmployeePosition))]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey")]
        public string EmployeePositionFieldKey { get; set; }
    }
}