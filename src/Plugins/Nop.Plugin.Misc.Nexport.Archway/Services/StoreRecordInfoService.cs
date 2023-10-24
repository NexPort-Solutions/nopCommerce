using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public interface IStoreRecordInfoService
{
    Task<StoreRecordInfo> GetById(int id);
    Task<StoreRecordInfo> GetByNumber(int storeNumber);
    Task<List<StoreRecordInfo>> GetAll();
    Task InsertOrUpdate(StoreRecordInfo record);
    Task Delete(StoreRecordInfo record);
}

public class StoreRecordInfoService : IStoreRecordInfoService
{
    private readonly IRepository<StoreRecordInfo> _storeRecord;
    private readonly IStaticCacheManager _cacheManager;

    public StoreRecordInfoService(IRepository<StoreRecordInfo> storeRecord, IStaticCacheManager cacheManager)
    {
        _storeRecord = storeRecord;
        _cacheManager = cacheManager;
    }

    public Task<StoreRecordInfo> GetById(int id) => _storeRecord.GetByIdAsync(id);
    public Task<StoreRecordInfo> GetByNumber(int storeNumber) => _storeRecord.Table.FirstOrDefaultAsync(s => s.StoreNumber == storeNumber);

    public Task<List<StoreRecordInfo>> GetAll()
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(PluginDefaults.ArchwayStoreRecordAllNoPaginationCacheKey);
        return _cacheManager.GetAsync(cacheKey, () => _storeRecord.Table.ToListAsync());
    }

    public async Task InsertOrUpdate(StoreRecordInfo record)
    {
        var currentRecord = await GetByNumber(record.StoreNumber);
        if (currentRecord is null)
        {
            await _storeRecord.InsertAsync(record);
            return;
        }
        var newRecord = new StoreRecordInfo
        {
            Id = currentRecord.Id,
            StoreNumber = record.StoreNumber,
            OperatorId = record.OperatorId,
            RegionCode = record.RegionCode,
            Address = record.Address,
            City = record.City,
            State = record.State,
            PostalCode = record.PostalCode,
            AdvertisingCoop = record.AdvertisingCoop,
            StoreType = record.StoreType,
            OperatorFirstName = record.OperatorFirstName,
            OperatorLastName = record.OperatorLastName,
        };
        await _storeRecord.UpdateAsync(newRecord);
    }

    public Task Delete(StoreRecordInfo record) => _storeRecord.DeleteAsync(record);
}
