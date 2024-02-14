using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases
{
    public record NexportGroupProductListSearchModel : BaseSearchModel
    {
        public NexportGroupModel CurrentGroup { get; set; } =
            new NexportGroupModel {Id = null, Name = "No Group"};

        public bool AdminView = false;

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.SearchName")]
        public string SearchName { get; set; }

        public NexportGroupProductListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
