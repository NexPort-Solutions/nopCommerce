using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.ScheduleTasks;

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

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                var mappingIds = await _nexportProductMappingRepository.Table
                    .OrderBy(m => m.UtcLastSynchronizationDate)
                    .Select(m => m.Id)
                    .Take(_batchSize)
                    .ToListAsync();

                await SynchronizeProductMappingsAsync(mappingIds);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot synchronize with Nexport", ex);
            }
        }

        public async Task SynchronizeProductMappingsAsync(IList<int> mappingIds)
        {
            try
            {
                foreach (var mappingId in mappingIds)
                {
                    _logger.DebugAsync($"Begin synchronization process for product mapping {mappingId}");

                    await _nexportService.SyncNexportProductAsync(mappingId);

                    _logger.DebugAsync($"Synchronization process for product mapping {mappingId} has been completed!");
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot synchronize mappings with Nexport", ex);
            }
        }
    }
}
