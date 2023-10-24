using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale;

public record GroupProductRedemptionListSearchModel : BaseSearchModel
{
    public GroupModel? CurrentGroup { get; set; }
    public Product? CurrentProduct { get; set; }
    public bool AdminView { get; set; }
    public GroupProductRedemptionListSearchModel() => SetGridPageSize();
}
