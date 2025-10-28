using Nop.Core;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Setting;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace Nop.Plugin.Misc.Nexport.Factories;

public class NexportSettingModelFactory : INexportSettingModelFactory
{
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;

    public NexportSettingModelFactory(
        IStoreContext storeContext,
        ISettingService settingService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
    }

    public async Task<NexportStoreSettingsModel> PrepareNexportStoreSettingsModelAsync(NexportStoreSettingsModel model = null)
    {
        //load settings for a chosen store scope
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var nexportStoreSettings = await _settingService.LoadSettingAsync<NexportStoreSettings>(storeId);

        //fill in model values from the entity
        model ??= nexportStoreSettings.ToSettingsModel<NexportStoreSettingsModel>();

        //fill in additional values (not existing in the entity)
        model.ActiveStoreScopeConfiguration = storeId;

        //fill in overridden values
        if (storeId > 0)
        {
            model.RedirectAfterOrderConfirmation_OverrideForStore = await _settingService.SettingExistsAsync(nexportStoreSettings, x => x.RedirectAfterOrderConfirmation, storeId);
            model.RedirectAfterOrderConfirmationPath_OverrideForStore = await _settingService.SettingExistsAsync(nexportStoreSettings, x => x.RedirectAfterOrderConfirmationPath, storeId);
            model.DisplayManagePurchasesLink_OverrideForStore = await _settingService.SettingExistsAsync(nexportStoreSettings, x => x.DisplayManagePurchasesLink, storeId);
        }

        return model;
    }
}