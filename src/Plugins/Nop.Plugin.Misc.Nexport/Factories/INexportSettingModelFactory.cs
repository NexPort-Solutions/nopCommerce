using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Setting;

namespace Nop.Plugin.Misc.Nexport.Factories;

public interface INexportSettingModelFactory
{
    Task<NexportStoreSettingsModel> PrepareNexportStoreSettingsModelAsync(NexportStoreSettingsModel model = null);
}