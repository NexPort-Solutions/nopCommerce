using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct
{
    public record ProductStepModel : BaseNopEntityModel
    {
        public IList<SelectListItem> AvailableMappings { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectTraining")]
        public int? SelectedProductMappingId { get; set; }

        public Guid InvoiceItemId { get; set; }

        public Product CurrentProduct { get; set; }

        public int? ProductMappingIdForOpenEndedProduct { get; set; }
    }
}
