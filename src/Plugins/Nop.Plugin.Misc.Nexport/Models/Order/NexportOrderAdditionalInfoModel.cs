using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public class NexportOrderAdditionalInfoModel : BaseNopModel
    {
        public int OrderId { get; set; }

        public NexportOrderApprovalModel NexportOrderApprovalModel { get; set; }
    }
}
