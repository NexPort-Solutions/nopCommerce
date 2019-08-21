using System;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public partial class MapProductToNexportProductListModel : BasePagedListModel<ProductModel>
    {
        public string NexportProductName { get; set; }

        public Guid? NexportCatalogSyllabusLinkId { get; set; }

        public Guid NexportCatalogId { get; set; }

        public Guid? NexportSyllabusId { get; set; }

        public Guid? NexportSubscriptionOrgId { get; set; }

        public NexportProductTypeEnum Type { get; set; }

        public int? PublishingModel { get; set; }

        public int? PricingModel { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }

        public DateTime? UtcAvailableDate { get; set; }

        public DateTime? UtcEndDate { get; set; }

        public long? AccessTimeLimit { get; set; }
    }
}
