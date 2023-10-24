using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;

namespace Nop.Plugin.Misc.Nexport.Validators;

public class MapProductValidator : BaseNopValidator<MapProductModel>
{
    public MapProductValidator()
    {
        RuleFor(mapProductModel => mapProductModel.ProductId)
            .NotEmpty();
        RuleFor(mapProductModel => mapProductModel.CatalogId)
            .NotEmpty();
    }
}
