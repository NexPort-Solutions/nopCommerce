using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Catalog
{
    public record NexportCatalogResponseItemModel : BaseNopModel
    {
        public Guid OrgId { get; set; }

        public Guid CatalogId { get; set; }

        public string Name { get; set; }

        public DateTime? UtcDateCreated { get; set; }

        public DateTime? UtcDateLastModified { get; set; }

        public NexportApi.Model.Enums.PricingModelEnum? PricingModel { get; set; }

        public Guid? OwnerId { get; set; }

        public string OwnerName { get; set; }

        public string OrgName { get; set; }

        public string OrgShortName { get; set; }

        public bool? IsEnabled { get; set; }

        public NexportApi.Model.Enums.PublishingModelEnum? PublishingModel { get; set; }

        public string AccessTimeLimit { get; set; }
    }
}
