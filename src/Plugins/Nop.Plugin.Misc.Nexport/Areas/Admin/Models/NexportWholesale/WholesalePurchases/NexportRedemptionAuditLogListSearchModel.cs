using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionAuditLogListSearchModel : BaseSearchModel
{
    public Guid InvoiceItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AuditLog.Redemption.SearchCustomerName")]
    public string SearchCustomerName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AuditLog.Redemption.SearchTargetedCustomerName")]
    public string SearchTargetedCustomerName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AuditLog.Redemption.SearchLogType")]
    public NexportRedemptionAuditLogTypeEnum? SearchAuditLogType { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AuditLog.Redemption.DateCreatedFrom")]
    [UIHint("~/Plugins/Misc.Nexport/Views/Shared/EditorTemplates/DateNullable.cshtml")]
    public DateTime? DateCreatedFrom { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AuditLog.Redemption.DateCreatedTo")]
    [UIHint("~/Plugins/Misc.Nexport/Views/Shared/EditorTemplates/DateNullable.cshtml")]
    public DateTime? DateCreatedTo { get; set; }

    public IList<SelectListItem> AvailableAuditLogType { get; set; }

    public NexportRedemptionAuditLogListSearchModel()
    {
        SetPopupGridPageSize();

        AvailableAuditLogType = new List<SelectListItem>();
    }
}
