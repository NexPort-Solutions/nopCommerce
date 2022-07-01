using System.Collections.Generic;
using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Organization
{
    public record NexportOrganizationListModel : BaseNopModel
    {
        public IList<OrganizationResponseItem> Organizations { get; set; } = new List<OrganizationResponseItem>();
    }
}
