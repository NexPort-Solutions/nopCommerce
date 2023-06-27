using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public record OrderSummaryCartFooterModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.Order.GroupId")]
        public Guid GroupId { get; set; }

        public bool IsPurchasingAgent{ get; set; }

        public IList<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();
    }
}
