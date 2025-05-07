using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale;

//public class WholesaleOrderInfo : BaseEntity
//{
//    public Guid? NexportGroupId { get; set; }

//    public int OrderId { get; set; }

//    public int OrderItemId { get; set; }

//    public int ProductId { get; set; }

//    public int Available { get; set; }

//    public int Awaiting { get; set; }

//    public int Redeemed { get; set; }

//    public int? FundingPoolId { get; set; }

//    public DateTime? UtcRedeemByDate { get; set; }
//}

public class WholesaleOrderInfo : NexportGroupProduct
{
    public int OrderId { get; set; }

    public int OrderItemId { get; set; }
}


public class NexportGroupProduct : BaseEntity
{
    public Guid? NexportGroupId { get; set; }

    public int ProductId { get; set; }

    public int Available { get; set; }

    public int Awaiting { get; set; }

    public int Redeemed { get; set; }

    public int Refunded { get; set; }

    public int ProcessingAvailable { get; set; }

    public int ProcessingAwaiting { get; set; }

    public int ProcessingRefund { get; set; }

    public int? FundingPoolId { get; set; }

    public DateTime? UtcRedeemByDate { get; set; }
}