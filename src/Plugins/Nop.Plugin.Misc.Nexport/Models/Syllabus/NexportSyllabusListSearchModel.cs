using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Syllabus
{
    public class NexportSyllabusListSearchModel : BaseSearchModel
    {
        public NexportSyllabusListSearchModel()
        {
            SetGridPageSize();
        }

        public Guid CatalogId { get; set; }

        public Guid ProductId { get; set; }

        public int NopProductId { get; set; }
    }
}
