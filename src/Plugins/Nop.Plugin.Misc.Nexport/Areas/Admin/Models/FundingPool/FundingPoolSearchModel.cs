namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;

public record FundingPoolSearchModel : Nop.Web.Framework.Models.BaseSearchModel
{
    public string? Name { get; init; }
    public string? Code { get; init; }
    public string? Description { get; init; }
}
