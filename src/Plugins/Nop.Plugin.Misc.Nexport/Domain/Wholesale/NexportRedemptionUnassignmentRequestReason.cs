using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale
{
    public class NexportRedemptionUnassignmentRequestReason : BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }

        public int DisplayOrder { get; set; }
    }
}
