using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record RedeemProductModel : BaseNopEntityModel
{
    public bool AdminView { get; set; } = false;

    public NexportGroupModel CurrentGroup { get; set; } = new() { OrganizationId = null, Name = "No Group" };

    public Product CurrentProduct { get; set; }

    public Guid InvoiceItemId { get; set; }

    public Guid? UserId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string ReturnUrl { get; set; } = "";

    public string AssignmentType { get; set; } = "Email";

    public IList<SelectListItem> AvailableMappings { get; set; }

    public int ProductId { get; set; }

    public int? RedeemingProductId { get; set; }

    public string Email { get; set; }

    public DateTime? UtcStartDate { get; set; }

    public int? StoreId { get; set; }

    public Guid? PurchasingGroupId { get; set; }

    public bool IsOpenEnded { get; set; }

    public int? ExtensionOption { get; set; }
}