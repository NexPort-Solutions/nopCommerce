using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Stores
{
    public record NexportStoreSearchModel : BaseSearchModel
    {

        public NexportStoreSearchModel()
        {
            SetGridPageSize();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Stores.SearchStoreName")]
        public string SearchStoreName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Stores.SearchStoreUrl")]
        public string SearchStoreUrl { get; set; }
    }
}
