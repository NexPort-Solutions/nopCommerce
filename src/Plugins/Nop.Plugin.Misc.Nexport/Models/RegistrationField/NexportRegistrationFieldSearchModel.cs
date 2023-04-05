using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField
{
    public record NexportRegistrationFieldSearchModel : BaseSearchModel
    {
        public NexportRegistrationFieldSearchModel()
        {
            AvailableStores = new List<SelectListItem>();

            SetGridPageSize();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationFields.SearchRegistrationFieldName")]
        public string SearchRegistrationFieldName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SearchStoreId")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}
