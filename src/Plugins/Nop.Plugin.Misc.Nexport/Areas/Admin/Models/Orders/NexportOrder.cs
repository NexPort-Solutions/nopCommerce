using System;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders
{
    internal class NexportOrder : Order
    {
        public bool IsWholesale { get; init; }
        public Guid OrganizationId { get; init; }
    }
}
