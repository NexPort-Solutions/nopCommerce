using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public interface IPendingOrderCancellationRequestService
    {
        Task<IPagedList<PendingOrderCancellationRequest>> SearchCancellationRequestsAsync(int storeId = 0,
            int customerId = 0,
            PendingOrderCancellationRequestStatus? requestStatus = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertCancellationRequestReasonAsync(PendingOrderCancellationRequestReason cancellationRequestReason);

        Task DeleteCancellationRequestReasonAsync(PendingOrderCancellationRequestReason cancellationRequestReason);

        Task UpdateCancellationRequestReasonAsync(PendingOrderCancellationRequestReason cancellationRequestReason);

        Task<IList<PendingOrderCancellationRequestReason>> GetAllCancellationRequestReasonsAsync();

        Task<PendingOrderCancellationRequestReason> GetCancellationRequestReasonByIdAsync(int reasonId);

        Task<bool> HasCancellationRequestForOrderAsync(int orderId);

        Task InsertCancellationRequestAsync(PendingOrderCancellationRequest cancellationRequest);

        Task DeleteCancellationRequestAsync(PendingOrderCancellationRequest cancellationRequest);

        Task UpdateCancellationRequestAsync(PendingOrderCancellationRequest cancellationRequest);

        Task<PendingOrderCancellationRequest> GetCancellationRequestByIdAsync(int requestId);

        Task<IList<int>> SendNewCancellationRequestStoreOwnerNotificationAsync(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId);

        Task<IList<int>> SendNewCancellationRequestCustomerNotificationAsync(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId);

        Task<IList<int>> SendCancellationRequestCustomerNotificationAsync(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId, string template);

        Task VoidCancelledOrderAsync(Order order);
    }
}
