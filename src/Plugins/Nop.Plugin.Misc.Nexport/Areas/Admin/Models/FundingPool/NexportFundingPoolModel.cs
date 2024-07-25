using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;

public record NexportFundingPoolModel : BaseNopEntityModel
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Code { get; set; }

    public DateTime UtcDateCreated { get; set; }

    public DateTime? UtcDateModified { get; set; }
}