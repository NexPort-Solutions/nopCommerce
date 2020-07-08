using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportSynchronizationTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly NexportService _nexportService;
        private readonly IRepository<NexportProductMapping> _nexportProductMappingRepository;

        private int _batchSize = 100;

        public NexportSynchronizationTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            IRepository<NexportProductMapping> nexportProductMappingRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _nexportProductMappingRepository = nexportProductMappingRepository;
            _nexportService = nexportService;
        }

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                var mappingIds = (from m in _nexportProductMappingRepository.Table
                    orderby m.UtcLastSynchronizationDate
                    select m.Id).Take(_batchSize).ToList();

                SynchronizeProductMappings(mappingIds);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize with Nexport", ex);
            }
        }

        public void SynchronizeProductMappings(IList<int> mappingIds)
        {
            try
            {
                foreach (var mappingId in mappingIds)
                {
                    _logger.Debug($"Begin synchronization process for product mapping {mappingId}");

                    _nexportService.SyncNexportProduct(mappingId);

                    _logger.Debug($"Synchronization process for product mapping {mappingId} has been completed!");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize mappings with Nexport", ex);
            }
        }
    }
}
