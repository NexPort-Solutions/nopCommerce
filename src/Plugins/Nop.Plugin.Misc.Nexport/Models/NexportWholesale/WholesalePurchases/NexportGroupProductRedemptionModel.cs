using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportGroupProductRedemptionModel : BaseNopEntityModel
{
    public int? CustomerId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public int? ProductId { get; set; }

    public string ProductName { get; set; }

    public int? AssignedProductId { get; set; }

    public string AssignedProductName { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = "Available";

    public DateTime DatePurchased { get; set; }

    public DateTime? DateRedeemed { get; set; }

    public Guid InvoiceItemId { get; set; }

    public int? PurchaseByCustomerId { get; set; }

    public string PurchasedByCustomerName { get; set; }

    public string PurchasedByCustomerEmail { get; set; }

    public string PurchasedInStore { get; set; }

    public string AssignedInStore { get; set; }

    public bool HasRefundRequest { get; set; }

    public int? RefundRequestId { get; set; }

    public string RefundNote { get; set; }

    public bool HasRecentUnassignmentRequest { get; set; }
}