using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category;

/// <summary>
/// Represents a category search model
/// </summary>
public partial record NexportCategorySearchModel : BaseSearchModel
{
    #region Ctor

    public NexportCategorySearchModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailablePublishedOptions = new List<SelectListItem>();

        SetGridPageSize();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchPublished")]
    public int SearchPublishedId { get; set; }

    public IList<SelectListItem> AvailablePublishedOptions { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchStore")]
    public int SearchStoreId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public bool HideStoresList { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Category.HasProductMapping")]
    public bool HasProductMapping { get; set; } = false;

    #endregion
}