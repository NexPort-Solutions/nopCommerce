using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

#pragma warning disable RCS0056 // A line is too long.

public class PluginService
{
    private readonly ILocalizationService _localizationService;

    public PluginService(ILocalizationService localizationService) => _localizationService = localizationService;

    public async Task AddOrUpdateResourcesAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey", "Store state field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey", "Store city field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey", "Store address field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey", "Store number field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey", "Store type field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey", "Employee GroupGuid field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey", "Employee position field key");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData", "Upload store data");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Fields.Options.UploadStoreData.Hint", "Update the Archway store data by uploading a new store location record files. Only CSV file format is supported.");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationState", "Restaurant State");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity", "Restaurant City");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress", "Restaurant Address");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.EmployeePosition", "Employee Position");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.Archway.Field.EmployeeId", "Employee GroupGuid");
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
