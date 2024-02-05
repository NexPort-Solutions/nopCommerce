using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportUserModel : BaseNopEntityModel
    {
        public Guid? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public Guid? OwnerOrgId { get; set; }
        public string? OwnerOrg { get; set; }
    }
}
