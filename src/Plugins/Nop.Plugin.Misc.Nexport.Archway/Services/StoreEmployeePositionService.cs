using System.Data;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public interface IStoreEmployeePositionService
{
    Task<StoreEmployeePosition> GetById(int id);
    Task<List<StoreEmployeePosition>> GetAll(string jobType);
    Task Insert(StoreEmployeePosition position);
    Task Update(StoreEmployeePosition position);
    Task Delete(StoreEmployeePosition position);
}

public class StoreEmployeePositionService : IStoreEmployeePositionService
{
    private readonly IRepository<StoreEmployeePosition> _storeEmployeePositions;
    private readonly IStaticCacheManager _cacheManager;

    public StoreEmployeePositionService(IRepository<StoreEmployeePosition> storeEmployeePositions, IStaticCacheManager cacheManager)
    {
        _storeEmployeePositions = storeEmployeePositions;
        _cacheManager = cacheManager;
    }

    public Task<StoreEmployeePosition> GetById(int id) => _storeEmployeePositions.GetByIdAsync(id);
    public Task<List<StoreEmployeePosition>> GetAll(string jobType)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(PluginDefaults.ArchwayStoreEmployeePositionAllNoPaginationCacheKey);
        if (string.IsNullOrWhiteSpace(jobType))
        {
            return _cacheManager.GetAsync(cacheKey, () => _storeEmployeePositions.Table.ToListAsync());
        }
        return _cacheManager.GetAsync(cacheKey, () => _storeEmployeePositions.Table.Where(p => p.JobType == jobType).ToListAsync());
    }

    public Task Insert(StoreEmployeePosition position) => _storeEmployeePositions.InsertAsync(position);
    public Task Update(StoreEmployeePosition position) => _storeEmployeePositions.UpdateAsync(position);
    public Task Delete(StoreEmployeePosition position) => _storeEmployeePositions.DeleteAsync(position);
}
