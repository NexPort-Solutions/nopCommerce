using System;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders
{
    public record NexportOrderModel : OrderModel
    {
        public string StoreUrl { get; set; }
    }
}
