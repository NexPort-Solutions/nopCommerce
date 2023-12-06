using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;

public record FundingPoolModel(int? Id, string Name, string Code, string? Description) : BaseNopModel
{
    public static FundingPoolModel FromEntity(Domain.FundingPool entity)
        => new(entity.Id, entity.Name, entity.Code, entity.Description);
}
