using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models
{
    public class RequirementModel
    {
        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours")]
        public decimal CreditHours { get; set; }
    }
}
