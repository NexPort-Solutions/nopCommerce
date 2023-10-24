using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using static Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo.Question;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoQuestionModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.NONE")]
    public string? QuestionText { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.Description")]
    public string? Description { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.Type")]
    public QuestionType Type { get; set; }

    public List<SelectListItem> AvailableQuestionTypes { get; init; } = new();

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateCreated")]
    public DateTime UtcDateCreated { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateModified")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcDateModified { get; set; }

    public SupplementalInfoOptionSearchModel SupplementalInfoOptionSearchModel { get; set; } = new();
}
