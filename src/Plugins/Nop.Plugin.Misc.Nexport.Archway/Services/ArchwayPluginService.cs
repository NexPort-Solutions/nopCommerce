using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Tasks;

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

        public void AddOrUpdateResources()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey",
                "Store state field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey",
                "Store city field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey",
                "Store address field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey",
                "Store number field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey",
                "Store type field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey",
                "Employee Id field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey",
                "Employee position field key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData",
                "Upload store data");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData.Hint",
                "Update the Archway store data by uploading a new store location record files. Only CSV file format is supported.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.StoreLocationState",
                "Restaurant State");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity",
                "Restaurant City");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress",
                "Restaurant Address");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.EmployeePosition",
                "Employee Position");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.EmployeeId",
                "Employee Id");
        }

        public void DeleteResources()
        {
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.StoreLocationState");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.EmployeePosition");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Archway.Field.EmployeeId");
        }
    }
}
