using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models.Order;

public class NexportUpdateInvoiceFields
{
    public Guid? ProductId { get; set; }

    public Enums.ProductTypeEnum ProductType { get; set; } = Enums.ProductTypeEnum.Syllabus;

    public string ProductCode { get; set; }

    public bool IsRenewal { get; set; } = false;

    public decimal? Cost { get; set; }

    public DateTime? AccessExpirationDate { get; set; }

    public string AccessExpirationTimeLimit { get; set; }

    public Guid? SubscriptionOrgId { get; set; }

    public List<Guid> GroupMembershipIds { get; set; }

    public Guid? PurchasingGroupId { get; set; }

    public string FundingPool { get; set; }

    public DateTime? RedemptionAvailableDate { get; set; }

    public string Note { get; set; }
}
