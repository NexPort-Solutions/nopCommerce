using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record CustomerGroupModel : BaseNopModel
{
    public Guid? GroupGuid { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
}

public record CustomerGroupsModel : BaseNopModel
{
    public IList<CustomerGroupModel> Groups { get; } = new List<CustomerGroupModel>();
    public PagerModel? PagerModel { get; set; }

    #region Nested class

    /// <summary>
    /// record that has only page for route value. Used for (My Account) My Product Reviews pagination
    /// </summary>
    public record CustomerGroupsRouteValues : IRouteValues
    {
        public int PageNumber { get; set; }
    }

    #endregion Nested class
}
