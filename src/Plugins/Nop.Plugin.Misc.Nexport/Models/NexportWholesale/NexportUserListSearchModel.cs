using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale
{
    public record NexportUserListSearchModel : BaseSearchModel
    {
        public string? SearchEmail { get; set; }

        public NexportUserListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
