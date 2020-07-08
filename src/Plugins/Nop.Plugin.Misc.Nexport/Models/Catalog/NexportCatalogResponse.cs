using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Catalog
{
    public class NexportCatalogResponse : NexportApiResponseBase
    {
        public List<CatalogResponseItem> CatalogList { get; set; }
    }
}