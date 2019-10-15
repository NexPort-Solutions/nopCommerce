using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class MappingProduct : Product
    {
        public bool HasNexportMapping { get; set; }
    }
}
