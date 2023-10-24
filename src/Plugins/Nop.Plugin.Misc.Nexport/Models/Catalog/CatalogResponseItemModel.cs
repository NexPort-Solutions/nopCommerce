using NexportApi.Model;
using Nop.Web.Framework.Models;
using static NexportApi.Model.Enums;

namespace Nop.Plugin.Misc.Nexport.Models.Catalog;

public record CatalogResponseItemModel : BaseNopModel
{
    public Guid OrgId { get; set; }

    public Guid CatalogId { get; set; }

    public string? Name { get; set; }

    public DateTime? UtcDateCreated { get; set; }

    public DateTime? UtcDateLastModified { get; set; }

    public PricingModelEnum? PricingModel { get; set; }

    public Guid? OwnerId { get; set; }

    public string? OwnerName { get; set; }

    public string? OrgName { get; set; }

    public string? OrgShortName { get; set; }

    public bool? IsEnabled { get; set; }

    public PublishingModelEnum? PublishingModel { get; set; }

    public string? AccessTimeLimit { get; set; }
}
