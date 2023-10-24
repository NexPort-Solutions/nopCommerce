using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record ProductMappingListSearchModel : BaseSearchModel
{
    public ProductMappingListSearchModel() => SetGridPageSize();

    public Guid? ProductId { get; set; }
    public Guid? CatalogId { get; set; }
    public Guid? SyllabusId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.ProductMapping.SearchProductType")]
    public Domain.ProductType? ProductType { get; set; }

    public int? NopProductId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.ProductMapping.SearchProductName")]
    public string? SearchProductName { get; set; }

    public IList<SelectListItem> AvailableProductTypes { get; init; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Misc.Nexport.ProductMapping.SearchStoreName")]
    public string? SearchStoreName { get; set; }
}
