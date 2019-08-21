using System;
using NexportApi.Model;
using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class BaseNexportProductMappingModel : BaseNopModel, INexportProductMapping
    {
        public int NopProductId { get; set; }

        public string NexportProductName { get; set; }

        public string DisplayName { get; set; }

        public Guid? NexportCatalogSyllabusLinkId { get; set; }

        public Guid NexportCatalogId { get; set; }

        public Guid? NexportSyllabusId { get; set; }

        public Guid? NexportSubscriptionOrgId { get; set; }

        public NexportProductTypeEnum Type { get; set; }

        public CatalogResponseItem.PublishingModelEnum? PublishingModel { get; set; }

        public CatalogResponseItem.PricingModelEnum? PricingModel { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }

        public DateTime? UtcAvailableDate { get; set; }

        public DateTime? UtcEndDate { get; set; }

        public DateTime? UtcLastSynchronizationDate { get; set; }

        public DateTime? UtcAccessExpirationDate { get; set; }

        public long? AccessTimeLimit { get; set; }

        public decimal? CreditHours { get; set; }
    }
}