using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
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
