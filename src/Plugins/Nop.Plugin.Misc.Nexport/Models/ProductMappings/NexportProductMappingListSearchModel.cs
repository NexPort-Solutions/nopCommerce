using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings
{
    public record NexportProductMappingListSearchModel : BaseSearchModel
    {
        public NexportProductMappingListSearchModel()
        {
            AvailableNexportProductTypes = new List<SelectListItem>();

            SetGridPageSize();
        }

        public Guid? NexportProductId { get; set; }

        public Guid? NexportCatalogId { get; set; }

        public Guid? NexportSyllabusId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductType")]
        public NexportProductTypeEnum? NexportProductType { get; set; }

        public int? NopProductId { get; set; }

       [NopResourceDisplayName("Plugins.Misc.Nexport.ProductMapping.SearchNexportProductName")]
        public string SearchNexportProductName { get; set; }

        public IList<SelectListItem> AvailableNexportProductTypes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.ProductMapping.SearchStoreName")]
        public string SearchStoreName { get; set; }

    }
}
