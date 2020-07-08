using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportSupplementalInfoQuestionModel : BaseNopEntityModel
    {
        public NexportSupplementalInfoQuestionModel()
        {
            AvailableQuestionTypes = new List<SelectListItem>();
            NexportSupplementalInfoOptionSearchModel = new NexportSupplementalInfoOptionSearchModel();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.Text")]
        public string QuestionText { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.Type")]
        public NexportSupplementalInfoQuestionType Type { get; set; }

        public IList<SelectListItem> AvailableQuestionTypes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateCreated")]
        public DateTime UtcDateCreated { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateModified")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcDateModified { get; set; }

        public NexportSupplementalInfoOptionSearchModel NexportSupplementalInfoOptionSearchModel { get; set; }
    }
}
