using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Stores;

public record StoreSearchModel : BaseSearchModel
{
    public StoreSearchModel() => SetGridPageSize();

    [NopResourceDisplayName("Plugins.Misc.Nexport.Stores.SearchStoreName")]
    public required string SearchStoreName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Stores.SearchStoreUrl")]
    public required string SearchStoreUrl { get; set; }
}
