using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public interface IPendingOrderCancellationRequestService
    {
        IPagedList<PendingOrderCancellationRequest> SearchCancellationRequests(
            int storeId = 0, int customerId = 0,
            PendingOrderCancellationRequestStatus? requestStatus = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        void InsertCancellationRequestReason(PendingOrderCancellationRequestReason cancellationRequestReason);

        void DeleteCancellationRequestReason(PendingOrderCancellationRequestReason cancellationRequestReason);

        void UpdateCancellationRequestReason(PendingOrderCancellationRequestReason cancellationRequestReason);

        IList<PendingOrderCancellationRequestReason> GetAllCancellationRequestReasons();

        PendingOrderCancellationRequestReason GetCancellationRequestReasonById(int reasonId);

        bool HasCancellationRequestForOrder(int orderId);

        void InsertCancellationRequest(PendingOrderCancellationRequest cancellationRequest);

        void DeleteCancellationRequest(PendingOrderCancellationRequest cancellationRequest);

        void UpdateCancellationRequest(PendingOrderCancellationRequest cancellationRequest);

        PendingOrderCancellationRequest GetCancellationRequestById(int requestId);

        IList<int> SendNewCancellationRequestStoreOwnerNotification(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId);

        IList<int> SendNewCancellationRequestCustomerNotification(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId);

        IList<int> SendCancellationRequestCustomerNotification(PendingOrderCancellationRequest cancellationRequest,
            Order order, int languageId, string template);

        void VoidCancelledOrder(Order order);
    }
}
