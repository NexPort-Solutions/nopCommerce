using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupListSearchModel : BaseSearchModel
    {
        public bool AdminView = false;

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.SearchShortName")]
        public string SearchShortName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.SearchNumberOfProducts")]
        public int? SearchNumberOfProducts { get; set; }

        public NexportGroupListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
