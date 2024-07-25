using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;

public record NexportFundingPoolSearchModel : BaseSearchModel
{
    public string Name { get; init; }

    public string Code { get; init; }

    public string Description { get; init; }
}