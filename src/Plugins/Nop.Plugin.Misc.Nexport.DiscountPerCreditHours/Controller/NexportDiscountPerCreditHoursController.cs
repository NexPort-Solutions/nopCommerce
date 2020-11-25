using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
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

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Controller
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class NexportDiscountPerCreditHoursController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        public NexportDiscountPerCreditHoursController(
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IDiscountService discountService,
            ISettingService settingService,
            IProductService productService,
            IStoreService storeService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _discountService = discountService;
            _settingService = settingService;
            _productService = productService;
            _storeService = storeService;
        }

        public IActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            //load the discount
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            //check whether the discount requirement exists
            if (discountRequirementId.HasValue && _discountService.GetDiscountRequirementById(discountRequirementId.Value) is null)
                return Content("Failed to load requirement.");

            var creditHours = _settingService.GetSettingByKey<decimal>(string.Format(NexportDiscountDefaults.SettingsKey, discountRequirementId ?? 0));

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
        public IActionResult Configure(RequirementModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            // Load the discount
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            // Get the discount requirement
            var discountRequirement = _discountService.GetDiscountRequirementById(model.RequirementId);

            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = model.DiscountId,
                    DiscountRequirementRuleSystemName = NexportDiscountDefaults.SystemName
                };

                _discountService.InsertDiscountRequirement(discountRequirement);
            }

            // Save restricted customer role identifier
            _settingService.SetSetting(string.Format(NexportDiscountDefaults.SettingsKey, discountRequirement.Id), model.CreditHours);

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }
    }
}
