using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale;

public record GroupListSearchModel : BaseSearchModel
{
    public bool AdminView { get; set; }

    public GroupListSearchModel() => SetGridPageSize();
}
