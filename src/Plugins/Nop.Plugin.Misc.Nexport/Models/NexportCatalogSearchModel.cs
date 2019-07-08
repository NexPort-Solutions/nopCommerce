using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportCatalogSearchModel : BaseSearchModel
    {
        public Guid? OrgId { get; set; }
    }
}
