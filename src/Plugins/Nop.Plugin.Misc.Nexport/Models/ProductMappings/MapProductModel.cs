using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record MapProductModel : BaseNopModel
{
    public int NopProductId { get; init; }

    /// <summary>
    /// This is either CatalogId or the CatalogLinkId
    /// </summary>
    public required Guid ProductId { get; init; }
    public required Guid CatalogId { get; init; }
    public Guid? SyllabusId { get; init; }
    public required ProductType ProductType { get; init; }
    public int? StoreId { get; init; }
}
