using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Enrollment
{
    public record NexportEnrollmentListSearchModel : BaseSearchModel
    {
        public Guid UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public NexportEnrollmentListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
