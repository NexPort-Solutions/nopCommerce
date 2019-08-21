using System;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Stores;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportSynchronizationTask : IScheduleTask
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ISettingService _settingService;
        private readonly NexportService _nexportService;
        private readonly IStoreService _storeService;
        private readonly NexportSettings _nexportSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        public NexportSynchronizationTask(
            IWidgetPluginManager widgetPluginManager,
            ILocalizationService localizationService,
            ILogger logger,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService,
            NexportService nexportService,
            NexportSettings nexportSettings)
        {
            _widgetPluginManager = widgetPluginManager;
            _localizationService = localizationService;
            _logger = logger;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
            _nexportService = nexportService;
            _nexportSettings = nexportSettings;
        }

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                //TODO: Sync job
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize with Nexport", ex);
            }
        }
    }
}
