using System.Collections.Generic;
using NexportApi.Model;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders
{
    public class WholesaleCreateModel
    {
        public IList<OrganizationResponseItem> Organizations { get; set; }
        public IList<Store> Stores { get; set; }
    }
}
