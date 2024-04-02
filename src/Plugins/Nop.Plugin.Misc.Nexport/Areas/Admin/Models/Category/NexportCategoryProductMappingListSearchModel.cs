using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category
{
    public record NexportCategoryProductMappingListSearchModel : BaseSearchModel
    {
        public NexportCategoryProductMappingListSearchModel()
        {
            SetGridPageSize();
        }

        public int NopCategoryId { get; set; }

    }
}
