using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportProductStoreMapping : BaseEntity
    {
        public int NexportProductMappingId { get; set; }

        public int StoreId { get; set; }
    }
}
