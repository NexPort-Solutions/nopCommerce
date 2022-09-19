using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Common;
using System.Linq;
using LinqToDB;
using Nop.Core.Domain.Localization;
using Nop.Data;

namespace Nop.Plugin.Misc.Nexport.Archway.Services
{
    public class ArchwayPluginService
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginLocalizationService _pluginLocalizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;

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

        public static Dictionary<string, string> GetLocaleResource()
        {
            return _localeResources;
        }

        public ArchwayPluginService(
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPluginLocalizationService pluginLocalizationService,
            ILogger logger)
        {
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _localizationService = localizationService;
            _pluginLocalizationService = pluginLocalizationService;
            _logger = logger;
        }

        public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int lanugageId = 1)
        {
            // if this returns back false, then it didn't exist
            if (await _pluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue))
            {
                // this is if we have multiple modified values
                var archwaySetting =
                    await _settingService.GetSettingAsync("Plugin.Misc.Nexport.Archway.HasModifiedLocaleResources");
              
                if (archwaySetting == null)
                {
                    await _settingService.SetSettingAsync<bool>("Plugin.Misc.Nexport.Archway.HasModifiedLocaleResources", true);
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
    }
}
