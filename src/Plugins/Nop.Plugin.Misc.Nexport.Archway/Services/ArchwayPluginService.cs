using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using Nop.Plugin.Misc.Nexport.Services;
using System.Linq;
using Nop.Core.Domain.Localization;
using Nop.Data;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public class ArchwayPluginService
{
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ILocalizationService _localizationService;
    private readonly INexportPluginLocalizationService _nexportPluginLocalizationService;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;

    private readonly IRepository<LocaleStringResource> _localeStringResourceRepository;

    private static readonly Dictionary<string, string> _localeResources = new()
    {
        {"Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey",
            "Store state field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey",
            "Store city field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey",
            "Store address field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey",
            "Store number field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey",
            "Store type field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey",
            "Employee Id field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey",
            "Employee position field key"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData",
            "Upload store data"},
        {"Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData.Hint",
            "Update the Archway store data by uploading a new store location record files. Only CSV file format is supported."},

        {"Plugins.Misc.Nexport.Archway.Field.StoreLocationState",
            "Restaurant State"},
        {"Plugins.Misc.Nexport.Archway.Field.StoreLocationCity",
            "Restaurant City"},
        {"Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress",
            "Restaurant Address"},
        {"Plugins.Misc.Nexport.Archway.Field.EmployeePosition",
            "Employee Position"},
        {"Plugins.Misc.Nexport.Archway.Field.EmployeeId",
            "Employee Id"},

        {"Plugins.Misc.Nexport.Archway.Errors.ModifiedLocaleResources",
            "There are modified resource values within the <b>Archway custom registration field plugin</b> that conflicts with the default value. Review them <a href=\"javascript:OpenWindow(\'{0}\', 800, 500, true)\">here</a>."}
    };

    public ArchwayPluginService(
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        ILocalizationService localizationService,
        ICustomerActivityService customerActivityService,
        INexportPluginLocalizationService nexportPluginLocalizationService,
        IRepository<LocaleStringResource> localeStringResourceRepository,
        ILogger logger)
    {
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
        _localizationService = localizationService;
        _nexportPluginLocalizationService = nexportPluginLocalizationService;
        _localeStringResourceRepository = localeStringResourceRepository;
        _logger = logger;
    }

    public Dictionary<string, string> GetLocaleResources()
    {
        return _localeResources;
    }

    public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int languageId = 1)
    {
        if (await _nexportPluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue, languageId))
        {
            var archwaySetting = await _settingService.GetSettingAsync("Plugin.Misc.Nexport.Archway.HasModifiedLocaleResources");

            if (archwaySetting == null)
            {
                await _settingService.SetSettingAsync("Plugin.Misc.Nexport.Archway.HasModifiedLocaleResources", true);
            }
        }
    }

    public async Task AddOrUpdateResourcesAsync()
    {
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