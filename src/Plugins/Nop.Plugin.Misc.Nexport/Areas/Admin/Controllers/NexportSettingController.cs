using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Setting;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class NexportSettingController : BaseAdminController
{
    private readonly ISettingService _settingService;
    private readonly IPermissionService _permissionService;
    private readonly INexportSettingModelFactory _nexportSettingModelFactory;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly INotificationService _notificationService;

    public NexportSettingController(
        ISettingService settingService,
        IPermissionService permissionService,
        INexportSettingModelFactory nexportSettingModelFactory,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        INotificationService notificationService)
    {
        _settingService = settingService;
        _permissionService = permissionService;
        _nexportSettingModelFactory = nexportSettingModelFactory;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _notificationService = notificationService;
    }


    public virtual async Task<IActionResult> Store()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        //prepare model
        var model = await _nexportSettingModelFactory.PrepareNexportStoreSettingsModelAsync();

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Setting/StoreSettings.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Store(NexportStoreSettingsModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var storeSettings = await _settingService.LoadSettingAsync<NexportStoreSettings>();
            storeSettings = model.ToSettings(storeSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(storeSettings, x => x.RedirectAfterOrderConfirmation, model.RedirectAfterOrderConfirmation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeSettings, x => x.RedirectAfterOrderConfirmationPath, model.RedirectAfterOrderConfirmationPath_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeSettings, x => x.DisplayManagePurchasesLink, model.DisplayManagePurchasesLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeSettings, x => x.PasswordRecoveryCooldownMinutes, model.PasswordRecoveryCooldownMinutes_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Store");
        }

        //prepare model
        model = await _nexportSettingModelFactory.PrepareNexportStoreSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Setting/StoreSettings.cshtml", model);
    }

    public virtual async Task<IActionResult> RequestProtection()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<NexportRequestProtectionSettings>(storeId: 0);
        var model = new NexportRequestProtectionSettingsModel
        {
            BlockRecognizedCrawlersOnAuthenticationPages = settings.BlockRecognizedCrawlersOnAuthenticationPages,
            BlockKnownProbePaths = settings.BlockKnownProbePaths,
            BlockedRequestFileExtensions = string.Join(Environment.NewLine,
                settings.BlockedRequestFileExtensions ?? new List<string>()),
            BlockedRequestPathPrefixes = string.Join(Environment.NewLine,
                settings.BlockedRequestPathPrefixes ?? new List<string>())
        };

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Setting/RequestProtectionSettings.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RequestProtection(NexportRequestProtectionSettingsModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        var pathPrefixes = ParseEntries(model.BlockedRequestPathPrefixes);
        var fileExtensions = ParseEntries(model.BlockedRequestFileExtensions);
        try
        {
            _ = new NexportRequestPathPolicy(pathPrefixes, Array.Empty<string>());
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.BlockedRequestPathPrefixes), exception.Message);
        }

        try
        {
            _ = new NexportRequestPathPolicy(Array.Empty<string>(), fileExtensions);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.BlockedRequestFileExtensions), exception.Message);
        }

        if (!ModelState.IsValid)
        {
            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Setting/RequestProtectionSettings.cshtml", model);
        }

        var settings = await _settingService.LoadSettingAsync<NexportRequestProtectionSettings>(storeId: 0);
        settings.BlockRecognizedCrawlersOnAuthenticationPages = model.BlockRecognizedCrawlersOnAuthenticationPages;
        settings.BlockKnownProbePaths = model.BlockKnownProbePaths;
        settings.BlockedRequestPathPrefixes = pathPrefixes;
        settings.BlockedRequestFileExtensions = fileExtensions;
        await _settingService.SaveSettingAsync(settings, storeId: 0);
        await _settingService.ClearCacheAsync();

        await _customerActivityService.InsertActivityAsync("EditSettings",
            await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(
            "Plugins.Misc.Nexport.Admin.Configuration.Settings.RequestProtection.RestartRequired"));

        return RedirectToAction(nameof(RequestProtection));
    }

    private static List<string> ParseEntries(string entries)
    {
        return (entries ?? string.Empty)
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
