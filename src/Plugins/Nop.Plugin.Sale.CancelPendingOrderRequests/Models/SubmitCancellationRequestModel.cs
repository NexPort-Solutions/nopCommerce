using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public record SubmitCancellationRequestModel : BaseNopModel
    {
        public int OrderId { get; set; }

        [NopResourceDisplayName("CancellationRequests.CancelReason")]
        public int CancellationRequestReasonId { get; set; }

        public IList<PendingOrderCancellationRequestReasonModel> AvailableCancelReasons { get; set; } = new List<PendingOrderCancellationRequestReasonModel>();

        [NopResourceDisplayName("CancellationRequests.Comments")]
        public string Comments { get; set; }

        public string Result { get; set; }
    }
}