using Nop.Web.Framework.Models;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public record CancellationRequestSettingsModel : BaseNopModel
    {
        public PendingOrderCancellationRequestReasonSearchModel CancellationRequestReasonSearchModel { get; set; } = new();
    }
}
