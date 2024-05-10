using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders
{
    public record WholesaleOrderProductSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        //[NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public bool SearchOnlyNexportProduct { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

        public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
    }
}
