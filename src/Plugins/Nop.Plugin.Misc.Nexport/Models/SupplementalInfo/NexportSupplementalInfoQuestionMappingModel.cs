﻿using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportSupplementalInfoQuestionMappingModel : BaseNopEntityModel
    {
        public int NexportProductMappingId { get; set; }

        public int SupplementalInfoQuestionId { get; set; }
    }
}