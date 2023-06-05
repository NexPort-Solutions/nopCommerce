using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class WholesaleController : BaseAdminController
{
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IOrderService _orderService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreService _storeService;
    private readonly IWholesaleService _wholesaleService;

    public WholesaleController(INexportPluginModelFactory nexportPluginModelFactory, IPermissionService permissionService, IOrderService orderService, IStoreService storeService, IWholesaleService wholesaleService)
    {
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _permissionService = permissionService;
        _orderService = orderService;
        _storeService = storeService;
        _wholesaleService = wholesaleService;
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            return AccessDeniedView();
        }

        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync();
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(int storeId, Guid organizationId, int redeemByDay,
        int redeemByMonth, int redeemByYear, string isRedemptionPeriodUnlimited, int quantity,
        int purchasingAgentCustomerId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            return AccessDeniedView();
        }

        var redeemByDate = new DateOnly(redeemByYear, redeemByMonth, redeemByDay);
        if (storeId <= 0
            || organizationId == Guid.Empty
            || redeemByDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest();
        }

        var parsedIsRedemptionPeriodUnlimited = isRedemptionPeriodUnlimited is "on";
        var shoppingCartItem = new ShoppingCartItem
        {
            ShoppingCartType = ShoppingCartType.ShoppingCart,
            StoreId = storeId,
            ProductId = productId,
            AttributesXml = null,
            CustomerEnteredPrice = decimal.Zero,
            Quantity = quantity,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
            CustomerId = purchasingAgentCustomerId,
        };

        var shoppingCartItems = new List<ShoppingCartItem> { shoppingCartItem };

        var processingPaymentRequest = new ProcessPaymentRequest
        {
            OrderGuid = Guid.NewGuid(),
            OrderGuidGeneratedOnUtc = DateTime.UtcNow,
            StoreId = storeId,
            CustomerId = purchasingAgentCustomerId,
            PaymentMethodSystemName = "Payments.Manual"
        };
        await _wholesaleService.PlaceOrderForCustomerAsync(processingPaymentRequest, shoppingCartItems, false);
        return NoContent();
    }
}
