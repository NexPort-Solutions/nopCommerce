using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class SynchronizationTask : IScheduleTask
{
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IRepository<ProductMapping> _productMappings;
    private readonly int _batchSize = 100;
    private readonly IProductMappingService _productMapping;

    public SynchronizationTask(
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        IRepository<ProductMapping> productMappingRepository,
        IProductMappingService productMapping)
    {
        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
        _productMappings = productMappingRepository;
        _productMapping = productMapping;
    }

    public async Task ExecuteAsync()
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync(SystemNames.SYSTEM_NAME))
        {
            return;
        }
        try
        {
            var mappingIds = await _productMappings.Table
                .OrderBy(productMapping => productMapping.UtcLastSynchronizationDate)
                .Select(productMapping => productMapping.Id)
                .Take(_batchSize)
                .ToListAsync();
            await SynchronizeProductMappingsAsync(mappingIds);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot synchronize with Nexport", exception);
        }
    }

    public async Task SynchronizeProductMappingsAsync(IList<int> mappingIds)
    {
        try
        {
            foreach (var mappingId in mappingIds)
            {
                await _logger.DebugAsync($"Begin synchronization process for product mapping {mappingId}");
                await _productMapping.SyncProductAsync(mappingId);
                await _logger.DebugAsync($"Synchronization process for product mapping {mappingId} has been completed!");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot synchronize mappings with Nexport", exception);
        }
    }
}
