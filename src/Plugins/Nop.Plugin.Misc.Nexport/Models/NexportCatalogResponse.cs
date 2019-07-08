using System.Collections.Generic;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportCatalogResponse : NexportApiResponseBase
    {
        public List<CatalogResponseItem> CatalogList { get; set; }
    }
}