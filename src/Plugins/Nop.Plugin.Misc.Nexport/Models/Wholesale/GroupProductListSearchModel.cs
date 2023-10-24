using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale;

public record GroupProductListSearchModel : BaseSearchModel
{
    public GroupModel? CurrentGroup { get; init; }
    public bool AdminView { get; init; }

    public GroupProductListSearchModel() => SetGridPageSize();
}
