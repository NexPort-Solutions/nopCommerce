using System;
using Nop.Core;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportGroupProductModel : BaseNopEntityModel
{
    public string Name { get; set; }

    public string GroupName { get; set; }

    public string GroupShortName { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public int Available { get; set; }

    public int Awaiting { get; set; }

    public int ProcessingAvailable { get; set; }

    public int ProcessingAwaiting { get; set; }

    public int Refunded { get; set; }

    public int ProcessingRefund { get; set; }

    public int Total { get; set; }

    public int Redeemed { get; set; }

    public Guid? GroupId { get; set; }

    public int? FundingPoolId { get; set; }

    public string FundingPool { get; set; }
}