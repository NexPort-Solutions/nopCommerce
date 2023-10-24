using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Order;

public class SummaryCartFooter : NopViewComponent
{
    public const string HIDE_GROUP_SELECT = "HideGroupSelect";
    public const string DISCOUNT_LIST = "DiscountList";

    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IOrderTotalCalculationService _orderTotalCalculation;
    private readonly IShoppingCartService _shoppingCart;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IPluginModelFactory _modelFactory;
    private readonly IGenericAttributeService _genericAttributes;

    public SummaryCartFooter(
        IWorkContext workContext,
        IStoreContext storeContext,
        IOrderTotalCalculationService orderTotalCalculationService,
        IShoppingCartService shoppingCartService,
        IActionContextAccessor actionContextAccessor,
        IPluginModelFactory modelFactory,
        IGenericAttributeService genericAttributes)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _orderTotalCalculation = orderTotalCalculationService;
        _shoppingCart = shoppingCartService;
        _actionContextAccessor = actionContextAccessor;
        _modelFactory = modelFactory;
        _genericAttributes = genericAttributes;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object __)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        if ((_actionContextAccessor.ActionContext?.ActionDescriptor as ControllerActionDescriptor)?.ActionName is not nameof(ShoppingCartController.Cart)
            && await _genericAttributes.GetAttributeAsync<string>(customer, Defaults.GROUP_FOR_CUSTOMER, store.Id) is { } selectedGroupInfo
            && JsonConvert.DeserializeObject<GroupModel>(selectedGroupInfo) is { } selectedGroup)
        {
            return View(new SummaryCartFooterModel { Group = selectedGroup, GroupGuid = selectedGroup.GroupGuid });
        }
        return View();
    }
}
