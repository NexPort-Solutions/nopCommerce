using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public class SubmitCancellationRequestModel : BaseNopModel
    {
        public SubmitCancellationRequestModel()
        {
            AvailableCancelReasons = new List<PendingOrderCancellationRequestReasonModel>();
        }

        public int OrderId { get; set; }

        [NopResourceDisplayName("CancellationRequests.CancelReason")]
        public int CancellationRequestReasonId { get; set; }

        public IList<PendingOrderCancellationRequestReasonModel> AvailableCancelReasons { get; set; }

        [NopResourceDisplayName("CancellationRequests.Comments")]
        public string Comments { get; set; }

        public string Result { get; set; }
    }
}