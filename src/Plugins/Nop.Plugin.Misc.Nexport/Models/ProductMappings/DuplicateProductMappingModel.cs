using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record DuplicateProductMappingModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.DuplicateSourceStoreMapping")]
    public int? SourceStoreId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.DuplicateDestinationStoreMappings")]
    public IList<int> DestinationStoreIds { get; init; } = new List<int>();
    public IList<SelectListItem> AvailableStores { get; init; } = new List<SelectListItem>();
    public IList<SelectListItem> DestinationStores { get; init; } = new List<SelectListItem>();
}
