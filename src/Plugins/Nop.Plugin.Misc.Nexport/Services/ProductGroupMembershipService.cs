using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IProductGroupMembershipService
{
    Task DeleteGroupMembershipMapping(ProductGroupMembershipMapping mapping);
    Task<List<Guid>> GetProductGroupMembershipIds(int productMappingId);
    Task<ProductGroupMembershipMapping?> GetProductGroupMembershipMappingById(int mappingId);
    Task<List<ProductGroupMembershipMapping>> GetProductGroupMembershipMappings(int productMappingId);
    Task<IPagedList<ProductGroupMembershipMapping>> GetProductGroupMembershipMappingsPagination(int productMappingId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    Task InsertProductGroupMembershipMapping(ProductGroupMembershipMapping productGroupMembershipMapping);
}

public class ProductGroupMembershipService : IProductGroupMembershipService
{
    private readonly IStaticCacheManager _cacheManager;
    private readonly IRepository<ProductGroupMembershipMapping> _productGroupMembershipMappings;
    private readonly ICustomerService _customer;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    public ProductGroupMembershipService(
        IStaticCacheManager cacheManager,
        IRepository<ProductGroupMembershipMapping> productGroupMembershipMappings,
        ICustomerService customer,
        IWorkContext workContext,
        IStoreContext storeContext)
    {
        _cacheManager = cacheManager;
        _productGroupMembershipMappings = productGroupMembershipMappings;
        _customer = customer;
        _workContext = workContext;
        _storeContext = storeContext;
    }

    public async Task InsertProductGroupMembershipMapping(ProductGroupMembershipMapping productGroupMembershipMapping)
    {
        if (_productGroupMembershipMappings.Table.Any(
            productMapping => productMapping.ProductMappingId == productGroupMembershipMapping.ProductMappingId
                && productMapping.GroupId == productGroupMembershipMapping.GroupId))
        {
            return;
        }
        await _productGroupMembershipMappings.InsertAsync(productGroupMembershipMapping);
    }

    public async Task<List<ProductGroupMembershipMapping>> GetProductGroupMembershipMappings(int productMappingId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var roleIds = await _customer.GetCustomerRoleIdsAsync(customer);
        var roleIdsCsv = string.Join(",", roleIds);
        var store = await _storeContext.GetCurrentStoreAsync();
        var key = _cacheManager.PrepareKeyForDefaultCache(CacheKey.ProductGroupMembershipMappingsAll, store.Id, roleIdsCsv, false, string.Empty, false);
        return await _cacheManager.GetAsync(key, acquire);

        Task<List<ProductGroupMembershipMapping>> acquire()
            => _productGroupMembershipMappings.Table
            .Where(membershipMapping => membershipMapping.ProductMappingId == productMappingId)
            .ToListAsync();
    }

    public Task<IPagedList<ProductGroupMembershipMapping>> GetProductGroupMembershipMappingsPagination(int productMappingId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        => _productGroupMembershipMappings.Table
            .Where(mapping => mapping.ProductMappingId == productMappingId)
            .ToPagedListAsync(pageIndex, pageSize);

    public Task<List<Guid>> GetProductGroupMembershipIds(int productMappingId)
        => _productGroupMembershipMappings.Table
            .Where(groupMembershipMapping => groupMembershipMapping.ProductMappingId == productMappingId)
            .Select(membershipMapping => membershipMapping.GroupId)
            .ToListAsync();

    public Task<ProductGroupMembershipMapping?> GetProductGroupMembershipMappingById(int mappingId)
        => _productGroupMembershipMappings.GetByIdAsync(mappingId)!; // GetByIdAsync can return null.

    public Task DeleteGroupMembershipMapping(ProductGroupMembershipMapping mapping) => _productGroupMembershipMappings.DeleteAsync(mapping);
}
