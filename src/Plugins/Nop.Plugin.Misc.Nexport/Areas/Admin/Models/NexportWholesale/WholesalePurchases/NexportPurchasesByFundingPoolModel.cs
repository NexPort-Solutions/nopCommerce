using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;

public record NexportPurchasesByFundingPoolModel : BaseNopModel
{
    public int Available { get; set; }

    public int Awaiting { get; set; }

    public int ProcessingAvailable { get; set; }

    public int ProcessingAwaiting { get; set; }

    public int Refunded { get; set; }

    public int ProcessingRefund { get; set; }

    public int ApprovalAwaiting { get; set; }

    public int Total { get; set; }

    public int Redeemed { get; set; }

    public int? FundingPoolId { get; set; }

    public string FundingPool { get; set; }
}