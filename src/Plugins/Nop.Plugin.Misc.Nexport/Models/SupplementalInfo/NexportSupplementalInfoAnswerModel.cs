using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportSupplementalInfoAnswerModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public string StoreName { get; set; }

        public int QuestionId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Option.Text")]
        public string OptionText { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.Status")]
        public NexportSupplementalInfoAnswerStatus Status { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.NexportMembership")]
        public IList<Guid> NexportMemberships { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.UtcDateCreated")]
        public DateTime UtcDateCreated { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.UtcDateModified")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcDateModified { get; set; }
    }
}
