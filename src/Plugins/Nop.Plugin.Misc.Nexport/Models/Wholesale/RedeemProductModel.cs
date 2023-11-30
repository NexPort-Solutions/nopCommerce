
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record RedeemProductModel : BaseNopEntityModel
    {
        public bool AdminView { get; set; } = false;

        public NexportGroupModel CurrentGroup { get; set; } = new NexportGroupModel {Id = null, Name = "No Group"};

        public Product CurrentProduct { get; set; }

        public Guid InvoiceItemId { get; set; }

        public int CustomerId { get; set; }

        public string returnUrl { get; set; }

        public string? AssignmentType { get; set; } = "Email";


        public IList<SelectListItem> AvailableMappings { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectTraining")]
        public int? SelectedProductMappingId { get; set; }

        public int? ProductMappingIdForOpenEndedProduct { get; set; }
    }
}
