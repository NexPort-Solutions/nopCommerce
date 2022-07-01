using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public record NexportSupplementalInfoOptionSearchModel : BaseSearchModel
    {
        public int QuestionId { get; set; }
    }
}
