using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;

public record ListCustomerAnswersSearchModel : BaseSearchModel
{
    public ListCustomerAnswersSearchModel() => SetGridPageSize();

    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
}
