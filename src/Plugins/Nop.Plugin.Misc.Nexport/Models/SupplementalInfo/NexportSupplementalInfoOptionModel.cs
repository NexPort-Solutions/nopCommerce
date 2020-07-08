using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportSupplementalInfoOptionModel : BaseNopEntityModel
    {
        public NexportSupplementalInfoOptionModel()
        {
            AddGroupMembershipMappingModel = new NexportProductGroupMembershipMappingModel();
            GroupMembershipMappingModels = new List<NexportProductGroupMembershipMappingModel>();
        }

        public int QuestionId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Option.Text")]
        public string OptionText { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateCreated")]
        public DateTime UtcDateCreated { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateModified")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcDateModified { get; set; }

        public NexportProductGroupMembershipMappingModel AddGroupMembershipMappingModel { get; set; }

        public IList<NexportProductGroupMembershipMappingModel> GroupMembershipMappingModels { get; set; }
    }
}