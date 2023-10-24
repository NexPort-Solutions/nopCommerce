using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Order;

public record SummaryCartFooterModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Order.GroupId")]
    public required Guid? GroupGuid { get; init; }
    public required GroupModel? Group { get; init; }
}
