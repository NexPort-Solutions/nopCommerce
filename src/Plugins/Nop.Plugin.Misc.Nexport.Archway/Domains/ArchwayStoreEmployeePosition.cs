using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains
{
    public class ArchwayStoreEmployeePosition : BaseEntity
    {
        public int JobCode { get; set; }

        public string JobTitle { get; set; }

        public string JobType { get; set; }

        public string JobLevel { get; set; }
    }
}
