using System;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings
{
    public class NexportProductMappingListSearchModel : BaseSearchModel
    {
        public NexportProductMappingListSearchModel()
        {
            SetGridPageSize();
        }

        public Guid? NexportProductId { get; set; }

        public Guid? NexportCatalogId { get; set; }

        public Guid? NexportSyllabusId { get; set; }

        public NexportProductTypeEnum? NexportProductType { get; set; }

        public int? NopProductId { get; set; }
    }
}
