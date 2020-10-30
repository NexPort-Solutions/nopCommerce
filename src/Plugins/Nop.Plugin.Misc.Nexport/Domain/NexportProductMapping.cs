using System;
using System.Collections.Generic;
using NexportApi.Model;
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

        public string NexportSubscriptionOrgName { get; set; }

        public string NexportSubscriptionOrgShortName { get; set; }

        public NexportProductTypeEnum Type { get; set; }

        public CatalogResponseItem.PublishingModelEnum? PublishingModel { get; set; }

        public CatalogResponseItem.PricingModelEnum? PricingModel { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }

        public DateTime? UtcAvailableDate { get; set; }

        public DateTime? UtcEndDate { get; set; }

        public DateTime? UtcLastSynchronizationDate { get; set; }

        public DateTime? UtcAccessExpirationDate { get; set; }

        public string AccessTimeLimit { get; set; }

        public decimal? CreditHours { get; set; }

        public string SectionCeus { get; set; }

        public bool IsSynchronized { get; set; }

        public bool AutoRedeem { get; set; }

        public bool AllowExtension { get; set; }

        public bool IsExtensionProduct { get; set; }

        public string RenewalWindow { get; set; }

        public string RenewalDuration { get; set; }

        public int? RenewalCompletionThreshold { get; set; }

        public NexportEnrollmentRenewalApprovalMethodEnum? RenewalApprovalMethod { get; set; }

        public int? ExtensionPurchaseLimit { get; set; }

        public int? StoreId { get; set; }
    }
}
