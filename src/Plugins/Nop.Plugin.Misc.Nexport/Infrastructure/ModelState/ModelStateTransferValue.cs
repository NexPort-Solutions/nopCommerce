using System.Collections.Generic;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.ModelState
{
    public class ModelStateTransferValue
    {
        public string Key { get; set; }

        public string AttemptedValue { get; set; }

        public object RawValue { get; set; }

        public ICollection<string> ErrorMessages { get; set; } = new List<string>();
    }
}
