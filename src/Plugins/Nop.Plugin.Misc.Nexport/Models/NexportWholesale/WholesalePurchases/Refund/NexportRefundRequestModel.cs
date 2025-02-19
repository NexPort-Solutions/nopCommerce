using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.Refund;

public record NexportRefundRequestModel : BaseNopEntityModel
{
    public int CustomerId { get; set; }

    public int ReturnRequestId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public int OrderId { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; }

    public DateTime UtcCreatedDate { get; set; }

    public int LastModifiedUserId { get; set; }

    public string LastModifiedUserInfo { get; set; }

    public bool IsNexportPurchase { get; set; }
}
