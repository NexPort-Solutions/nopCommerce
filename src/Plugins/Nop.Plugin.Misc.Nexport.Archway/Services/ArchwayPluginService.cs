using System.Threading.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.Nexport.Archway.Services
{
    public class ArchwayPluginService
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public ArchwayPluginService(
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            ILogger logger)
        {
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
            _logger = logger;
        }

        public async Task AddOrUpdateResourcesAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey",
                "Store state field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey",
                "Store city field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey",
                "Store address field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey",
                "Store number field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey",
                "Store type field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey",
                "Employee Id field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey",
                "Employee position field key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData",
                "Upload store data");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData.Hint",
                "Update the Archway store data by uploading a new store location record files. Only CSV file format is supported.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationState",
                "Restaurant State");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity",
                "Restaurant City");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress",
                "Restaurant Address");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.EmployeePosition",
                "Employee Position");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.EmployeeId",
                "Employee Id");
        }

        public async Task DeleteResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData.Hint");

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationState");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.EmployeePosition");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.EmployeeId");
        }
    }
}
