using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;

public class NexportDiscountPerCreditHoursPluginService
{
    private readonly ILocalizationService _localizationService;
    private readonly IPluginLocalizationService _pluginLocalizationService;
    private readonly ISettingService _settingService;

    private static readonly Dictionary<string, string> _localeResources = new()
    {
        {"Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours",
            "Credit hours"},
        {"Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours.Hint",
            "Minimum credit hours for the discount to be effective"},
        {"Plugins.Misc.Nexport.DiscountPerCreditHours.Errors.ModifiedLocaleResources",
            "There are modified resource values within the <b>Nexport discount per credit hours plugin</b> that conflicts with the default value. Review them <a href=\"javascript:OpenWindow(\'{0}\', 800, 500, true)\">here</a>."}
    };

    public static Dictionary<string, string> GetLocaleResources()
    {
        return _localeResources;
    }

    public NexportDiscountPerCreditHoursPluginService(
        ILocalizationService localizationService,
        IPluginLocalizationService pluginLocalizationService,
        ISettingService settingService)
    {
        _localizationService = localizationService;
        _pluginLocalizationService = pluginLocalizationService;
        _settingService = settingService;
    }

    public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int lanugageId = 1)
    {
        // if this returns back false, then it didn't exist
        if (await _pluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue))
        {
            // this is if we have multiple modified values
            var discountPerCreditHourSetting =
                await _settingService.GetSettingAsync("Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources");
              
            if (discountPerCreditHourSetting == null)
            {
                await _settingService.SetSettingAsync<bool>("Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources", true);
            }
        }
    }

    public async Task AddOrUpdateResourcesAsync()
    {
        var discountPerCreditHourSetting =
            await _settingService.GetSettingAsync("Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources");

        if (discountPerCreditHourSetting != null)
        {
            await _settingService.DeleteSettingAsync(discountPerCreditHourSetting);
        }

        foreach (var localeResource in _localeResources)
        {
            await AddOrUpdateResourceAsync(localeResource.Key, localeResource.Value);
        }
    }

    public async Task DeleteResourcesAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync(_localeResources.Keys.ToList());
    }
}