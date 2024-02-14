using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale
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
        public string? OwnerOrgShortName { get; set; }
    }
}
