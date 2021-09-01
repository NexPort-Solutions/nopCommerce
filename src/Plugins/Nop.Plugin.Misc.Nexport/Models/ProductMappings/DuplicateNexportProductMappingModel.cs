using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings
{
    public class DuplicateNexportProductMappingModel : BaseNopModel
    {
        public DuplicateNexportProductMappingModel()
        {
            AvailableStores = new List<SelectListItem>();
            DestinationStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.DuplicateSourceStoreMapping")]
        public int? SourceStoreId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings")]
        public IList<int> DestinationStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> DestinationStores { get; set; }
    }
}