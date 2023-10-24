using Nop.Core;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Common;
using Nop.Web.Framework.Controllers;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;

namespace Nop.Plugin.Misc.Nexport.Controllers;

public interface IWholesaleController
{
    Task<IActionResult> GroupProductsAsync(Guid groupId);
}

public class WholesaleController : BasePluginController
{
    #region Fields

    private readonly IPluginModelFactory _model;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerService _customer;
    private readonly IGroupService _group;

    #endregion Fields

    #region Constructor

    public WholesaleController(
        IPluginModelFactory pluginModelFactory,
        IGenericAttributeService genericAttribute,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICustomerService customer,
        IGroupService group)
    {
        _model = pluginModelFactory;
        _genericAttribute = genericAttribute;
        _workContext = workContext;
        _storeContext = storeContext;
        _customer = customer;
        _group = group;
    }

    #endregion Constructor

    #region Actions

    [HttpPost]
    public async Task<IActionResult> SetPurchaseGroupForCustomer(string? groupSelected)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        // save group id for customer in generic attribute so it can be saved for the order later
        await _genericAttribute.SaveAttributeAsync(customer, Defaults.GROUP_FOR_CUSTOMER, groupSelected, store.Id);
        return Json(new { Result = true });
    }

    [HttpGet]
    public async Task<IActionResult> CustomerGroups()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/Groups.cshtml";
        ViewData["ModelForPartialView"] = new GroupListSearchModel { AdminView = false };
        return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> CustomerGroupProducts(Guid groupId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        var groupProductListSearchModel = await _model.GroupProductListSearchModel(groupId, false);
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/GroupProducts.cshtml";
        ViewData["ModelForPartialView"] = groupProductListSearchModel;
        return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> GroupProductRedemptions(Guid groupId, int productId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        var groupProductRedemptionListSearchModel = await _model.GroupProductRedemptionListSearchModel(groupId, productId);
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/GroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = groupProductRedemptionListSearchModel;
        return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> RedeemProduct(Guid groupId, int productId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = await _model.RedeemProductOrModifyProductRedemptionModel(groupId, productId, false);
        return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> ModifyRedemption(Guid groupId, int productId, Guid? invoiceItemId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = await _model.RedeemProductOrModifyProductRedemptionModel(groupId, productId, false, invoiceItemId);
        return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GetGroups(GroupListSearchModel model)
        => Json(await _model.GroupListModel(model));

    [HttpPost]
    public async Task<IActionResult> GetGroupProducts(GroupProductListSearchModel model, Guid groupId)
        => Json(await _model.GroupProductListModel(model, groupId));

    [HttpPost]
    public async Task<IActionResult> GetGroupProductRedemptions(GroupProductRedemptionListSearchModel model, Guid groupId, int productId)
        => Json(await _model.GroupProductCustomerListModel(model, groupId, productId));

    [HttpPost]
    public async Task<IActionResult> GetAvailableGroupProductRedemptionsCount(Guid groupId, int productId)
        => Json(new { result = await _group.GetAvailableGroupProductRedemptionsCount(groupId, productId) });
    #endregion Actions
}
