using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportCatalogSearchModel : BaseSearchModel
    {
        public NexportCatalogSearchModel()
        {
            NexportSyllabusListSearch = new NexportSyllabusListSearchModel();
        }

        public Guid? OrgId { get; set; }

        public int NopProductId { get; set; }

        public NexportSyllabusListSearchModel NexportSyllabusListSearch { get; set; }
    }
}
