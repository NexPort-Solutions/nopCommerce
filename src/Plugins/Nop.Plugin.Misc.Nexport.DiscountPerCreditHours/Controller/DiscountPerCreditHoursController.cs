using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models;
using System.Globalization;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Controller;

public class DiscountPerCreditHoursController : BaseAdminController
{
    private readonly IPermissionService _permission;
    private readonly IDiscountService _discount;
    private readonly ISettingService _setting;

    public DiscountPerCreditHoursController(
        IPermissionService permission,
        IDiscountService discount,
        ISettingService setting)
    {
        _permission = permission;
        _discount = discount;
        _setting = setting;
    }

    [HttpGet]
    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
        {
            return Content("Access denied");
        }
        if (await _discount.GetDiscountByIdAsync(discountId) is not { })
        {
            return BadRequest("Discount could not be loaded");
        }
        if (discountRequirementId is not null //check whether the discount requirement exists
            && await _discount.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
        {
            return BadRequest("Failed to load requirement.");
        }
        var creditHours = await _setting.GetSettingByKeyAsync<decimal>(string.Format(CultureInfo.InvariantCulture, NexportDiscountDefaults.SETTINGS_KEY, discountRequirementId ?? 0));
        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discountId,
            CreditHours = creditHours,
        };
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(CultureInfo.InvariantCulture, NexportDiscountDefaults.HTML_FIELD_PREFIX, discountRequirementId ?? 0);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
        {
            return Content("Access denied");
        }
        if (await _discount.GetDiscountByIdAsync(model.DiscountId) is null)
        {
            return NotFound(new { Errors = new[] { "Discount could not be loaded" } });
        }
        if (await _discount.GetDiscountRequirementByIdAsync(model.RequirementId) is not { } discountRequirement)
        {
            discountRequirement = new()
            {
                DiscountId = model.DiscountId,
                DiscountRequirementRuleSystemName = NexportDiscountDefaults.SYSTEM_NAME,
            };
            await _discount.InsertDiscountRequirementAsync(discountRequirement);
        }
        await _setting.SetSettingAsync(string.Format(CultureInfo.InvariantCulture, NexportDiscountDefaults.SETTINGS_KEY, discountRequirement.Id), model.CreditHours);
        return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
    }
}
