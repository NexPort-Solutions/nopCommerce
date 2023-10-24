using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order;

public record AdditionalInfoModel : BaseNopModel
{
    public int Id { get; set; }

    public ApprovalModel? ApprovalModel { get; set; }
}
