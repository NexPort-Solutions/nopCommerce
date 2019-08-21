using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportTrainingListModel
    {
        public IList<NexportOrganizationModel> RedemptionOrganizations { get; set; }

        public Guid UserId { get; set; }
    }
}
