using Nop.Core.Domain.Orders;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Factories
{
    public interface IPendingOrderCancellationRequestModelFactory
    {
        PendingOrderCancellationRequestSearchModel PreparePendingOrderCancellationRequestSearchModel
            (PendingOrderCancellationRequestSearchModel searchModel);

        PendingOrderCancellationRequestListModel PreparePendingOrderCancellationRequestListModel
            (PendingOrderCancellationRequestSearchModel searchModel);

        SubmitCancellationRequestModel PrepareSubmitCancellationRequestModel(SubmitCancellationRequestModel model, Order order);

        PendingOrderCancellationRequestModel PreparePendingOrderCancellationRequestModel(
            PendingOrderCancellationRequestModel model, PendingOrderCancellationRequest cancellationRequest, bool excludeProperties = false);
    }
}