using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Factories
{
    public interface IPendingOrderCancellationRequestModelFactory
    {
        Task<PendingOrderCancellationRequestSearchModel> PreparePendingOrderCancellationRequestSearchModelAsync
            (PendingOrderCancellationRequestSearchModel searchModel);

        Task<PendingOrderCancellationRequestListModel> PreparePendingOrderCancellationRequestListModelAsync
            (PendingOrderCancellationRequestSearchModel searchModel);

        Task<SubmitCancellationRequestModel> PrepareSubmitCancellationRequestModelAsync(
            SubmitCancellationRequestModel model,
            Order order);

        Task<PendingOrderCancellationRequestModel> PreparePendingOrderCancellationRequestModelAsync(
            PendingOrderCancellationRequestModel model, PendingOrderCancellationRequest cancellationRequest,
            bool excludeProperties = false);

        Task<PendingOrderCancellationRequestReasonModel> PreparePendingOrderCancellationRequestReasonModelAsync(
            PendingOrderCancellationRequestReasonModel model,
            PendingOrderCancellationRequestReason cancellationRequestReason, bool excludeProperties = false);

        Task<PendingOrderCancellationRequestReasonListModel> PreparePendingOrderCancellationRequestReasonListModelAsync(
            PendingOrderCancellationRequestReasonSearchModel searchModel);
    }
}