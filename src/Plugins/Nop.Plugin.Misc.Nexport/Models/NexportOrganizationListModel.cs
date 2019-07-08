using System.Collections.Generic;
using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportOrganizationListModel : BaseNopModel
    {
        public NexportOrganizationListModel()
        {
            Organizations = new List<OrganizationResponseItem>();
        }

        public IList<OrganizationResponseItem> Organizations { get; set; }
    }
}
