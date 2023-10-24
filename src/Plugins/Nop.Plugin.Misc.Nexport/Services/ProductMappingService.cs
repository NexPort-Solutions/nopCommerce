using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Services.Logging;
using Nop.Plugin.Misc.Nexport.Extensions;
using static Nop.Plugin.Misc.Nexport.Services.CacheKey;
using Type = Nop.Plugin.Misc.Nexport.Domain.ProductType;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IProductMappingService
{
    Task Delete(ProductMapping mapping);
    Task DuplicateAsync(ProductMapping productMapping, int storeId);
    Task Insert(ProductMapping productMapping);
    Task Update(ProductMapping mapping);
    Task<IPagedList<ProductMapping>> GetAll(string? searchProductName, Type? searchProductType, string? searchStoreName, int productId, int pageIndex = 0, int pageSize = int.MaxValue);
    Task<ProductMapping?> GetById(int id);
    Task<ProductMapping?> GetByNopProductId(int nopProductId, int? storeId = null);
    Task<List<ProductMapping>> GetAll(int? nopProductId = null, int? storeId = null);

    // ?
    Task<Result<string>> SyncProductAsync(int mappingId, Product? product = null);

    // product
    Task<List<int?>> GetStoreIds(int productId);
    Task<bool> HasProductMapping(int productId);
    Task CopyProductMappingsAsync(Product originalProduct, Product copyingProduct);
    ProductMapping? FindProductCatalog(IList<ProductMapping> source, int productId, Guid catalogId, int? storeId = null);
    ProductMapping? FindProductSection(IList<ProductMapping> source, int productId, Guid sectionId, int? storeId = null);
    ProductMapping? FindProductTrainingPlan(IList<ProductMapping> source, int productId, Guid trainingPlanId, int? storeId = null);
    Task<IPagedList<ProductMapping>> GetProductCatalogsByCatalogId(Guid catalogId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    Task<List<ProductMapping>> GetAllByStoreId(int storeId);
    Task<IPagedList<ProductMapping>> GetProductSectionsBySectionId(Guid sectionId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    Task<IPagedList<ProductMapping>> GetProductTrainingPlansByTrainingPlanId(Guid trainingPlanId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    Task<bool> HasProductMapping(Order order);
    Task<bool> HasProductMappingForNopProduct(int nopProductId, Guid catalogId, Guid? syllabusId);
    Task<bool> HasProductMappingForStore(int nopProductId, int storeId);
    Task<Result<string>> MapProduct(MapProductModel model);
    Task<bool> HasDefaultMapping(int nopProductId);
    Task<int> FindMappingCountForSyllabus(Guid syllabusId);
    Task<Dictionary<Guid, int>> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList);
}

public class ProductMappingService : IProductMappingService
{
    private const string DEFAULT_STORE_NAME = "Default";
    private readonly IRepository<ProductMapping> _productMappings;
    private readonly IRepository<Product> _products;
    private readonly IRepository<Store> _stores;
    private readonly ISectionService _section;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IProductService _product;
    private readonly ICustomerService _customer;
    private readonly IOrderService _order;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICatalogService _catalog;
    private readonly ITrainingPlanService _trainingPlan;
    private readonly ILogger _logger;
    private readonly IProductGroupMembershipService _productGroupMembership;

    public ProductMappingService(
        IStaticCacheManager cacheManager,
        IProductService productService,
        IRepository<Product> productRepository,
        IRepository<ProductMapping> productMappings,
        ICustomerService customerService,
        IOrderService orderService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IRepository<Store> storeRepository,
        ISectionService section,
        ICatalogService catalog,
        ITrainingPlanService trainingPlan,
        ILogger logger,
        IProductGroupMembershipService productGroupMembership)
    {
        _cacheManager = cacheManager;
        _product = productService;
        _products = productRepository;
        _productMappings = productMappings;
        _customer = customerService;
        _order = orderService;
        _workContext = workContext;
        _storeContext = storeContext;
        _stores = storeRepository;
        _section = section;
        _catalog = catalog;
        _trainingPlan = trainingPlan;
        _logger = logger;
        _productGroupMembership = productGroupMembership;
    }

    public async Task<Dictionary<Guid, int>> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList)
    {
        var result = new Dictionary<Guid, int>();
        foreach (var item in syllabusList)
        {
            var count = await FindMappingCountForSyllabus(item.SyllabusId);
            result.Add(item.SyllabusId, count);
        }
        return result;
    }

    public async Task<Result<string>> SyncProductAsync(int mappingId, Product? product = null)
    {
        if (await GetById(mappingId) is not { } productMapping
            || (product ?? await _product.GetProductByIdAsync(productMapping.NopProductId)) is not { Id: var productId }
            || productMapping.NopProductId != productId)
        {
            return Error("Product mapping not found.");
        }
        return productMapping.Type switch
        {
            Type.Section or Type.TrainingPlan when productMapping.SyllabusId is null
                => Error($"Mappings for product type {nameof(Type.Section)} or {nameof(Type.TrainingPlan)} must specify a value for {nameof(productMapping.SyllabusId)}."),
            Type.Catalog when productMapping.CatalogId is null
                => Error($"{nameof(productMapping.CatalogId)} must have a value."),
            Type.Section when await _section.GetSectionDetails(productMapping.SyllabusId.Value) is { CreditHours: var creditHours, SectionCeus: var sectionCeus }
                => await finalize(creditHours, sectionCeus),
            Type.TrainingPlan when await _trainingPlan.GetTrainingPlanDetails(productMapping.SyllabusId.Value) is { CreditHours: var creditHours }
                => await finalize(creditHours, null),
            Type.Catalog when await _catalog.GetCatalogCreditHours(productMapping.CatalogId.Value) is { CreditHours: var creditHours }
                => await finalize(creditHours, null),
            _ => Error("Provided id was not found."),
        };
        async Task<Ok> finalize(decimal? creditHours, string? sectionCeus)
        {
            productMapping.CreditHours = creditHours;
            productMapping.SectionCeus = sectionCeus;
            productMapping.IsSynchronized = true;
            productMapping.UtcLastSynchronizationDate = DateTime.UtcNow;
            await Update(productMapping);
            await _logger.DebugAsync($"Successfully synchronized product {productMapping.NopProductId} with NexPort using the information from mapping {productMapping.Id}");
            return Okay();
        }
    }

    public async Task<IPagedList<ProductMapping>> GetProductCatalogsByCatalogId(Guid catalogId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(
            ProductMappingCatalogAllByCatalogId,
            showHidden,
            catalogId,
            pageIndex,
            pageSize,
            (await _workContext.GetCurrentCustomerAsync()).Id,
            (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(
            cacheKey,
            () =>
            {
                var productQuery = _products.Table
                    .Where(product => !product.Deleted && (showHidden || product.Published))
                    .Select(product => product.Id)
                    .ToList();
                var query = _productMappings.Table
                    .Where(productMapping => productMapping.CatalogId == catalogId
                        && productQuery.Contains(productMapping.NopProductId));
                return query.ToPagedListAsync(pageIndex, pageSize);
            });
    }

    public async Task<Result<string>> MapProduct(MapProductModel model)
    {
        if (await _product.GetProductByIdAsync(model.NopProductId) is not { } product)
        {
            return Error($"Product with id {model.NopProductId} not found.");
        }
        var update = false;
        if (await GetByNopProductId(product.Id, model.StoreId) is { } productMapping)
        {
            productMapping.Type = model.ProductType;
            update = true;
        }
        else
        {
            productMapping = new()
            {
                NopProductId = product.Id,
                DisplayName = product.Name,
                Type = model.ProductType,
            };
        }
        switch (model.ProductType)
        {
            case Type.Section when model.SyllabusId is { } syllabusId && await _section.GetSectionDetails(syllabusId) is { } sectionDetails:
            {
                productMapping.ProductName = sectionDetails.Title;
                productMapping.CatalogId = model.CatalogId;
                productMapping.SyllabusId = model.SyllabusId;
                productMapping.CatalogSyllabusLinkId = model.ProductId;
                productMapping.UtcAvailableDate = sectionDetails.EnrollmentStart;
                productMapping.UtcEndDate = sectionDetails.EnrollmentEnd;
                productMapping.UtcLastModifiedDate = sectionDetails.UtcDateLastModified;
                productMapping.CreditHours = sectionDetails.CreditHours;
                productMapping.SectionCeus = sectionDetails.SectionCeus;
                break;
            }
            case Type.TrainingPlan when model.SyllabusId is { } syllabusId && await _trainingPlan.GetTrainingPlanDetails(syllabusId) is { } trainingPlanDetails:
            {
                productMapping.ProductName = trainingPlanDetails.Name;
                productMapping.CatalogId = model.CatalogId;
                productMapping.SyllabusId = model.SyllabusId;
                productMapping.CatalogSyllabusLinkId = model.ProductId;
                productMapping.UtcAvailableDate = trainingPlanDetails.EnrollmentStart;
                productMapping.UtcEndDate = trainingPlanDetails.EnrollmentEnd;
                productMapping.UtcLastModifiedDate = trainingPlanDetails.UtcDateLastModified;
                productMapping.CreditHours = trainingPlanDetails.CreditHours;
                break;
            }
            case Type.Catalog when await _catalog.GetCatalogDetails(model.ProductId) is { } catalogDetails && await _catalog.GetCatalogCreditHours(model.ProductId) is { CreditHours: var creditHours }:
            {
                productMapping.ProductName = catalogDetails.Name;
                productMapping.CatalogId = model.CatalogId;
                productMapping.PricingModel = catalogDetails.PricingModel;
                productMapping.PublishingModel = catalogDetails.PublishingModel;
                productMapping.CreditHours = creditHours;
                break;
            }
            default:
                return Error($"Missing {nameof(model.SyllabusId)} or invalid product type {model.ProductType}.");
        }
        if (update)
        {
            await Update(productMapping);
        }
        else
        {
            await Insert(productMapping);
        }
        return Okay();
    }

    public async Task<IPagedList<ProductMapping>> GetProductSectionsBySectionId(
        Guid sectionId,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(
            ProductMappingSectionAllBySectionId,
            showHidden,
            sectionId,
            pageIndex,
            pageSize,
            (await _workContext.GetCurrentCustomerAsync()).Id,
            (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(
            cacheKey,
            () =>
            {
                var productQuery = (from p in _products.Table
                                    where !p.Deleted && (showHidden || p.Published)
                                    select p.Id)
                    .ToList();
                var query = from productMapping in _productMappings.Table
                            where productMapping.SyllabusId == sectionId
                                && productMapping.Type == Type.Section
                                && productQuery.Contains(productMapping.NopProductId)
                            select productMapping;
                return query.ToPagedListAsync(pageIndex, pageSize);
            });
    }

    public async Task<IPagedList<ProductMapping>> GetProductTrainingPlansByTrainingPlanId(
        Guid trainingPlanId,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        var key = _cacheManager.PrepareKeyForDefaultCache(
            ProductMappingTrainingPlanAllByTrainingPlanId,
            showHidden,
            trainingPlanId,
            pageIndex,
            pageSize,
            (await _workContext.GetCurrentCustomerAsync()).Id,
            (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(
            key,
            () =>
            {
                var productQuery = _products.Table.Where(product => !product.Deleted && (showHidden || product.Published))
                    .Select(product => product.Id)
                    .ToList();
                var query = _productMappings.Table
                    .Where(productMapping =>
                        productMapping.SyllabusId == trainingPlanId
                            && productMapping.Type == Type.TrainingPlan
                            && productQuery.Contains(productMapping.NopProductId));
                return query.ToPagedListAsync(pageIndex, pageSize);
            });
    }

    public ProductMapping? FindProductCatalog(
        IList<ProductMapping> source,
        int productId,
        Guid catalogId,
        int? storeId = null)
        => source.FirstOrDefault(
            productCatalog => productCatalog.NopProductId == productId
                && productCatalog.CatalogId == catalogId
                && productCatalog.StoreId == storeId);

    public ProductMapping? FindProductSection(
        IList<ProductMapping> source,
        int productId,
        Guid sectionId,
        int? storeId = null)
            => source.FirstOrDefault(productSectionMapping => productSectionMapping.NopProductId == productId
                && productSectionMapping.SyllabusId == sectionId
                && productSectionMapping.Type is Type.Section
                && productSectionMapping.StoreId == storeId);

    public ProductMapping? FindProductTrainingPlan(
        IList<ProductMapping> source,
        int productId,
        Guid trainingPlanId,
        int? storeId = null)
            => source.FirstOrDefault(productTrainingPlanMapping => productTrainingPlanMapping.NopProductId == productId
                && productTrainingPlanMapping.SyllabusId == trainingPlanId
                && productTrainingPlanMapping.Type is Type.TrainingPlan
                && productTrainingPlanMapping.StoreId == storeId);

    public Task<List<ProductMapping>> GetAllByStoreId(int storeId)
        => _productMappings.Table.Where(productMapping => productMapping.StoreId == storeId).ToListAsync();

    public Task<ProductMapping?> GetByNopProductId(int nopProductId, int? storeId = null)
        => _productMappings.Table.OnlyOneOrDefault(productMapping => productMapping.NopProductId == nopProductId && productMapping.StoreId == storeId)!; // OnlyOneOrDefault can return null.

    public async Task<List<ProductMapping>> GetAll(int? nopProductId = null, int? storeId = null)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var roleIds = await _customer.GetCustomerRoleIdsAsync(customer);
        var roleIdsCsv = string.Join(",", roleIds);
        var store = await _storeContext.GetCurrentStoreAsync();
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ProductMappingsAll, store.Id, roleIdsCsv, false, string.Empty, true);
        return await _cacheManager.GetAsync(
            cacheKey,
            () => ((nopProductId, storeId) switch
                {
                    (not null, _) => _productMappings.Table.Where(mapping => mapping.NopProductId == nopProductId),
                    (_, not null) => _productMappings.Table.Where(mapping => mapping.StoreId == storeId),
                    _ => _productMappings.Table
                })
                .ToListAsync());
    }

    public virtual async Task<IPagedList<ProductMapping>> GetAll(
        string? searchProductName,
        Type? searchProductType,
        string? searchStoreName,
        int productId,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var productMappings = _productMappings.Table.Where(productMapping => productMapping.NopProductId == productId);

        // this checks if a default mapping exists for the product.
        // if it does not then set insert empty default to true and if it doesnt get filtered out we insert it at the end.
        var insertEmptyDefault = await _productMappings.Table.AnyAsync(productMapping => productMapping.NopProductId == productId && productMapping.StoreId == null);
        var left = _stores.Table.Where(store => !store.Deleted)
            .GroupJoin(
                productMappings,
                store => store.Id,
                productMapping => productMapping.StoreId,
                (store, productMapping) => new { store, productMapping })
            .SelectMany(
                joined => joined.productMapping,
                (joined, productMapping) => new { joined.store, productMapping });
        var right = productMappings
            .GroupJoin(
                _stores.Table,
                productMapping => productMapping.StoreId,
                store => store.Id,
                (productMapping, store) => new { store, productMapping })
            .SelectMany(
                joined => joined.store,
                (joined, store) => new { store, joined.productMapping });
        var query = left.Union(right);
        if (!string.IsNullOrEmpty(searchStoreName))
        {
            if (DEFAULT_STORE_NAME.Equals(searchStoreName, StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(mapping => mapping.store.Name.Equals(DEFAULT_STORE_NAME, StringComparison.OrdinalIgnoreCase) || mapping.store.Name == null);
            }
            else
            {
                query = query.Where(mapping => mapping.store.Name.Contains(searchStoreName, StringComparison.OrdinalIgnoreCase));
                insertEmptyDefault = false;
            }
        }
        if (!string.IsNullOrEmpty(searchProductName))
        {
            query = query.Where(storeAndProductMapping => storeAndProductMapping.productMapping != null
                && storeAndProductMapping.productMapping.ProductName != null
                && storeAndProductMapping.productMapping.ProductName.Contains(searchProductName, StringComparison.OrdinalIgnoreCase));
            insertEmptyDefault = false;
        }
        if (searchProductType is not null)
        {
            query = query.Where(mapping => mapping.productMapping != null && mapping.productMapping.Type == searchProductType);
            insertEmptyDefault = false;
        }
        var productMappingsList = await query.Select(storeAndProductMapping => storeAndProductMapping.productMapping).ToListAsync();
        return new PagedList<ProductMapping>(productMappingsList, pageIndex, pageSize);
    }

    public async Task<bool> HasProductMapping(Order order)
        => await (await _order.GetOrderItemsAsync(order.Id))
            .WhereAwait(async orderItem => await _productMappings.Table.AnyAsync(productMapping => productMapping.NopProductId == orderItem.ProductId))
            .AnyAsync();

    public Task<bool> HasProductMapping(int productId)
        => _productMappings.Table.AnyAsync(productMapping => productMapping.NopProductId == productId);

    public Task<List<int?>> GetStoreIds(int productId)
        => _productMappings.Table
            .Where(productMapping => productMapping.NopProductId == productId)
            .Select(productMapping => productMapping.StoreId)
            .ToListAsync();

    public async Task CopyProductMappingsAsync(Product originalProduct, Product copyingProduct)
    {
        foreach (var mapping in await GetAll(originalProduct.Id))
        {
            var newMapping = new ProductMapping
            {
                NopProductId = copyingProduct.Id,
                ProductName = mapping.ProductName,
                DisplayName = mapping.DisplayName,
                CatalogId = mapping.CatalogId,
                SyllabusId = mapping.SyllabusId,
                CatalogSyllabusLinkId = mapping.CatalogSyllabusLinkId,
                SubscriptionOrgId = mapping.SubscriptionOrgId,
                Type = mapping.Type,
                PublishingModel = mapping.PublishingModel,
                PricingModel = mapping.PricingModel,
                AccessTimeLimit = mapping.AccessTimeLimit,
                UtcAvailableDate = mapping.UtcAvailableDate,
                UtcEndDate = mapping.UtcEndDate,
                UtcLastModifiedDate = DateTime.UtcNow,
                CreditHours = mapping.CreditHours,
                SectionCeus = mapping.SectionCeus,
                SubscriptionOrgName = mapping.SubscriptionOrgName,
                SubscriptionOrgShortName = mapping.SubscriptionOrgShortName,
                AutoRedeem = mapping.AutoRedeem,
                UtcAccessExpirationDate = mapping.UtcAccessExpirationDate,
                StoreId = mapping.StoreId,
                AllowExtension = mapping.AllowExtension,
                IsExtensionProduct = mapping.IsExtensionProduct,
                RenewalWindow = mapping.RenewalWindow,
            };
            await Insert(newMapping);
            var groupMembershipMappings = await _productGroupMembership.GetProductGroupMembershipMappings(mapping.Id);
            foreach (var groupMembershipMapping in groupMembershipMappings)
            {
                var newGroupMembershipMapping = new ProductGroupMembershipMapping
                {
                    GroupId = groupMembershipMapping.GroupId,
                    GroupName = groupMembershipMapping.GroupName,
                    GroupShortName = groupMembershipMapping.GroupShortName,
                    ProductMappingId = newMapping.Id,
                };
                await _productGroupMembership.InsertProductGroupMembershipMapping(newGroupMembershipMapping);
            }
        }
    }

    public async Task DuplicateAsync(ProductMapping productMapping, int storeId)
    {
        var newMapping = AutoMapperConfiguration.Mapper.Map<ProductMapping>(productMapping);
        newMapping.StoreId = storeId;
        await Insert(newMapping);
        var newMembershipMappings = (await _productGroupMembership.GetProductGroupMembershipMappings(productMapping.Id))
            .Select(mapping => new ProductGroupMembershipMapping
                {
                    GroupId = mapping.GroupId,
                    GroupName = mapping.GroupName,
                    GroupShortName = mapping.GroupShortName,
                    ProductMappingId = newMapping.Id,
                });
        foreach (var newMembershipMapping in newMembershipMappings)
        {
            await _productGroupMembership.InsertProductGroupMembershipMapping(newMembershipMapping);
        }
    }

    public Task<int> FindMappingCountForSyllabus(Guid syllabusId)
        => _productMappings.Table.Where(np => np.SyllabusId == syllabusId).CountAsync();

    public Task<bool> HasDefaultMapping(int nopProductId)
        => _productMappings.Table.AnyAsync(productMapping => productMapping.NopProductId == nopProductId && productMapping.StoreId == null);

    public Task Insert(ProductMapping productMapping)
    {
        if (!_productMappings.Table.Any(productMapping =>
            productMapping.NopProductId == productMapping.NopProductId
                && productMapping.StoreId == productMapping.StoreId))
        {
            return _productMappings.InsertAsync(productMapping);
        }
        return Task.CompletedTask;
    }

    public Task<bool> HasProductMappingForStore(int nopProductId, int storeId)
        => _productMappings.Table.AnyAsync(productMapping => productMapping.NopProductId == nopProductId && productMapping.StoreId == storeId);

    public Task<bool> HasProductMappingForNopProduct(int nopProductId, Guid catalogId, Guid? syllabusId)
        => _productMappings.Table.AnyAsync(productMapping => productMapping.NopProductId == nopProductId
            && productMapping.CatalogId == catalogId
            && productMapping.SyllabusId == syllabusId);

    public Task<ProductMapping?> GetById(int id)
        => _productMappings.GetByIdAsync(id)!; // GetByIdAsync can return null.

    public Task Delete(ProductMapping mapping)
        => _productMappings.DeleteAsync(mapping);

    public Task Update(ProductMapping mapping)
        => _productMappings.UpdateAsync(mapping);
}
