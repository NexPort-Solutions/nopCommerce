
using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;

public record CustomerStepModel : BaseSearchModel
{
    public bool CustomerStepSendViaEmail { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Customer.SearchNameOrEmail")]
    public string SearchEmail { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Customer.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Customer.FirstName")]
    public string FirstName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Customer.LastName")]
    public string LastName { get; set; }

    public Guid? SelectedUserId { get; set; }

    public string SelectedUserFirstName { get; set; }

    public string SelectedUserLastName { get; set; }

    public string SelectedUserEmailAddress { get; set; }

    public bool TableFirstDraw { get; set; } = true;

    public CustomerStepModel()
    {
        SetGridPageSize();
    }
}