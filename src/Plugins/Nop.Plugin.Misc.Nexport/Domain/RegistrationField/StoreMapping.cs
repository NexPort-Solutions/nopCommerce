using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

public class StoreMapping : BaseEntity
{
    public int FieldId { get; set; }
    public int StoreId { get; set; }
}
