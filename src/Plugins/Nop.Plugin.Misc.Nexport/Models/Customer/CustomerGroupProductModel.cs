using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record CustomerGroupProductModel : BaseNopModel
{
    public CustomerGroupProductModel() { }

    public Guid? ProductGuid { get; set; }
    public string? Name { get; set; }
}

public record CustomerGroupProductsModel : BaseNopModel
{
    public IList<CustomerGroupProductModel> GroupProducts { get; } = new List<CustomerGroupProductModel>();
    public CustomerGroupModel? GroupModel { get; set; }
    public PagerModel? PagerModel { get; set; }

    #region Nested class

    /// <summary>
    /// record that has only page for route value. Used for (My Account) My Product Reviews pagination
    /// </summary>
    public record CustomerGroupProductsRouteValues : IRouteValues
    {
        public int PageNumber { get; set; }
    }

    #endregion Nested class
}
