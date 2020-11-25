using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Caching;

namespace Nop.Plugin.Misc.Nexport.Services.Caching
{
    public class NexportProductMappingCacheEventConsumer : CacheEventConsumer<NexportProductMapping>
    {
        protected override void ClearCache(NexportProductMapping entity)
        {
        }
    }
}
