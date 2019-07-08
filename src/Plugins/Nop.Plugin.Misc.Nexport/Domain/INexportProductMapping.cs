using System;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public partial interface INexportProductMapping
    {
        int NopProductId { get; set; }

        string NexportProductName { get; set; }

        string DisplayName { get; set; }

        Guid? NexportCatalogSyllabusLinkId { get; set; }

        Guid NexportCatalogId { get; set; }

        Guid? NexportSyllabusId { get; set; }

        Guid? NexportSubscriptionOrgId { get; set; }

        NexportProductTypeEnum Type { get; set; }

        int? PublishingModel { get; set; }

        int? PricingModel { get; set; }

        DateTime? UtcLastModifiedDate { get; set; }

        DateTime? UtcAvailableDate { get; set; }

        DateTime? UtcEndDate { get; set; }

        DateTime? UtcLastSynchronizationDate { get; set; }

        int? AccessTimeLimit { get; set; }

        decimal? CreditHours { get; set; }
    }
}
