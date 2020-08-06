using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Archway.Models
{
    public class ArchwayStudentEmployeeRegistrationFieldModel : BaseNopModel
    {
        public ArchwayStudentEmployeeRegistrationFieldModel()
        {
            AvailableStates = new List<SelectListItem>();
            AvailableCities = new List<SelectListItem>();
            AvailableAddresses = new List<SelectListItem>();
            AvailableEmployeePositions = new List<SelectListItem>();
        }

        public int FieldId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.StoreLocationState")]
        public string StoreLocationState { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity")]
        public string StoreLocationCity { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress")]
        public string StoreLocationAddress { get; set; }

        public int StoreNumber { get; set; }

        public string StoreType { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.EmployeePosition")]
        public string EmployeePosition { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.EmployeeId")]
        public string EmployeeId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }

        public IList<SelectListItem> AvailableCities { get; set; }

        public IList<SelectListItem> AvailableAddresses { get; set; }

        public IList<SelectListItem> AvailableEmployeePositions { get; set; }
    }
}
