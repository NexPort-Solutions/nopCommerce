using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record MappingProductModel : ProductModel
{
    public bool HasMapping { get; set; }
}
