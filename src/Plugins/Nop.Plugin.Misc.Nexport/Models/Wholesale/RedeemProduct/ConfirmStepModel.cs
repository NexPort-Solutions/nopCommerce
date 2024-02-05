
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record ConfirmStepModel : BaseNopEntityModel
    {
        public string? Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Product { get; set; }

        public string? SendViaEmail { get; set; }

        public string? ExistsInCampus { get; set; }

        public string? ExistsInMarketplace { get; set; }
    }
}
