using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

public record SupplementalInfoOptionModel : BaseNopEntityModel
{
    public int QuestionId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Option.NONE")]
    public required string OptionText { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateCreated")]
    public DateTime UtcDateCreated { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SupplementalInfo.Question.UtcDateModified")]
    [UIHint("DateTimeNullable")]
    public DateTime? UtcDateModified { get; set; }

    public ProductGroupMembershipMappingModel AddGroupMembershipMappingModel { get; set; } = new();
    public IList<ProductGroupMembershipMappingModel> GroupMembershipMappingModels { get; init; } = new List<ProductGroupMembershipMappingModel>();
}
