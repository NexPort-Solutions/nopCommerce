using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField
{
    public record NexportRegistrationFieldOptionSearchModel : BaseSearchModel
    {
        public int RegistrationFieldId { get; set; }
    }
}
