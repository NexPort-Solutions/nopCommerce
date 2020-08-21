using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;

namespace Nop.Plugin.Misc.Nexport.Validators
{
    public class MapNexportProductValidator : BaseNopValidator<MapNexportProductModel>
    {
        public MapNexportProductValidator()
        {
            RuleFor(x => x.NexportProductId)
                .NotEmpty();
            RuleFor(x => x.NexportCatalogId)
                .NotEmpty();
        }
    }
}
