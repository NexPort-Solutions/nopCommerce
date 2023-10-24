using System.ComponentModel.DataAnnotations;
using NexportApi.Model;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain;
using static NexportApi.Model.Enums;

public class ProductMapping : BaseEntity, IProductMapping
{
    public required int NopProductId { get; set; }

    [StringLength(255, MinimumLength = 1)]
    public string? ProductName { get; set; }

    [StringLength(255, MinimumLength = 1)]
    public string? DisplayName { get; set; }

    public Guid? CatalogSyllabusLinkId { get; set; }
    public Guid? CatalogId { get; set; }
    public Guid? SyllabusId { get; set; }

    /// <summary>
    /// Optional set, default to store where the product is purchased (Org Id based of the store)
    /// </summary>
    public Guid? SubscriptionOrgId { get; set; }
    public string? SubscriptionOrgName { get; set; }
    public string? SubscriptionOrgShortName { get; set; }
    public required ProductType Type { get; set; }
    public PublishingModelEnum? PublishingModel { get; set; }
    public PricingModelEnum? PricingModel { get; set; }
    public DateTime? UtcLastModifiedDate { get; set; }
    public DateTime? UtcAvailableDate { get; set; }
    public DateTime? UtcEndDate { get; set; }
    public DateTime? UtcLastSynchronizationDate { get; set; }
    public DateTime? UtcAccessExpirationDate { get; set; }
    public string? AccessTimeLimit { get; set; }
    public decimal? CreditHours { get; set; }
    public string? SectionCeus { get; set; }
    public bool IsSynchronized { get; set; }
    public bool AutoRedeem { get; set; }
    public bool AllowExtension { get; set; }
    public bool IsExtensionProduct { get; set; }
    public string? RenewalWindow { get; set; }
    public string? RenewalDuration { get; set; }
    public int? RenewalCompletionThreshold { get; set; }
    public EnrollmentRenewalApprovalMethod? RenewalApprovalMethod { get; set; }
    public int? ExtensionPurchaseLimit { get; set; }
    public int? StoreId { get; set; }
}
