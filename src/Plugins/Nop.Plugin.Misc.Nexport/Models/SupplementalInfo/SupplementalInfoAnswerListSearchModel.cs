using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoAnswerListSearchModel : BaseSearchModel
{
    public SupplementalInfoAnswerListSearchModel()
    {
        SetGridPageSize();

        AvailableStores = new List<SelectListItem>();
    }

    public int CustomerId { get; set; }

    public int QuestionId { get; set; }

    public IList<SelectListItem> AvailableStores { get; init; }
}
