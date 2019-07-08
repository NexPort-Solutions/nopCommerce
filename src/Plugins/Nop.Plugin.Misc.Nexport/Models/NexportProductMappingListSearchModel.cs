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

        public string NexportProductId { get; set; }

        public string NexportCatalogId { get; set; }

        public string NexportSyllabusId { get; set; }

        public NexportProductTypeEnum NexportProductType { get; set; }
    }
}
