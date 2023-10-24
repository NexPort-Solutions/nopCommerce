using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class RequiredSupplementalInfo : BaseEntity
{
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public int QuestionId { get; set; }
    public DateTime UtcDateCreated { get; set; }
}
