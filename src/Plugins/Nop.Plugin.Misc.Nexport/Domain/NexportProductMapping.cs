using System;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportProductMapping : BaseEntity, INexportProductMapping
    {
        public int NopProductId { get; set; }

        public string NexportProductName { get; set; }

        public string DisplayName { get; set; }

        public Guid? NexportCatalogSyllabusLinkId { get; set; }

        public Guid NexportCatalogId { get; set; }

        public Guid? NexportSyllabusId { get; set; }

        /// <summary>
        /// Optional set, default to store where the product is purchased (Org Id based of the store)
        /// </summary>
        public Guid? NexportSubscriptionOrgId { get; set; }

        public NexportProductTypeEnum Type { get; set; }

        public int? PublishingModel { get; set; }

        public int? PricingModel { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }

        public DateTime? UtcAvailableDate { get; set; }

        public DateTime? UtcEndDate { get; set; }

        public DateTime? UtcLastSynchronizationDate { get; set; }

        public int? AccessTimeLimit { get; set; }

        public decimal? CreditHours { get; set; }

        public bool IsSynchronized { get; set; }
    }
}
