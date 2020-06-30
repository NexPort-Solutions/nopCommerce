using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public class PendingOrderCancellationRequestReasonModel : BaseNopEntityModel, ILocalizedModel<PendingOrderCancellationRequestReasonLocalizedModel>
    {
        public PendingOrderCancellationRequestReasonModel()
        {
            Locales = new List<PendingOrderCancellationRequestReasonLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Order.CancellationRequestReasons.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<PendingOrderCancellationRequestReasonLocalizedModel> Locales { get; set; }
    }

    public class PendingOrderCancellationRequestReasonLocalizedModel : ILocalizedLocaleModel
    {
        [NopResourceDisplayName("Admin.Configuration.Settings.Order.CancellationRequestReasons.Name")]
        public string Name { get; set; }

        public int LanguageId { get; set; }
    }
}