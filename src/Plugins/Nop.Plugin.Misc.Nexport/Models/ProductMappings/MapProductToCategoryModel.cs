using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings
{
    public record MapProductToCategoryModel : BaseNopModel
    {
        [NopResourceDisplayName("Category")]
        public int SelectedCategoryId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public int ProductId { get; set; }

        public int? StoreId { get; set; }

        public MapProductToCategoryModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }
    }
}
