using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;

public class NexportDiscountPerCreditHoursPluginService
{
    private readonly ILocalizationService _localizationService;
    private readonly INexportPluginLocalizationService _nexportPluginLocalizationService;
    private readonly ISettingService _settingService;

    private readonly IRepository<LocaleStringResource> _localeStringResourceRepository;

    private static readonly Dictionary<string, string> _localeResources = new()
    {
        {"Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours",
            "Credit hours"},
        {"Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours.Hint",
            "Minimum credit hours for the discount to be effective"},

        {"Plugins.Misc.Nexport.DiscountPerCreditHours.Errors.ModifiedLocaleResources",
            "There are modified resource values within the <b>Nexport discount per credit hours plugin</b> that conflicts with the default value. Review them <a href=\"javascript:OpenWindow(\'{0}\', 800, 500, true)\">here</a>."}
    };

    public NexportDiscountPerCreditHoursPluginService(
        ILocalizationService localizationService,
        INexportPluginLocalizationService nexportPluginLocalizationService,
        ISettingService settingService,
        IRepository<LocaleStringResource> localeStringResourceRepository)
    {
        _localizationService = localizationService;
        _nexportPluginLocalizationService = nexportPluginLocalizationService;
        _settingService = settingService;
        _localeStringResourceRepository = localeStringResourceRepository;
    }

    public Dictionary<string, string> GetLocaleResources()
    {
        return _localeResources;
    }

    public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int languageId = 1)
    {
        if (await _nexportPluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue, languageId))
        {
            var discountPerCreditHourSetting =
                await _settingService.GetSettingAsync("Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources");

            if (discountPerCreditHourSetting == null)
            {
                await _settingService.SetSettingAsync("Plugin.Misc.Nexport.DiscountPerCreditHours.HasModifiedLocaleResources", true);
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

    public async Task<List<LocaleStringResource>> GetConflictedLocalizedResourcesAsync()
    {
        var results = new List<LocaleStringResource>();

        foreach (var localeResource in GetLocaleResources())
        {
            var lsrList = await _localeStringResourceRepository
                .Table
                .WhereAwait(async l => l.ResourceName == localeResource.Key && l.ResourceValue != localeResource.Value)
                .ToListAsync();

            results.AddRange(lsrList);
        }

        return results;
    }
}