using System;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Catalog
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
