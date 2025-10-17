using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services;

public class NexportStoreService(IRepository<Store> storeRepository) : StoreService(storeRepository)
{
    public override async Task<IList<Store>> GetAllStoresAsync()
    {
        return await _storeRepository.GetAllAsync(query =>
        {
            var result = from s in query orderby s.Name, s.DisplayOrder, s.Id select s;
            return result;
        }, includeDeleted: false);
    }
}