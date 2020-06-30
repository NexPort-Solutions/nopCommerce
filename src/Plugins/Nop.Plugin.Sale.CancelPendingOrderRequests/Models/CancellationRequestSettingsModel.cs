using Nop.Web.Framework.Models;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public class CancellationRequestSettingsModel : BaseNopModel
    {
        public CancellationRequestSettingsModel()
        {
            CancellationRequestReasonSearchModel = new PendingOrderCancellationRequestReasonSearchModel();
        }

        public PendingOrderCancellationRequestReasonSearchModel CancellationRequestReasonSearchModel { get; set; }
    }
}
