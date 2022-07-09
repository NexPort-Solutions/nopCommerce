using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings
{
    public record DuplicateNexportProductMappingModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.DuplicateSourceStoreMapping")]
        public int? SourceStoreId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings")]
        public IList<int> DestinationStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();

        public IList<SelectListItem> DestinationStores { get; set; } = new List<SelectListItem>();
    }
}