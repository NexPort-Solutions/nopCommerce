using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using static Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo.Answer;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoAnswerModel : BaseNopEntityModel
{
    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public string? StoreName { get; set; }

    public int QuestionId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Option.NONE")]
    public string? OptionText { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.Status")]
    public AnswerStatus Status { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.Membership")]
    public IList<Guid> Memberships { get; set; } = new List<Guid>();

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.UtcDateCreated")]
    public DateTime UtcDateCreated { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Answer.UtcDateModified")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcDateModified { get; set; }
}
