using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Domains
{
    public class PendingOrderCancellationRequestReason : BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }

        public int DisplayOrder { get; set; }
    }
}
