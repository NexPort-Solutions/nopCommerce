using System;
using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Models
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
