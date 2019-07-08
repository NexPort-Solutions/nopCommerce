using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportSyllabusListSearchModel : BaseSearchModel
    {
        public NexportSyllabusListSearchModel()
        {
            SetGridPageSize();
        }

        public Guid CatalogId { get; set; }
    }
}
