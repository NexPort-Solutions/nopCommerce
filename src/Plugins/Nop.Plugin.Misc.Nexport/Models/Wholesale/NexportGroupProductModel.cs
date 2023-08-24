using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupProductModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public int Available { get; set; }
        public int Awaiting { get; set; }
        public int Redeemed { get; set; }
        public Guid GroupId { get; set; }
    }
}
