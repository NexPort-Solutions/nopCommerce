using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record CustomerMissingInfoModel : BaseNopModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string ReturnUrl { get; set; }
}