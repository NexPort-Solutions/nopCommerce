using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionUnassignmentRequestReasonModel : BaseNopEntityModel, ILocalizedModel<NexportRedemptionUnassignmentRequestReasonLocalizedModel>
{
    [NopResourceDisplayName("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public IList<NexportRedemptionUnassignmentRequestReasonLocalizedModel> Locales { get; set; } = new List<NexportRedemptionUnassignmentRequestReasonLocalizedModel>();
}

public record NexportRedemptionUnassignmentRequestReasonLocalizedModel : ILocalizedLocaleModel
{
    [NopResourceDisplayName("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name")]
    public string Name { get; set; }

    public int LanguageId { get; set; }
}