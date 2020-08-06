using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Models
{
    public class ArchwayStoreAddressModel : BaseNopModel
    {
        public int storeNumber { get; set; }

        public string name { get; set; }

        public string storeType { get; set; }
    }
}