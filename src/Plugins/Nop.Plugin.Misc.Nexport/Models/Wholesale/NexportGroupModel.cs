using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupModel : BaseNopEntityModel
    {
        public new Guid? Id { get; set; }
        public Guid ParentId { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public int Type { get; set; }
        public int NumberOfProducts { get; set; }
    }
}
