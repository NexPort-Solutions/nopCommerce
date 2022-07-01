using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Models
{
    public record ArchwayStoreEmployeePositionModel : BaseNopModel
    {
        public string id { get; set; }

        public string name { get; set; }
    }
}