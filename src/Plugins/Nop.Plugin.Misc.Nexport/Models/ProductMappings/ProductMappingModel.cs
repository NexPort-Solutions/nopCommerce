using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using static NexportApi.Model.Enums;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings;

public record ProductMappingModel : BaseNopEntityModel, IProductMapping
{
    public required int NopProductId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.ProductName")]
    public string? ProductName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.DisplayName")]
    public string? DisplayName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.CatalogSyllabusLinkId")]
    public Guid? CatalogSyllabusLinkId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.CatalogId")]
    public Guid? CatalogId { get; set; }

    public string? CatalogName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SyllabusId")]
    public Guid? SyllabusId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgId")]
    public Guid? SubscriptionOrgId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgName")]
    public string? SubscriptionOrgName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgShortName")]
    public string? SubscriptionOrgShortName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Type")]
    public ProductType Type { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.PublishingModel")]
    public PublishingModelEnum? PublishingModel { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.PricingModel")]
    public PricingModelEnum? PricingModel { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.ModifiedDate")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcLastModifiedDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AvailableDate")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcAvailableDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.EndDate")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcEndDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.LastSynchronizationDate")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcLastSynchronizationDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.UtcAccessExpirationDate")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcAccessExpirationDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AccessTimeLimit")]
    public string? AccessTimeLimit { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.CreditHours")]
    public decimal? CreditHours { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.UniqueName")]
    public string? UniqueName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SectionCeus")]
    public string? SectionCeus { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SectionNumber")]
    public string? SectionNumber { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.IsSynchronized")]
    public bool IsSynchronized { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AutoRedeem")]
    public bool AutoRedeem { get; set; }

    public int? StoreId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.StoreMapping")]
    public string? StoreName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AllowExtension")]
    public bool AllowExtension { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.IsExtensionProduct")]
    public bool IsExtensionProduct { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RenewalWindow")]
    public string? RenewalWindow { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RenewalDuration")]
    public string? RenewalDuration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RenewalCompletionThreshold")]
    [UIHint("Int32Nullable")]
    public int? RenewalCompletionThreshold { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RenewalApprovalMethod")]
    public EnrollmentRenewalApprovalMethod? RenewalApprovalMethod { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.ExtensionPurchaseLimit")]
    [UIHint("Int32Nullable")]
    public int? ExtensionPurchaseLimit { get; set; }

    public ProductGroupMembershipMappingModel? AddGroupMembershipMappingModel { get; set; }

    public IList<ProductGroupMembershipMappingModel> GroupMembershipMappingModels { get; init; } = new List<ProductGroupMembershipMappingModel>();

    public IList<int> SupplementalInfoQuestionIds { get; set; } = new List<int>();

    public IList<SelectListItem> AvailableSupplementalInfoQuestions { get; set; } = new List<SelectListItem>();

    public bool Editable { get; set; }
}
