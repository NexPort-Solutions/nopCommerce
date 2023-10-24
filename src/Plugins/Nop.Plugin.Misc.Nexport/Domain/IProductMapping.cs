using System.ComponentModel.DataAnnotations;
using NexportApi.Model;
using static NexportApi.Model.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain;

public interface IProductMapping
{
    int NopProductId { get; set; }
    string? ProductName { get; set; }
    string? DisplayName { get; set; }
    Guid? CatalogSyllabusLinkId { get; set; }
    Guid? CatalogId { get; set; }
    Guid? SyllabusId { get; set; }
    Guid? SubscriptionOrgId { get; set; }
    ProductType Type { get; set; }
    PublishingModelEnum? PublishingModel { get; set; }
    PricingModelEnum? PricingModel { get; set; }
    DateTime? UtcLastModifiedDate { get; set; }
    DateTime? UtcAvailableDate { get; set; }
    DateTime? UtcEndDate { get; set; }
    DateTime? UtcLastSynchronizationDate { get; set; }
    DateTime? UtcAccessExpirationDate { get; set; }
    string? AccessTimeLimit { get; set; }
    decimal? CreditHours { get; set; }
}

public enum ProductType
{
    [Display(Name = "Catalog")]
    Catalog = 0,

    [Display(Name = "Section")]
    Section = 1,

    [Display(Name = "Training Plan")]
    TrainingPlan = 2,
}
