using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IStoreService : Nop.Services.Stores.IStoreService
{
    Task<IPagedList<Store>> GetAllStoresAsync(string storeName, string storeUrl, int pageIndex = 0, int pageSize = int.MaxValue, bool excludeDeleted = true);
    Task<string?> GetStoreNameAsync(int storeId);
}

public class StoreService : Nop.Services.Stores.StoreService, IStoreService
{
    private readonly IRepository<Store> _stores;

    public StoreService(IRepository<Store> stores)
        : base(stores) => _stores = stores;

    public override Task<IList<Store>> GetAllStoresAsync()
        => _stores.GetAllAsync(query => from store in query orderby store.Name, store.DisplayOrder, store.Id select store, _ => default);

    public virtual async Task<IPagedList<Store>> GetAllStoresAsync(string storeName, string storeUrl, int pageIndex = 0, int pageSize = int.MaxValue, bool excludeDeleted = true)
    {
        var stores = await _stores.GetAllAsync(query =>
        {
            if (excludeDeleted)
            {
                query = query.Where(store => !store.Deleted);
            }
            if (!string.IsNullOrEmpty(storeName))
            {
                query = query.Where(store => store.Name.Contains(storeName));
            }
            if (!string.IsNullOrEmpty(storeUrl))
            {
                query = query.Where(store => store.Url.Contains(storeUrl));
            }
            return query;
        });
        return new PagedList<Store>(stores, pageIndex, pageSize);
    }

    public async Task<string?> GetStoreNameAsync(int storeId) => await GetStoreByIdAsync(storeId) is { Name: var name } ? name : null;
}
