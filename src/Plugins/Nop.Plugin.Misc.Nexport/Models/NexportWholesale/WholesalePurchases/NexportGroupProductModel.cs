using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases
{
    public record NexportGroupProductModel : BaseNopEntityModel
    {
        public string? GroupName { get; set; }
        public string? GroupShortName { get; set; }
        public string? ProductName { get; set; }
        public int Available { get; set; }
        public int Awaiting { get; set; }
        public int Redeemed { get; set; }
        public Guid? GroupId { get; set; }
    }
}
