using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Factories;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models.Plugins;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Controller
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class NexportDiscountPerCreditHoursController : BasePluginController
    {
        private readonly INexportDiscountPerCreditHourModelFactory _nexportDiscountPerCreditHourModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        public NexportDiscountPerCreditHoursController(
            INexportDiscountPerCreditHourModelFactory nexportDiscountPerCreditHourModelFactory,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IDiscountService discountService,
            ISettingService settingService,
            IProductService productService,
            IStoreService storeService)
        {
            _nexportDiscountPerCreditHourModelFactory = nexportDiscountPerCreditHourModelFactory;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _discountService = discountService;
            _settingService = settingService;
            _productService = productService;
            _storeService = storeService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetModifiedLocaleResources(
            DiscountPerCreditHoursPluginResourceListSearchModel searchModel, string friendlyName)
        {
            var model = await _nexportDiscountPerCreditHourModelFactory
                .PrepareDiscountPerCreditHourPluginResourceListModelAsync(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> OverrideResources(ICollection<int> selectedIds, bool allChecked)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (selectedIds != null && selectedIds.Count != 0)
            {
                foreach (var id in selectedIds)
                {
                    var localeStringResourceById = await _localizationService.GetLocaleStringResourceByIdAsync(id);

                    if (localeStringResourceById != null)
                    {
                        var discountPerCreditLocaleResources =
                            NexportDiscountPerCreditHoursPluginService.GetLocaleResources();
                        var overridingResourceValue = discountPerCreditLocaleResources.Where(l =>
                                l.Key.ToLower() == localeStringResourceById.ResourceName.ToLower())
                            .Select(l => l.Value)
                            .First();

                        if (overridingResourceValue != null)
                        {
                            localeStringResourceById.ResourceValue = overridingResourceValue;

                            await _localizationService.UpdateLocaleStringResourceAsync(localeStringResourceById);
                        }
                    }
                }

                if (allChecked)
                {
                    var discountPerCreditSetting =
                        await _settingService.GetSettingAsync(
                            "Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources");

                    if (discountPerCreditSetting != null)
                    {
                        await _settingService.DeleteSettingAsync(discountPerCreditSetting);
                    }
                }

            }
            else
            {
                return NoContent();
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            //load the discount
            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            //check whether the discount requirement exists
            if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
                return Content("Failed to load requirement.");

            var creditHours = await _settingService.GetSettingByKeyAsync<decimal>(
                string.Format(NexportDiscountDefaults.SettingsKey, discountRequirementId ?? 0));

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                CreditHours = creditHours
            };

            //set the HTML field prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(NexportDiscountDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);

            return View("~/Plugins/Misc.Nexport.DiscountPerCreditHours/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure(RequirementModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            // Load the discount
            var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            // Get the discount requirement
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = model.DiscountId,
                    DiscountRequirementRuleSystemName = NexportDiscountDefaults.SystemName
                };

                await _discountService.InsertDiscountRequirementAsync(discountRequirement);
            }

            // Save restricted customer role identifier
            await _settingService.SetSettingAsync(string.Format(NexportDiscountDefaults.SettingsKey, discountRequirement.Id), model.CreditHours);

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }
    }
}
