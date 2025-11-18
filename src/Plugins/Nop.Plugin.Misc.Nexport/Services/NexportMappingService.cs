using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Infrastructure.CustomExceptions;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.Nexport.Services;

public partial class NexportService : INexportService
{
    public async Task InsertNexportProductMapping(NexportProductMapping nexportProductMapping)
    {
        if (nexportProductMapping == null)
            throw new ArgumentNullException(nameof(nexportProductMapping));

        if (_nexportProductMappingRepository.Table.Any(m =>
            m.NopProductId == nexportProductMapping.NopProductId &&
            m.StoreId == nexportProductMapping.StoreId))
            return;

        await _nexportProductMappingRepository.InsertAsync(nexportProductMapping);
    }

    public async Task InsertNexportProductGroupMembershipMapping(
        NexportProductGroupMembershipMapping nexportProductGroupMembershipMapping)
    {
        if (nexportProductGroupMembershipMapping == null)
            throw new ArgumentNullException(nameof(nexportProductGroupMembershipMapping));

        if (_nexportProductGroupMembershipMappingRepository.Table.Any(
            m => m.NexportProductMappingId == nexportProductGroupMembershipMapping.NexportProductMappingId &&
            m.NexportGroupId == nexportProductGroupMembershipMapping.NexportGroupId))
            return;

        await _nexportProductGroupMembershipMappingRepository.InsertAsync(nexportProductGroupMembershipMapping);
    }

    public async Task<IPagedList<NexportProductMapping>> GetProductCatalogsByCatalogId(Guid catalogId,
        int pageIndex = 0, int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        if (catalogId == Guid.Empty)
        {
            return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
        }

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.ProductMappingCatalogAllByCatalogIdCacheKey,
            showHidden, catalogId, pageIndex, pageSize, (await _workContext.GetCurrentCustomerAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            //var query = from np in _nexportProductMappingRepository.Table
            //    join p in _productRepository.Table on np.NopProductId equals p.Id
            //    where np.NexportCatalogId == catalogId && !p.Deleted && (showHidden || p.Published)
            //    select np;

            var productQuery = _productRepository.Table
                .Where(p => !p.Deleted && (showHidden || p.Published))
                .Select(p => p.Id).ToList();

            var query = _nexportProductMappingRepository.Table
                .Where(np => np.NexportCatalogId == catalogId && productQuery.Contains(np.NopProductId));
            //var query = from pc in _productCategoryRepository.Table
            //            join p in _productRepository.Table on pc.ProductId equals p.Id
            //            where pc.CategoryId == categoryId &&
            //                  !p.Deleted &&
            //                  (showHidden || p.Published)
            //            orderby pc.DisplayOrder, pc.Id
            //            select pc;
            if (!showHidden && (!_nexportSettings.IgnoreAcl || !_nexportSettings.IgnoreStoreLimitations))
            {
                //if (!_nexportSettings.IgnoreAcl)
                //{
                //    //ACL (access control list)
                //    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                //    query = from pc in query
                //            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                //            join acl in _aclRepository.Table
                //            on new { c1 = c.Id, c2 = _entityName } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                //            from acl in c_acl.DefaultIfEmpty()
                //            where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                //            select pc;
                //}

                //if (!_nexportSettings.IgnoreStoreLimitations)
                //{
                //    //Store mapping
                //    var currentStoreId = _storeContext.CurrentStore.Id;
                //    query = from pc in query
                //            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                //            join sm in _storeMappingRepository.Table
                //            on new { c1 = c.Id, c2 = _entityName } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                //            from sm in c_sm.DefaultIfEmpty()
                //            where !c.LimitedToStores || currentStoreId == sm.StoreId
                //            select pc;
                //}

                //query = query.Distinct().OrderBy(pc => pc.DisplayOrder).ThenBy(pc => pc.Id);
                //query = query.Distinct();
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task<IPagedList<NexportProductMapping>> GetProductSectionsBySectionId(Guid sectionId, int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        if (sectionId == Guid.Empty)
        {
            return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
        }

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.ProductMappingSectionAllBySectionIdCacheKey,
            showHidden, sectionId, pageIndex, pageSize, (await _workContext.GetCurrentCustomerAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var productQuery = (from p in _productRepository.Table
                                where !p.Deleted && (showHidden || p.Published)
                                select p.Id).ToList();

            var query = from np in _nexportProductMappingRepository.Table
                        where np.NexportSyllabusId == sectionId &&
                              np.Type == NexportProductTypeEnum.Section &&
                              productQuery.Contains(np.NopProductId)
                        select np;

            if (!showHidden && (!_nexportSettings.IgnoreAcl || !_nexportSettings.IgnoreStoreLimitations))
            {
                //if (!_nexportSettings.IgnoreAcl)
                //{
                //    //ACL (access control list)
                //    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                //    query = from pc in query
                //            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                //            join acl in _aclRepository.Table
                //            on new { c1 = c.Id, c2 = _entityName } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                //            from acl in c_acl.DefaultIfEmpty()
                //            where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                //            select pc;
                //}

                //if (!_nexportSettings.IgnoreStoreLimitations)
                //{
                //    //Store mapping
                //    var currentStoreId = _storeContext.CurrentStore.Id;
                //    query = from pc in query
                //            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                //            join sm in _storeMappingRepository.Table
                //            on new { c1 = c.Id, c2 = _entityName } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                //            from sm in c_sm.DefaultIfEmpty()
                //            where !c.LimitedToStores || currentStoreId == sm.StoreId
                //            select pc;
                //}

                //query = query.Distinct().OrderBy(pc => pc.DisplayOrder).ThenBy(pc => pc.Id);
                //query = query.Distinct();
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task<IPagedList<NexportProductMapping>> GetProductTrainingPlansByTrainingPlanId(Guid trainingPlanId,
        int pageIndex = 0, int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        if (trainingPlanId == Guid.Empty)
        {
            return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
        }

        var key = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.ProductMappingTrainingPlanAllByTrainingPlanIdCacheKey,
            showHidden, trainingPlanId, pageIndex, pageSize, (await _workContext.GetCurrentCustomerAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(key, async () =>
        {
            var productQuery =
                (_productRepository.Table.Where(p => !p.Deleted && (showHidden || p.Published))
                    .Select(p => p.Id)).ToList();

            var query = _nexportProductMappingRepository.Table
                .Where(np =>
                    np.NexportSyllabusId == trainingPlanId &&
                    np.Type == NexportProductTypeEnum.TrainingPlan &&
                    productQuery.Contains(np.NopProductId));

            if (!showHidden && (!_nexportSettings.IgnoreAcl || !_nexportSettings.IgnoreStoreLimitations))
            {
                //if (!_nexportSettings.IgnoreAcl)
                //{
                //    //ACL (access control list)
                //    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                //    query = from pc in query
                //            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                //            join acl in _aclRepository.Table
                //            on new { c1 = c.Id, c2 = _entityName } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                //            from acl in c_acl.DefaultIfEmpty()
                //            where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                //            select pc;
                //}

                //if (!_nexportSettings.IgnoreStoreLimitations)
                //{
                //    //Store mapping
                //    var currentStoreId = _storeContext.CurrentStore.Id;
                //    query = from pc in query
                //            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                //            join sm in _storeMappingRepository.Table
                //            on new { c1 = c.Id, c2 = _entityName } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                //            from sm in c_sm.DefaultIfEmpty()
                //            where !c.LimitedToStores || currentStoreId == sm.StoreId
                //            select pc;
                //}

                //query = query.Distinct().OrderBy(pc => pc.DisplayOrder).ThenBy(pc => pc.Id);
                //query = query.Distinct();
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public NexportProductMapping FindProductCatalog(IList<NexportProductMapping> source, int productId,
        Guid catalogId, int? storeId = null)
    {
        return source.FirstOrDefault(productCatalog => productCatalog.NopProductId == productId &&
                                                       productCatalog.NexportCatalogId == catalogId &&
                                                       productCatalog.StoreId == storeId);
    }

    public NexportProductMapping FindProductSection(IList<NexportProductMapping> source, int productId,
        Guid sectionId, int? storeId = null)
    {
        return source.FirstOrDefault(productSectionMapping => productSectionMapping.NopProductId == productId &&
                                                       productSectionMapping.NexportSyllabusId == sectionId &&
                                                       productSectionMapping.Type == NexportProductTypeEnum.Section &&
                                                       productSectionMapping.StoreId == storeId);
    }

    public NexportProductMapping FindProductTrainingPlan(IList<NexportProductMapping> source,
        int productId, Guid trainingPlanId, int? storeId = null)
    {
        return source.FirstOrDefault(productTrainingPlanMapping => productTrainingPlanMapping.NopProductId == productId &&
                                                       productTrainingPlanMapping.NexportSyllabusId == trainingPlanId &&
                                                       productTrainingPlanMapping.Type == NexportProductTypeEnum.TrainingPlan &&
                                                       productTrainingPlanMapping.StoreId == storeId);
    }

    public async Task<IList<NexportProductMapping>> GetProductMappingsByStoreId(int storeId)
    {
        return storeId < 1
            ? new List<NexportProductMapping>()
            : await _nexportProductMappingRepository.Table.Where(np => np.StoreId == storeId).ToListAsync();
    }

    public async Task<NexportProductMapping> GetProductMappingByNopProductId(int nopProductId, int? storeId = null)
    {
        return nopProductId < 1
            ? null
            : await _nexportProductMappingRepository
                .Table.SingleOrDefaultAsync(np =>
                    np.NopProductId == nopProductId &&
                    np.StoreId == storeId);
    }

    public async Task<IList<NexportProductMapping>> GetProductMappings(int? nopProductId = null, int? storeId = null)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.ProductMappingsAllCacheKey,
            (await _storeContext.GetCurrentStoreAsync()).Id,
            string.Join(",", await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync())),
            false, "", true);

        return _cacheManager.Get(cacheKey, () =>
        {
            var query = _nexportProductMappingRepository.Table;

            if (nopProductId != null)
                query = query.Where(np => np.NopProductId == nopProductId);

            if (storeId != null)
                query = query.Where(np => np.StoreId == storeId);

            return query.ToList();
        });
    }

    public async Task<IList<NexportProductGroupMembershipMapping>> GetProductGroupMembershipMappings(
        int nexportProductMappingId)
    {
        if (nexportProductMappingId < 1)
            return new List<NexportProductGroupMembershipMapping>();

        var key = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.GroupMembershipMappingsByNexportProductMappingIdCacheKey, nexportProductMappingId);

        return await _cacheManager.GetAsync(key, async () =>
        {
            return await _nexportProductGroupMembershipMappingRepository.Table.Where(np =>
                np.NexportProductMappingId == nexportProductMappingId).ToListAsync();
        });
    }

    public async Task<IPagedList<NexportProductGroupMembershipMapping>> GetProductGroupMembershipMappingsPagination(
        int nexportProductMappingId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        if (nexportProductMappingId < 1)
            return new PagedList<NexportProductGroupMembershipMapping>(new List<NexportProductGroupMembershipMapping>(), pageIndex, pageSize);

        var query = _nexportProductGroupMembershipMappingRepository.Table.Where(np =>
            np.NexportProductMappingId == nexportProductMappingId);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<IList<Guid>> GetProductGroupMembershipIds(int nexportProductMappingId)
    {
        return nexportProductMappingId < 1
            ? new List<Guid>()
            : await _nexportProductGroupMembershipMappingRepository.Table.Where(np =>
                    np.NexportProductMappingId == nexportProductMappingId)
                .Select(g => g.NexportGroupId).ToListAsync();
    }

    public async Task<Dictionary<Guid, int>> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList)
    {
        var result = new Dictionary<Guid, int>();
        foreach (var item in syllabusList)
        {
            var count = await FindMappingCountPerSyllabi(item.SyllabusId);
            result.Add(item.SyllabusId, count);
        }

        return result;
    }

    public async Task<int> FindMappingCountPerSyllabi(Guid syllabusId)
    {
        return await (from np in _nexportProductMappingRepository.Table
                      where np.NexportSyllabusId == syllabusId
                      select np.Id).CountAsync();
    }

    public async Task<bool> HasDefaultMapping(int nopProductId)
    {
        return nopProductId > 0 &&
               await _nexportProductMappingRepository.Table.AnyAsync(np =>
                   np.NopProductId == nopProductId &&
                   np.StoreId == null);
    }

    public async Task<bool> HasProductMappingForStore(int nopProductId, int storeId)
    {
        if (nopProductId <= 0 || storeId <= 0)
            return false;

        return await _nexportProductMappingRepository.Table.AnyAsync(np =>
            np.NopProductId == nopProductId && np.StoreId == storeId);
    }

    public async Task<bool> HasProductMappingForNopProduct(int nopProductId, Guid catalogId, Guid? syllabusId)
    {
        return await _nexportProductMappingRepository.Table.AnyAsync(np =>
            np.NopProductId == nopProductId && np.NexportCatalogId == catalogId && np.NexportSyllabusId == syllabusId);
    }

    public async Task<NexportProductMapping> GetProductMappingById(int mappingId)
    {
        return mappingId == 0 ? null : await _nexportProductMappingRepository.GetByIdAsync(mappingId);
    }

    public async Task DeleteNexportProductMapping(NexportProductMapping mapping)
    {
        if (mapping == null)
            throw new ArgumentNullException(nameof(mapping));

        await _nexportProductMappingRepository.DeleteAsync(mapping);
    }

    public async Task UpdateNexportProductMapping(NexportProductMapping mapping)
    {
        if (mapping == null)
            throw new ArgumentNullException(nameof(mapping));

        await _nexportProductMappingRepository.UpdateAsync(mapping);
    }

    public async Task<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappingById(int mappingId)
    {
        return mappingId == 0
            ? null
            : await _nexportProductGroupMembershipMappingRepository.GetByIdAsync(mappingId);
    }

    public async Task DeleteGroupMembershipMapping(NexportProductGroupMembershipMapping mapping)
    {
        if (mapping == null)
            throw new ArgumentNullException(nameof(mapping));

        await _nexportProductGroupMembershipMappingRepository.DeleteAsync(mapping);
    }

    public async Task InsertNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        if (await _nexportOrderProcessingQueueRepository.Table.AnyAsync(q => q.OrderId == queueItem.OrderId))
            return;

        await _logger.InformationAsync($"Order {queueItem.OrderId} has been added to the processing queue and awaiting to be processed.");
        await _nexportOrderProcessingQueueRepository.InsertAsync(queueItem);
    }

    public async Task DeleteNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportOrderProcessingQueueRepository.DeleteAsync(queueItem);
    }

    public async Task<bool> InsertOrUpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (await _nexportOrderInvoiceItemRepository.Table.AnyAsync(q => q.OrderId == item.OrderId &&
                                                                         q.OrderItemId == item.OrderItemId && q.InvoiceItemId == item.InvoiceItemId))
        {
            try
            {
                var currentInvoiceItems = await FindNexportOrderInvoiceItems(item.OrderId, item.OrderItemId);
                NexportOrderInvoiceItem invoiceToUpdate = null;
                if (currentInvoiceItems != null)
                {
                    foreach (var invoiceItem in currentInvoiceItems)
                    {
                        if (invoiceItem.UtcDateRedemption != null)
                        {
                            invoiceToUpdate = invoiceItem;
                            break;
                        }
                    }
                }

                if (invoiceToUpdate != null)
                {
                    invoiceToUpdate.InvoiceItemId = item.InvoiceItemId;
                    await _nexportOrderInvoiceItemRepository.UpdateAsync(invoiceToUpdate);
                }
                else
                {
                    await _nexportOrderInvoiceItemRepository.InsertAsync(item);
                }

                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot update new invoice item for the order item {item.OrderItemId} in order {item.OrderId}", ex);
                return false;
            }
        }
        else
        {
            try
            {
                await _nexportOrderInvoiceItemRepository.InsertAsync(item);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot add new Nexport order invoice item for the order item {item.OrderItemId} in order {item.OrderId}", ex);
                return false;
            }
        }
    }

    public async Task InsertNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        if (_nexportOrderInvoiceRedemptionQueueRepository.Table.Any(q => q.OrderInvoiceItemId == queueItem.OrderInvoiceItemId))
            return;

        await _nexportOrderInvoiceRedemptionQueueRepository.InsertAsync(queueItem);
        await _logger.InformationAsync($"Invoice redemption {queueItem.OrderInvoiceItemId} for user {queueItem.RedeemingUserId} has been scheduled.");
    }

    public async Task InsertNexportOrderInvoiceResetRedemptionQueueItem(NexportOrderInvoiceResetRedemptionQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        if (_nexportOrderInvoiceResetRedemptionQueueRepository.Table.Any(q => q.OrderInvoiceItemId == queueItem.OrderInvoiceItemId))
            return;

        await _nexportOrderInvoiceResetRedemptionQueueRepository.InsertAsync(queueItem);
        await _logger.InformationAsync($"Invoice redemption reset for invoice item with id: {queueItem.OrderInvoiceItemId} has been scheduled.");
    }

    public async Task DeleteNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        await _nexportOrderInvoiceItemRepository.DeleteAsync(item);
    }

    public async Task UpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        await _nexportOrderInvoiceItemRepository.UpdateAsync(item);
    }

    public async Task DeleteNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportOrderInvoiceRedemptionQueueRepository.DeleteAsync(queueItem);
    }

    public async Task DeleteNexportOrderInvoiceResetRedemptionQueueItem(NexportOrderInvoiceResetRedemptionQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportOrderInvoiceResetRedemptionQueueRepository.DeleteAsync(queueItem);
    }

    public async Task UpdateNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportOrderInvoiceRedemptionQueueRepository.UpdateAsync(queueItem);
    }

    public async Task UpdateNexportOrderInvoiceResetRedemptionQueueItem(NexportOrderInvoiceResetRedemptionQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportOrderInvoiceResetRedemptionQueueRepository.UpdateAsync(queueItem);
    }

    //TODO @js - this function needs to be fixed or go away. does not work for wholesale because more than one invoice can have the same order id and orderitem id
    public async Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItem(int orderId, int orderItemId)
    {
        if (orderId < 1)
            return null;

        return orderItemId < 1
            ? null
            : await _nexportOrderInvoiceItemRepository
                .Table.SingleOrDefaultAsync(o => o.OrderId == orderId && o.OrderItemId == orderItemId);
    }


    public async Task<IList<NexportOrderInvoiceItem>> FindNexportOrderInvoiceItems(int orderId, int orderItemId)
    {
        if (orderId < 1)
            return null;

        return orderItemId < 1
            ? null
            : await _nexportOrderInvoiceItemRepository
                .Table.Where(o => o.OrderId == orderId && o.OrderItemId == orderItemId).ToListAsync();
    }

    public async Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItemById(int orderInvoiceItemId)
    {
        return orderInvoiceItemId < 1
            ? null
            : await _nexportOrderInvoiceItemRepository.GetByIdAsync(orderInvoiceItemId);
    }

    public async Task<IList<NexportOrderInvoiceItem>> GetNexportOrderInvoiceItems(Guid userId)
    {
        return userId == Guid.Empty
            ? new List<NexportOrderInvoiceItem>()
            : await _nexportOrderInvoiceItemRepository.Table
                .Where(o => o.RedeemingUserId == userId)
                .ToListAsync();
    }

    public async Task<NexportOrderInvoiceItem> GetNexportOrderInvoiceItem(Guid userId, Guid enrollmentId)
    {
        return userId == Guid.Empty
            ? null
            : await _nexportOrderInvoiceItemRepository.Table
                .FirstOrDefaultAsync(o => o.RedeemingUserId == userId && o.RedemptionEnrollmentId == enrollmentId);
    }

    //public async Task<IList<NexportOrderInvoiceItem>> GetNexportOrderInvoiceItems(int orderId)
    //{
    //    return orderId < 1
    //        ? []
    //        : await _nexportOrderInvoiceItemRepository.Table
    //            .Where(o => o.OrderId == orderId)
    //            .ToListAsync();
    //}

    public async Task<IPagedList<NexportOrderInvoiceItem>> GetNexportOrderInvoiceItems(int orderId,
        bool excludeNonApproval = false,
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        if (orderId < 1)
            return new PagedList<NexportOrderInvoiceItem>(new List<NexportOrderInvoiceItem>(), pageIndex, pageSize);

        var redemptionQueueQuery = _nexportOrderInvoiceRedemptionQueueRepository.Table
            .Select(q => q.OrderItemId).ToList();

        var query = _nexportOrderInvoiceItemRepository.Table
            .Where(o => o.OrderId == orderId);

        if (excludeNonApproval)
            query = query.Where(x =>
                x.RequireManualApproval.HasValue &&
                x.RequireManualApproval.Value &&
                !redemptionQueueQuery.Contains(x.OrderItemId));

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task MapNexportProduct(MapNexportProductModel model)
    {
        //get selected products
        var product = await _productService.GetProductByIdAsync(model.NopProductId);
        if (product != null)
        {
            var updateProductMapping = false;

            var productMapping = await GetProductMappingByNopProductId(product.Id, model.StoreId);
            if (productMapping != null)
            {
                productMapping.Type = model.NexportProductType;
                updateProductMapping = true;
            }
            else
            {
                productMapping = new NexportProductMapping
                {
                    NopProductId = product.Id,
                    DisplayName = product.Name,
                    Type = model.NexportProductType
                };
            }

            productMapping.AssignWhenRedeemed = false;
            productMapping.NopCategoryId = null;

            switch (model.NexportProductType)
            {
                case NexportProductTypeEnum.OpenEnded:
                    var openCatalogDetails = await GetCatalogDetailsAsync(model.NexportProductId);
                    var openCatalogCreditHours = await GetCatalogCreditHoursAsync(model.NexportProductId);

                    productMapping.NexportProductName = openCatalogDetails?.Name;
                    productMapping.NexportCatalogId = model.NexportCatalogId;
                    productMapping.PricingModel = openCatalogDetails?.PricingModel;
                    productMapping.PublishingModel = openCatalogDetails?.PublishingModel;
                    productMapping.CreditHours = openCatalogCreditHours?.CreditHours;
                    if (model.AssignWhenRedeemed.HasValue && model.AssignWhenRedeemed.Value)
                        productMapping.AssignWhenRedeemed = model.AssignWhenRedeemed.Value;

                    break;

                case NexportProductTypeEnum.Catalog:
                    var catalogDetails = await GetCatalogDetailsAsync(model.NexportProductId);
                    var catalogCreditHours = await GetCatalogCreditHoursAsync(model.NexportProductId);

                    productMapping.NexportProductName = catalogDetails?.Name;
                    productMapping.NexportCatalogId = model.NexportCatalogId;
                    productMapping.PricingModel = catalogDetails?.PricingModel;
                    productMapping.PublishingModel = catalogDetails?.PublishingModel;
                    productMapping.CreditHours = catalogCreditHours?.CreditHours;

                    break;

                case NexportProductTypeEnum.Section:
                    if (model.NexportSyllabusId == null)
                        throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                    var sectionDetails = await GetSectionDetailsAsync(model.NexportSyllabusId.Value);

                    productMapping.NexportProductName = sectionDetails?.Title;
                    productMapping.NexportCatalogId = model.NexportCatalogId;
                    productMapping.NexportSyllabusId = model.NexportSyllabusId;
                    productMapping.NexportCatalogSyllabusLinkId = model.NexportProductId;
                    productMapping.UtcAvailableDate = sectionDetails?.EnrollmentStart;
                    productMapping.UtcEndDate = sectionDetails?.EnrollmentEnd;
                    productMapping.UtcLastModifiedDate = sectionDetails?.UtcDateLastModified;
                    productMapping.CreditHours = sectionDetails?.CreditHours;
                    productMapping.SectionCeus = sectionDetails?.SectionCeus;

                    break;

                case NexportProductTypeEnum.TrainingPlan:
                    if (model.NexportSyllabusId == null)
                        throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                    var trainingPlanDetails = await GetTrainingPlanDetailsAsync(model.NexportSyllabusId.Value);

                    productMapping.NexportProductName = trainingPlanDetails?.Name;
                    productMapping.NexportCatalogId = model.NexportCatalogId;
                    productMapping.NexportSyllabusId = model.NexportSyllabusId;
                    productMapping.NexportCatalogSyllabusLinkId = model.NexportProductId;
                    productMapping.UtcAvailableDate = trainingPlanDetails?.EnrollmentStart;
                    productMapping.UtcEndDate = trainingPlanDetails?.EnrollmentEnd;
                    productMapping.UtcLastModifiedDate = trainingPlanDetails?.UtcDateLastModified;
                    productMapping.CreditHours = trainingPlanDetails?.CreditHours;

                    break;

                default:
                    goto case NexportProductTypeEnum.Catalog;
            }

            if (model.StoreId.HasValue)
            {
                productMapping.StoreId = model.StoreId.Value;
            }

            if (updateProductMapping)
            {
                await UpdateNexportProductMapping(productMapping);
            }
            else
            {
                await InsertNexportProductMapping(productMapping);
            }
        }
    }

    public async Task InsertUserMapping(NexportUserMapping nexportUserMapping)
    {
        if (nexportUserMapping == null)
            throw new ArgumentNullException(nameof(nexportUserMapping));

        var existingMapping = _nexportUserMappingRepository
            .Table
            .FirstOrDefault(user => user.NexportUserId == nexportUserMapping.NexportUserId);

        if (existingMapping != null)
            throw new NexportUserMappingException(
                $"The Nexport user Id {nexportUserMapping.NexportUserId} has been mapped with another users!",
                existingMapping);
        if (await _nexportUserMappingRepository.Table.AnyAsync(user => user.NopUserId == nexportUserMapping.NopUserId))
            return;

        await _nexportUserMappingRepository.InsertAsync(nexportUserMapping);
    }

    public async Task DeleteUserMapping(NexportUserMapping nexportUserMapping)
    {
        if (nexportUserMapping == null)
            throw new ArgumentNullException(nameof(nexportUserMapping));

        await _nexportUserMappingRepository.DeleteAsync(nexportUserMapping);
    }

    public async Task UpdateUserMapping(NexportUserMapping nexportUserMapping)
    {
        if (nexportUserMapping == null)
            throw new ArgumentNullException(nameof(nexportUserMapping));

        await _nexportUserMappingRepository.UpdateAsync(nexportUserMapping);
    }

    public async Task<NexportUserMapping> FindUserMappingByCustomerId(int nopCustomerId)
    {
        return nopCustomerId < 1
            ? null
            : await _nexportUserMappingRepository.Table.SingleOrDefaultAsync(np => np.NopUserId == nopCustomerId);
    }

    public async Task<NexportUserMapping> FindUserMappingByNexportUserId(Guid userId)
    {
        return userId == Guid.Empty
            ? null
            : await _nexportUserMappingRepository.Table.SingleOrDefaultAsync(np => np.NexportUserId == userId);
    }

    public async Task<Guid?> FindExistingInvoiceForOrder(int orderId)
    {
        return orderId < 1
            ? null
            : (await _nexportOrderInvoiceItemRepository.Table
                .FirstOrDefaultAsync(i => i.OrderId == orderId))?.InvoiceId;
    }

    public async Task<Guid?> FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId)
    {
        if (orderId < 1)
            return null;

        return orderItemId <= 0
            ? null
            : (await _nexportOrderInvoiceItemRepository.Table.FirstOrDefaultAsync(i =>
                i.OrderId == orderId && i.OrderItemId == orderItemId))?.InvoiceItemId;

    }

    public async Task<bool> HasNexportOrderProcessingQueueItem(int orderId)
    {
        return orderId > 0 &&
               await _nexportOrderProcessingQueueRepository.Table.AnyAsync(q => q.OrderId == orderId);
    }

    public async Task<bool> HasNexportProductMapping(Order order)
    {
        if (order == null)
            return false;

        var items = (await _orderService.GetOrderItemsAsync(order.Id));

        return await items.WhereAwait(async t =>
            await _nexportProductMappingRepository
                .Table.AnyAsync(p => p.NopProductId == t.ProductId))
            .AnyAsync();
    }

    public async Task<bool> HasNexportProductMapping(int productId)
    {
        return productId > 0 && await _nexportProductMappingRepository.Table.AnyAsync(p => p.NopProductId == productId);
    }

    public async Task<IList<int?>> GetStoreIdsPerProductMapping(int productId)
    {
        return productId < 1
            ? new List<int?>()
            : await _nexportProductMappingRepository.Table
                .Where(np => np.NopProductId == productId)
                .Select(np => np.StoreId)
                .ToListAsync();
    }

    public async Task CopyProductMappingsAsync(Product originalProduct, Product copyingProduct)
    {
        if (originalProduct == null)
            throw new ArgumentNullException(nameof(originalProduct));

        if (copyingProduct == null)
            throw new ArgumentNullException(nameof(copyingProduct));

        var mappings = await GetProductMappings(originalProduct.Id);
        foreach (var mapping in mappings)
        {
            var newMapping = new NexportProductMapping
            {
                NopProductId = copyingProduct.Id,
                NexportProductName = mapping.NexportProductName,
                DisplayName = mapping.DisplayName,
                NexportCatalogId = mapping.NexportCatalogId,
                NexportSyllabusId = mapping.NexportSyllabusId,
                NexportCatalogSyllabusLinkId = mapping.NexportCatalogSyllabusLinkId,
                NexportSubscriptionOrgId = mapping.NexportSubscriptionOrgId,
                Type = mapping.Type,
                PublishingModel = mapping.PublishingModel,
                PricingModel = mapping.PricingModel,
                AccessTimeLimit = mapping.AccessTimeLimit,
                UtcAvailableDate = mapping.UtcAvailableDate,
                UtcEndDate = mapping.UtcEndDate,
                //UtcLastModifiedDate = DateTime.UtcNow,
                CreditHours = mapping.CreditHours,
                SectionCeus = mapping.SectionCeus,
                NexportSubscriptionOrgName = mapping.NexportSubscriptionOrgName,
                NexportSubscriptionOrgShortName = mapping.NexportSubscriptionOrgShortName,
                AutoRedeem = mapping.AutoRedeem,
                UtcAccessExpirationDate = mapping.UtcAccessExpirationDate,
                StoreId = mapping.StoreId,
                AllowExtension = mapping.AllowExtension,
                IsExtensionProduct = mapping.IsExtensionProduct,
                RenewalWindow = mapping.RenewalWindow
            };

            await InsertNexportProductMapping(newMapping);

            var groupMembershipMappings = await GetProductGroupMembershipMappings(mapping.Id);
            foreach (var groupMembershipMapping in groupMembershipMappings)
            {
                var newGroupMembershipMapping = new NexportProductGroupMembershipMapping
                {
                    NexportGroupId = groupMembershipMapping.NexportGroupId,
                    NexportGroupName = groupMembershipMapping.NexportGroupName,
                    NexportGroupShortName = groupMembershipMapping.NexportGroupShortName,
                    NexportProductMappingId = newMapping.Id
                };

                await InsertNexportProductGroupMembershipMapping(newGroupMembershipMapping);
            }
        }
    }

    public async Task DuplicateProductMappingAsync(NexportProductMapping productMapping, int storeId)
    {
        var newMapping = AutoMapperConfiguration.Mapper.Map<NexportProductMapping>(productMapping);
        newMapping.StoreId = storeId;

        await InsertNexportProductMapping(newMapping);

        var groupMembershipMappings = await GetProductGroupMembershipMappings(productMapping.Id);
        foreach (var groupMembershipMapping in groupMembershipMappings)
        {
            var newGroupMembershipMapping = new NexportProductGroupMembershipMapping
            {
                NexportGroupId = groupMembershipMapping.NexportGroupId,
                NexportGroupName = groupMembershipMapping.NexportGroupName,
                NexportGroupShortName = groupMembershipMapping.NexportGroupShortName,
                NexportProductMappingId = newMapping.Id
            };

            await InsertNexportProductGroupMembershipMapping(newGroupMembershipMapping);
        }
    }

    public async Task<IPagedList<NexportSupplementalInfoQuestion>> GetAllNexportSupplementalInfoQuestionsPagination(
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.SupplementalInfoQuestionAllCacheKey, pageIndex, pageSize);
        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = _nexportSupplementalInfoQuestionRepository.Table.Select(question => question);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task<IList<NexportSupplementalInfoQuestion>> GetAllNexportSupplementalInfoQuestions()
    {
        return (await GetAllNexportSupplementalInfoQuestionsPagination()).ToList();
    }

    public async Task<NexportSupplementalInfoQuestion> GetNexportSupplementalInfoQuestionById(int questionId)
    {
        return questionId > 0 ? await _nexportSupplementalInfoQuestionRepository.GetByIdAsync(questionId) : null;
    }

    public async Task<IList<NexportSupplementalInfoQuestion>> GetNexportSupplementalInfoQuestionsByIds(int[] questionIds)
    {
        if (questionIds == null || questionIds.Length == 0)
            return new List<NexportSupplementalInfoQuestion>();

        var questions = await _nexportSupplementalInfoQuestionRepository
            .Table
            .Where(question => questionIds.Contains(question.Id))
            .ToListAsync();

        return await questionIds
            .Select(id => questions.Find(x => x.Id == id))
            .Where(question => question != null)
            .ToListAsync();
    }

    public async Task InsertNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        await _nexportSupplementalInfoQuestionRepository.InsertAsync(question);
    }

    public async Task DeleteNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        await _nexportSupplementalInfoQuestionRepository.DeleteAsync(question);
    }

    public async Task DeleteNexportSupplementalInfoQuestions(IList<NexportSupplementalInfoQuestion> questions)
    {
        if (questions == null)
            throw new ArgumentNullException(nameof(questions));

        foreach (var question in questions)
        {
            await DeleteNexportSupplementalInfoQuestion(question);
        }
    }

    public async Task UpdateNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        question.UtcDateModified = DateTime.UtcNow;

        await _nexportSupplementalInfoQuestionRepository.UpdateAsync(question);
    }

    public async Task<NexportSupplementalInfoOption> GetNexportSupplementalInfoOptionById(int optionId)
    {
        return optionId > 0 ? await _nexportSupplementalInfoOptionRepository.GetByIdAsync(optionId) : null;
    }

    public async Task<IList<NexportSupplementalInfoOption>> GetNexportSupplementalInfoOptionsByQuestionId(int questionId,
        bool showHidden = false)
    {
        if (questionId <= 0)
            return new List<NexportSupplementalInfoOption>();

        var query = _nexportSupplementalInfoOptionRepository.Table
            .Where(opt => opt.QuestionId == questionId);

        return showHidden
            ? await query.ToListAsync()
            : await query.Where(opt => !opt.Deleted).ToListAsync();
    }

    public async Task InsertNexportSupplementalInfoOption(NexportSupplementalInfoOption option)
    {
        if (option == null)
            throw new ArgumentNullException(nameof(option));

        await _nexportSupplementalInfoOptionRepository.InsertAsync(option);
    }

    public async Task DeleteNexportSupplementalInfoOption(NexportSupplementalInfoOption option)
    {
        if (option == null)
            throw new ArgumentNullException(nameof(option));

        option.Deleted = true;

        await UpdateNexportSupplementalInfoOption(option);
    }

    public async Task UpdateNexportSupplementalInfoOption(NexportSupplementalInfoOption option)
    {
        if (option == null)
            throw new ArgumentNullException(nameof(option));

        option.UtcDateModified = DateTime.UtcNow;

        await _nexportSupplementalInfoOptionRepository.UpdateAsync(option);
    }

    public async Task<NexportSupplementalInfoQuestionMapping> GetNexportSupplementalInfoQuestionMappingById(int questionMappingId)
    {
        return questionMappingId > 0
            ? await _nexportSupplementalInfoQuestionMappingRepository.GetByIdAsync(questionMappingId)
            : null;
    }

    public async Task<IList<NexportSupplementalInfoQuestionMapping>>
        GetNexportSupplementalInfoQuestionMappingsByProductMappingId(int nexportProductMappingId)
    {
        return nexportProductMappingId > 0
            ? await _nexportSupplementalInfoQuestionMappingRepository
                .Table
                .Where(qm => qm.ProductMappingId == nexportProductMappingId)
                .ToListAsync()
            : new List<NexportSupplementalInfoQuestionMapping>();
    }

    public async Task<NexportSupplementalInfoQuestionMapping> GetNexportSupplementalInfoQuestionMapping(int nexportProductMappingId,
        int questionId)
    {
        if (nexportProductMappingId < 1 || questionId < 1)
            return null;

        return await _nexportSupplementalInfoQuestionMappingRepository
            .Table
            .FirstOrDefaultAsync(qm =>
                qm.ProductMappingId == nexportProductMappingId &&
                qm.QuestionId == questionId);
    }

    public async Task InsertNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping)
    {
        if (questionMapping == null)
            throw new ArgumentNullException(nameof(questionMapping));

        if (await _nexportSupplementalInfoQuestionMappingRepository.Table.AnyAsync(qm =>
            qm.QuestionId == questionMapping.QuestionId &&
            qm.ProductMappingId == questionMapping.ProductMappingId))
            return;

        await _nexportSupplementalInfoQuestionMappingRepository.InsertAsync(questionMapping);
    }

    public async Task DeleteNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping)
    {
        if (questionMapping == null)
            throw new ArgumentNullException(nameof(questionMapping));

        await _nexportSupplementalInfoQuestionMappingRepository.DeleteAsync(questionMapping);
    }

    public async Task UpdateNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping)
    {
        if (questionMapping == null)
            throw new ArgumentNullException(nameof(questionMapping));

        await _nexportSupplementalInfoQuestionMappingRepository.UpdateAsync(questionMapping);
    }

    public async Task<IPagedList<NexportSupplementalInfoOptionGroupAssociation>>
        GetNexportSupplementalInfoOptionGroupAssociationsPagination(int optionId, int pageIndex = 0,
            int pageSize = int.MaxValue)
    {
        if (optionId < 1)
            return new PagedList<NexportSupplementalInfoOptionGroupAssociation>(
                new List<NexportSupplementalInfoOptionGroupAssociation>(), pageIndex, pageSize);

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.SupplementalInfoOptionGroupAssociationsAllCacheKey,
            (await _storeContext.GetCurrentStoreAsync()).Id);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query =
                _nexportSupplementalInfoOptionGroupAssociationRepository
                    .Table.Where(ga => ga.OptionId == optionId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task<IList<NexportSupplementalInfoOptionGroupAssociation>>
        GetNexportSupplementalInfoOptionGroupAssociations(int optionId, bool excludeInactive = false)
    {
        if (optionId < 1)
            return new List<NexportSupplementalInfoOptionGroupAssociation>();

        var query = _nexportSupplementalInfoOptionGroupAssociationRepository.Table
            .Where(a => a.OptionId == optionId);

        if (excludeInactive)
            query = query.Where(a => a.IsActive);

        return await query.ToListAsync();
    }

    public async Task<NexportSupplementalInfoOptionGroupAssociation> GetNexportSupplementalInfoOptionGroupAssociationById(int groupAssociationId)
    {
        return groupAssociationId < 1
            ? null
            : await _nexportSupplementalInfoOptionGroupAssociationRepository.GetByIdAsync(groupAssociationId);
    }

    public async Task InsertNexportSupplementalInfoOptionGroupAssociation(NexportSupplementalInfoOptionGroupAssociation groupAssociation)
    {
        if (groupAssociation == null)
            throw new ArgumentNullException(nameof(groupAssociation));

        if (await _nexportSupplementalInfoOptionGroupAssociationRepository.Table.AnyAsync(ga =>
            ga.OptionId == groupAssociation.OptionId &&
            ga.NexportGroupId == groupAssociation.NexportGroupId))
            return;

        await _nexportSupplementalInfoOptionGroupAssociationRepository.InsertAsync(groupAssociation);
    }

    public async Task DeleteNexportSupplementalInfoOptionGroupAssociation(NexportSupplementalInfoOptionGroupAssociation groupAssociation)
    {
        if (groupAssociation == null)
            throw new ArgumentNullException(nameof(groupAssociation));

        await _nexportSupplementalInfoOptionGroupAssociationRepository.DeleteAsync(groupAssociation);
    }

    public async Task UpdateNexportSupplementalInfoOptionGroupAssociation(NexportSupplementalInfoOptionGroupAssociation groupAssociation)
    {
        if (groupAssociation == null)
            throw new ArgumentNullException(nameof(groupAssociation));

        await _nexportSupplementalInfoOptionGroupAssociationRepository.UpdateAsync(groupAssociation);
    }

    public async Task InsertNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer)
    {
        if (answer == null)
            throw new ArgumentNullException(nameof(answer));

        if (await _nexportSupplementalInfoAnswerRepository.Table.AnyAsync(a =>
            a.CustomerId == answer.CustomerId &&
            a.OptionId == answer.OptionId &&
            a.QuestionId == answer.QuestionId &&
            a.StoreId == answer.StoreId))
            return;

        await _nexportSupplementalInfoAnswerRepository.InsertAsync(answer);
    }

    public async Task DeleteNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer)
    {
        if (answer == null)
            throw new ArgumentNullException(nameof(answer));

        await _nexportSupplementalInfoAnswerRepository.DeleteAsync(answer);
    }

    public async Task UpdateNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer)
    {
        if (answer == null)
            throw new ArgumentNullException(nameof(answer));

        await _nexportSupplementalInfoAnswerRepository.UpdateAsync(answer);
    }

    public async Task<IList<NexportSupplementalInfoAnswer>> GetNexportSupplementalInfoAnswers(int customerId, int storeId,
        int? questionId = null)
    {
        if (customerId < 1 || storeId < 1)
            return new List<NexportSupplementalInfoAnswer>();

        var query = _nexportSupplementalInfoAnswerRepository.Table
            .Where(a => a.CustomerId == customerId && a.StoreId == storeId);

        if (questionId != null)
        {
            query = query.Where(a => a.QuestionId == questionId);
        }

        return await query.ToListAsync();
    }

    public async Task<IPagedList<NexportSupplementalInfoAnswer>> GetNexportSupplementalInfoAnswersPagination(
        int customerId, int? questionId = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        if (customerId < 1)
            return new PagedList<NexportSupplementalInfoAnswer>(
                new List<NexportSupplementalInfoAnswer>(), pageIndex, pageSize);

        return await _cacheManager.GetAsync(NexportIntegrationDefaults.SupplementalInfoAnswerAllCacheKey, async () =>
        {
            var query =
                _nexportSupplementalInfoAnswerRepository
                    .Table.Where(a => a.CustomerId == customerId);

            if (questionId != null)
                query = query.Where(a => a.QuestionId == questionId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task<NexportSupplementalInfoAnswer> GetNexportSupplementalInfoAnswerById(int answerId)
    {
        return answerId < 1
            ? null
            : await _nexportSupplementalInfoAnswerRepository.GetByIdAsync(answerId);
    }

    public async Task<IPagedList<NexportSupplementalInfoQuestion>> GetNexportSupplementalInfoAnsweredQuestionsPagination(
        int customerId, int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        if (customerId < 1)
            return new PagedList<NexportSupplementalInfoQuestion>(
                new List<NexportSupplementalInfoQuestion>(), pageIndex, pageSize);

        var questionIdsQuery =
            _nexportSupplementalInfoAnswerRepository.Table
                .Where(a => a.CustomerId == customerId)
                .Select(x => x.QuestionId);
        var query = _nexportSupplementalInfoQuestionRepository.Table
            .Where(q => questionIdsQuery.Contains(q.Id));

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task InsertNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership)
    {
        if (answerMembership == null)
            throw new ArgumentNullException(nameof(answerMembership));

        if (await _nexportSupplementalInfoAnswerMembershipRepository.Table.AnyAsync(am =>
            am.AnswerId == answerMembership.AnswerId &&
            am.NexportMembershipId == answerMembership.NexportMembershipId))
            return;

        await _nexportSupplementalInfoAnswerMembershipRepository.InsertAsync(answerMembership);
    }

    public async Task DeleteNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership)
    {
        if (answerMembership == null)
            throw new ArgumentNullException(nameof(answerMembership));

        await _nexportSupplementalInfoAnswerMembershipRepository.DeleteAsync(answerMembership);
    }

    public async Task<IList<NexportSupplementalInfoAnswerMembership>>
        GetNexportSupplementalInfoAnswerMembershipsByAnswerId(int answerId)
    {
        return answerId < 1
            ? new List<NexportSupplementalInfoAnswerMembership>()
            : await _nexportSupplementalInfoAnswerMembershipRepository.Table
                .Where(am => am.AnswerId == answerId)
                .ToListAsync();
    }

    public async Task<NexportSupplementalInfoAnswerMembership> GetNexportSupplementalInfoAnswerMembership(Guid nexportMembershipId)
    {
        return await _nexportSupplementalInfoAnswerMembershipRepository.Table
                .FirstOrDefaultAsync(am => am.NexportMembershipId == nexportMembershipId);
    }

    public async Task InsertNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement)
    {
        if (requirement == null)
            throw new ArgumentNullException(nameof(requirement));

        if (await _nexportRequiredSupplementalInfoRepository.Table.AnyAsync(r =>
            r.CustomerId == requirement.CustomerId &&
            r.StoreId == requirement.StoreId &&
            r.QuestionId == requirement.QuestionId))
            return;

        await _nexportRequiredSupplementalInfoRepository.InsertAsync(requirement);
    }

    public async Task DeleteNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement)
    {
        if (requirement == null)
            throw new ArgumentNullException(nameof(requirement));

        await _nexportRequiredSupplementalInfoRepository.DeleteAsync(requirement);
    }

    public async Task<IList<NexportRequiredSupplementalInfo>> GetNexportRequiredSupplementalInfos(int customerId,
        int storeId, int? questionId = null)
    {
        if (customerId < 1 || storeId < 1)
            return new List<NexportRequiredSupplementalInfo>();

        var query = _nexportRequiredSupplementalInfoRepository.Table
            .Where(r => r.CustomerId == customerId && r.StoreId == storeId);

        if (questionId != null)
            query = query.Where(r => r.QuestionId == questionId);

        return await query.ToListAsync();
    }

    public async Task<NexportRequiredSupplementalInfo> GetNexportRequiredSupplementalInfoByNopProductId(int customerId,
        int storeId, int questionId)
    {
        if (customerId < 1 || storeId < 1 || questionId < 1)
            return null;

        return await _nexportRequiredSupplementalInfoRepository.Table
            .FirstOrDefaultAsync(r =>
                r.CustomerId == customerId &&
                r.StoreId == storeId &&
                r.QuestionId == questionId);
    }

    public async Task<bool> HasRequiredSupplementalInfo(int customerId, int storeId)
    {
        if (customerId < 1 || storeId < 1)
            return false;

        return await _nexportRequiredSupplementalInfoRepository.Table
            .AnyAsync(r =>
                r.CustomerId == customerId &&
                r.StoreId == storeId);
    }

    public async Task<bool> HasUnprocessedAnswer(int orderId)
    {
        if (orderId < 1)
            return false;

        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null || order.Deleted || order.OrderStatus != OrderStatus.Complete)
            return false;

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        var nexportProductMappings = await orderItems
            .SelectAwait(async item =>
                await GetProductMappingByNopProductId(item.ProductId, order.StoreId) ??
                await GetProductMappingByNopProductId(item.ProductId))
            .Where(mapping => mapping != null).ToListAsync();

        return await nexportProductMappings.SelectAwait(async t =>
                await (await GetNexportSupplementalInfoQuestionMappingsByProductMappingId(t.Id))
                    .Select(x => x.QuestionId)
                    .ToListAsync())
                    .SelectAwait(async questions =>
                        (await GetNexportSupplementalInfoAnswers(order.CustomerId, order.StoreId))
                            .Where(x =>
                                questions.Contains(x.QuestionId))
                            .Any(x => x.Status == NexportSupplementalInfoAnswerStatus.NotProcessed))
                                .FirstOrDefaultAsync();
    }

    public async Task InsertNexportSupplementalInfoAnswerProcessingQueueItem(NexportSupplementalInfoAnswerProcessingQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportSupplementalInfoAnswerProcessingQueueRepository.InsertAsync(queueItem);
    }

    public async Task DeleteNexportSupplementalInfoAnswerProcessingQueueItem(NexportSupplementalInfoAnswerProcessingQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportSupplementalInfoAnswerProcessingQueueRepository.DeleteAsync(queueItem);
    }

    public async Task InsertNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportGroupMembershipRemovalQueueRepository.InsertAsync(queueItem);
    }

    public async Task DeleteNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportGroupMembershipRemovalQueueRepository.DeleteAsync(queueItem);
    }

    public async Task<NexportRegistrationField> GetNexportRegistrationFieldById(int fieldId, int? categoryId = null)
    {
        if (fieldId < 1)
            return null;

        var query = _nexportRegistrationFieldRepository.Table.Where(rf => rf.Id == fieldId);

        if (categoryId != null)
            query = query.Where(rf => rf.FieldCategoryId == categoryId);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<NexportRegistrationField>> GetNexportRegistrationFields(int storeId)
    {
        if (storeId < 1)
            return new List<NexportRegistrationField>();

        var fieldStoreMappingsForCurrentStore = await _nexportRegistrationFieldStoreMappingRepository.Table
            .Where(rfs => rfs.StoreId == storeId)
            .Select(rfs => rfs.FieldId).ToListAsync();

        var fieldOStoreMappingsForOtherStores = await _nexportRegistrationFieldStoreMappingRepository.Table
            .Where(rfs => rfs.StoreId != storeId)
            .Select(rfs => rfs.FieldId).ToListAsync();

        var fields = _nexportRegistrationFieldRepository.Table
            .Where(f => f.IsActive &&
                        (fieldStoreMappingsForCurrentStore.Contains(f.Id) ||
                         !fieldOStoreMappingsForOtherStores.Contains(f.Id)));

        return await fields.ToListAsync();
    }

    public async Task<IList<NexportRegistrationField>> GetNexportRegistrationFieldsByCategoryId(int categoryId)
    {
        return categoryId < 1
            ? new List<NexportRegistrationField>()
            : await _nexportRegistrationFieldRepository.Table
                .Where(f => f.FieldCategoryId == categoryId).ToListAsync();
    }

    public async Task<IPagedList<NexportRegistrationField>> GetNexportRegistrationFieldsPagination(IList<int> storeIds, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _nexportRegistrationFieldRepository.Table;

        //check if contains 0 because nopselect puts 0 in list of ids if "all" is selected
        if (storeIds == null || storeIds.Contains(0))
            return await query.ToPagedListAsync(pageIndex, pageSize);

        var storeQuery = _storeRepository.Table
            .Where(s => storeIds.Contains(s.Id)).Select(s => s.Id).ToList();

        var storeMappingQuery = _nexportRegistrationFieldStoreMappingRepository.Table.Where(sm => storeQuery.Contains(sm.StoreId)).Select(sm => sm.FieldId).ToList();

        query = query.Where(f => storeMappingQuery.Contains(f.Id));

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<IPagedList<NexportRegistrationField>> GetNexportRegistrationFieldsWithAnswersPagination(
        int customerId, int? storeId = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var registrationFieldWithAnswerIds = await
            _nexportRegistrationFieldAnswerRepository
                .Table
                .Where(fa => fa.CustomerId == customerId)
                .GroupBy(fa => fa.FieldId)
                .Select(x => x.FirstOrDefault().FieldId)
                .ToListAsync();

        if (storeId != null)
        {
            registrationFieldWithAnswerIds = await registrationFieldWithAnswerIds.Where(id =>
                _nexportRegistrationFieldStoreMappingRepository.Table
                    .Where(mapping => mapping.StoreId == storeId)
                    .Select(mapping => mapping.FieldId).Contains(id) ||
                !(_nexportRegistrationFieldStoreMappingRepository.Table
                    .Where(mapping => mapping.FieldId == id)
                    .Select(mapping => mapping.FieldId)).Contains(id)).ToListAsync();
        }

        var query = _nexportRegistrationFieldRepository
            .Table
            .Where(f => registrationFieldWithAnswerIds.Contains(f.Id));

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<IList<NexportRegistrationField>> GetNexportRegistrationFieldsWithAnswers(int customerId,
        int? storeId = null)
    {
        var registrationFieldWithAnswerIds = await
            _nexportRegistrationFieldAnswerRepository
                .Table
                .Where(fa => fa.CustomerId == customerId)
                .GroupBy(fa => fa.FieldId)
                .Select(x => x.FirstOrDefault().FieldId)
                .ToListAsync();

        if (storeId != null)
        {
            var registrationFieldIdsByStore = await _nexportRegistrationFieldStoreMappingRepository
                .Table
                .Where(x => x.StoreId == storeId)
                .Select(x => x.FieldId)
                .ToListAsync();

            registrationFieldWithAnswerIds = registrationFieldWithAnswerIds.Intersect(registrationFieldIdsByStore).ToList();
        }

        var query = _nexportRegistrationFieldRepository
            .Table
            .Where(f => registrationFieldWithAnswerIds.Contains(f.Id));

        return await query.ToListAsync();
    }

    public async Task InsertNexportRegistrationField(NexportRegistrationField registrationField)
    {
        if (registrationField == null)
            throw new ArgumentNullException(nameof(registrationField));

        await _nexportRegistrationFieldRepository.InsertAsync(registrationField);
    }

    public async Task DeleteNexportRegistrationField(NexportRegistrationField registrationField)
    {
        if (registrationField == null)
            throw new ArgumentNullException(nameof(registrationField));

        await _nexportRegistrationFieldRepository.DeleteAsync(registrationField);
    }

    public async Task UpdateNexportRegistrationField(NexportRegistrationField registrationField)
    {
        if (registrationField == null)
            throw new ArgumentNullException(nameof(registrationField));

        await _nexportRegistrationFieldRepository.UpdateAsync(registrationField);
    }

    public async Task<NexportRegistrationFieldOption> GetNexportRegistrationFieldOptionById(int fieldOptionId, int? fieldId = null)
    {
        if (fieldOptionId < 1)
            return null;

        if (fieldId != null)
            return await _nexportRegistrationFieldOptionRepository.Table.SingleOrDefaultAsync(rfo =>
                rfo.Id == fieldOptionId && rfo.FieldId == fieldId);

        return await _nexportRegistrationFieldOptionRepository.GetByIdAsync(fieldOptionId);
    }

    public async Task<IList<NexportRegistrationFieldOption>> GetNexportRegistrationFieldOptions(int? fieldId = null)
    {
        var query = _nexportRegistrationFieldOptionRepository.Table;

        if (fieldId != null)
            query = query.Where(rfo => rfo.FieldId == fieldId);

        return await query.ToListAsync();
    }

    public async Task<IPagedList<NexportRegistrationFieldOption>> GetNexportRegistrationFieldOptionsPagination(
        int fieldId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        if (fieldId < 1)
            return new PagedList<NexportRegistrationFieldOption>(new List<NexportRegistrationFieldOption>(), pageIndex, pageSize);

        return await _cacheManager.GetAsync(NexportIntegrationDefaults.RegistrationFieldOptionAllCacheKey, async () =>
        {
            var query = _nexportRegistrationFieldOptionRepository.Table
                .Where(rfo => rfo.FieldId == fieldId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task InsertNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption)
    {
        if (registrationFieldOption == null)
            throw new ArgumentNullException(nameof(registrationFieldOption));

        await _nexportRegistrationFieldOptionRepository.InsertAsync(registrationFieldOption);
    }

    public async Task DeleteNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption)
    {
        if (registrationFieldOption == null)
            throw new ArgumentNullException(nameof(registrationFieldOption));

        await _nexportRegistrationFieldOptionRepository.DeleteAsync(registrationFieldOption);
    }

    public async Task UpdateNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption)
    {
        if (registrationFieldOption == null)
            throw new ArgumentNullException(nameof(registrationFieldOption));

        await _nexportRegistrationFieldOptionRepository.UpdateAsync(registrationFieldOption);
    }

    public async Task<NexportRegistrationFieldCategory> GetNexportRegistrationFieldCategoryById(int fieldCategoryId)
    {
        return fieldCategoryId < 1
            ? null
            : await _nexportRegistrationFieldCategoryRepository.GetByIdAsync(fieldCategoryId);
    }

    public async Task<IList<NexportRegistrationFieldCategory>> GetNexportRegistrationFieldCategories()
    {
        var query =
            _nexportRegistrationFieldCategoryRepository.Table
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Id);

        return await query.ToListAsync();
    }

    public async Task<IList<NexportRegistrationFieldCategory>> GetNexportRegistrationFieldCategories(IList<int> fieldCategoryIds)
    {
        return await fieldCategoryIds
            .SelectAwait(async fieldCategoryId =>
                await GetNexportRegistrationFieldCategoryById(fieldCategoryId))
            .ToListAsync();
    }

    public async Task<IPagedList<NexportRegistrationFieldCategory>> GetNexportRegistrationFieldCategoriesPagination(
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _cacheManager.GetAsync(NexportIntegrationDefaults.RegistrationFieldCategoryAllCacheKey, async () =>
        {
            var query = _nexportRegistrationFieldCategoryRepository.Table;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task InsertNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory)
    {
        if (registrationFieldCategory == null)
            throw new ArgumentNullException(nameof(registrationFieldCategory));

        await _nexportRegistrationFieldCategoryRepository.InsertAsync(registrationFieldCategory);
    }

    public async Task DeleteNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory)
    {
        if (registrationFieldCategory == null)
            throw new ArgumentNullException(nameof(registrationFieldCategory));

        await _nexportRegistrationFieldCategoryRepository.DeleteAsync(registrationFieldCategory);
    }

    public async Task UpdateNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory)
    {
        if (registrationFieldCategory == null)
            throw new ArgumentNullException(nameof(registrationFieldCategory));

        await _nexportRegistrationFieldCategoryRepository.UpdateAsync(registrationFieldCategory);
    }

    public async Task<NexportRegistrationFieldStoreMapping> GetNexportRegistrationFieldStoreMappingById(int fieldStoreMappingId)
    {
        return fieldStoreMappingId < 1
            ? null
            : await _nexportRegistrationFieldStoreMappingRepository.GetByIdAsync(fieldStoreMappingId);
    }

    public async Task<IList<NexportRegistrationFieldStoreMapping>> GetNexportRegistrationFieldStoreMappings(int fieldId)
    {
        if (fieldId < 1)
            return new List<NexportRegistrationFieldStoreMapping>();

        return await _nexportRegistrationFieldStoreMappingRepository.Table
            .Where(rf => rf.FieldId == fieldId)
            .ToListAsync();
    }

    public async Task InsertNexportRegistrationFieldStoreMapping(NexportRegistrationFieldStoreMapping registrationFieldStoreMapping)
    {
        if (registrationFieldStoreMapping == null)
            throw new ArgumentNullException(nameof(registrationFieldStoreMapping));

        if (await _nexportRegistrationFieldStoreMappingRepository.Table.AnyAsync(sm =>
            sm.FieldId == registrationFieldStoreMapping.FieldId && sm.StoreId == registrationFieldStoreMapping.StoreId))
            return;

        await _nexportRegistrationFieldStoreMappingRepository.InsertAsync(registrationFieldStoreMapping);
    }

    public async Task DeleteNexportRegistrationFieldStoreMapping(NexportRegistrationFieldStoreMapping registrationFieldStoreMapping)
    {
        if (registrationFieldStoreMapping == null)
            throw new ArgumentNullException(nameof(registrationFieldStoreMapping));

        await _nexportRegistrationFieldStoreMappingRepository.DeleteAsync(registrationFieldStoreMapping);
    }

    public async Task<NexportRegistrationFieldAnswer> GetNexportRegistrationFieldAnswerById(int fieldAnswerId)
    {
        return fieldAnswerId < 1
            ? null
            : await _nexportRegistrationFieldAnswerRepository.GetByIdAsync(fieldAnswerId);
    }

    public async Task<IList<NexportRegistrationFieldAnswer>> GetNexportRegistrationFieldAnswers(int customerId,
        int? fieldId = null)
    {
        if (customerId < 1)
            return new List<NexportRegistrationFieldAnswer>();

        var query = _nexportRegistrationFieldAnswerRepository.Table
            .Where(fa => fa.CustomerId == customerId);

        if (fieldId != null)
        {
            query = query.Where(x => x.FieldId == fieldId);
        }

        return await query.ToListAsync();
    }

    public async Task<IPagedList<NexportRegistrationFieldAnswer>> GetNexportRegistrationFieldAnswersPagination(
        int customerId,
        int? fieldId = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _cacheManager.GetAsync(NexportIntegrationDefaults.RegistrationFieldAnswerAllCacheKey, async () =>
        {
            var query = _nexportRegistrationFieldAnswerRepository
                .Table.Where(fa => fa.CustomerId == customerId);

            if (fieldId != null)
            {
                query = query.Where(x => x.FieldId == fieldId);
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        });
    }

    public async Task<NexportRegistrationFieldAnswer> GetNexportRegistrationFieldAnswerByFieldOption(int customerId,
        int fieldId, int fieldOptionId)
    {
        if (customerId < 1 || fieldId < 1)
            return null;

        return await _nexportRegistrationFieldAnswerRepository.Table
            .FirstOrDefaultAsync(fa =>
            fa.CustomerId == customerId &&
            fa.FieldId == fieldId &&
            fa.FieldOptionId == fieldOptionId);
    }

    public async Task InsertNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer)
    {
        if (registrationFieldAnswer == null)
            throw new ArgumentNullException(nameof(registrationFieldAnswer));

        if (registrationFieldAnswer.FieldOptionId.HasValue)
        {
            if (await _nexportRegistrationFieldAnswerRepository.Table.AnyAsync(fa =>
                fa.CustomerId == registrationFieldAnswer.CustomerId &&
                fa.FieldId == registrationFieldAnswer.FieldOptionId &&
                fa.FieldOptionId == registrationFieldAnswer.FieldOptionId))
                return;
        }
        else
        {
            if (await _nexportRegistrationFieldAnswerRepository.Table.AnyAsync(fa =>
                fa.CustomerId == registrationFieldAnswer.CustomerId &&
                fa.FieldId == registrationFieldAnswer.FieldOptionId))
                return;
        }

        await _nexportRegistrationFieldAnswerRepository.InsertAsync(registrationFieldAnswer);
    }

    public async Task DeleteNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer)
    {
        if (registrationFieldAnswer == null)
            throw new ArgumentNullException(nameof(registrationFieldAnswer));

        await _nexportRegistrationFieldAnswerRepository.DeleteAsync(registrationFieldAnswer);
    }

    public async Task UpdateNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer)
    {
        if (registrationFieldAnswer == null)
            throw new ArgumentNullException(nameof(registrationFieldAnswer));

        await _nexportRegistrationFieldAnswerRepository.UpdateAsync(registrationFieldAnswer);
    }

    public async Task InsertNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportRegistrationFieldSynchronizationQueueRepository.InsertAsync(queueItem);
    }

    public async Task DeleteNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(queueItem);
    }

    public async Task UpdateNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem)
    {
        if (queueItem == null)
            throw new ArgumentNullException(nameof(queueItem));

        await _nexportRegistrationFieldSynchronizationQueueRepository.UpdateAsync(queueItem);
    }

    public async Task<bool> HasCustomRegistrationFieldRenderForStores(int fieldId, IList<int> storeIds, string customFieldRender)
    {
        var fieldWithCustomFieldRenders = _nexportRegistrationFieldRepository.Table
            .Where(x => x.CustomFieldRender == customFieldRender);

        if (storeIds.Count > 0)
        {
            // Find fields that have same custom field render
            var fieldWithCustomFieldRenderIds = fieldWithCustomFieldRenders
                .Where(x => x.Id != fieldId).Select(x => x.Id);

            foreach (var id in fieldWithCustomFieldRenderIds)
            {
                if (storeIds.Any(storeId =>
                    _nexportRegistrationFieldStoreMappingRepository.Table
                        .Any(x => x.FieldId == id && x.StoreId == storeId)))
                    return true;
            }
        }
        else
        {
            var availableStoreIds = await (await _storeService.GetAllStoresAsync()).Select(x => x.Id).ToListAsync();

            var fieldWithCustomFieldRenderIds = fieldWithCustomFieldRenders
                .Select(x => x.Id);

            foreach (var id in fieldWithCustomFieldRenderIds)
            {
                if (availableStoreIds.Any(storeId =>
                    _nexportRegistrationFieldStoreMappingRepository.Table
                        .Any(x => x.FieldId == id && x.StoreId == storeId)))
                    return true;
            }
        }

        return false;
    }

    public virtual async Task<IPagedList<NexportProductMapping>> GetAllNexportProductMappingsAsync(string searchProductName, NexportProductTypeEnum? searchProductType, string searchStoreName, int productId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        //get product mappings for the product
        var productMappings = _nexportProductMappingRepository.Table.Where(x => x.NopProductId == productId);

        //this checks if a default mapping exists for the product. if it does not then set insert empty default to true and if
        //it doesnt get filtered out we insert it at the end
        var insertEmptyDefault = await _nexportProductMappingRepository.Table.FirstOrDefaultAsync(x => x.NopProductId == productId && x.StoreId == null) == null;

        var left = _storeRepository.Table.Where(s => !s.Deleted)
            .GroupJoin(productMappings,
                s => s.Id,
                pm => pm.StoreId,
                (s, pm) => new { s, pm })
            .SelectMany(joined => joined.pm.DefaultIfEmpty(),
                (joined, pm) => new { joined.s, pm });

        var right = productMappings
            .GroupJoin(_storeRepository.Table,
                pm => pm.StoreId,
                s => s.Id,
                (pm, s) => new { s, pm })
            .SelectMany(joined => joined.s.DefaultIfEmpty(),
                (joined, s) => new { s = s, joined.pm });

        var productMappingsQuery = left.Union(right);

        if (!string.IsNullOrEmpty(searchStoreName))
        {
            if ("Default".Contains(searchStoreName, StringComparison.OrdinalIgnoreCase))
            {
                productMappingsQuery = productMappingsQuery.Where(x =>
                    x.s.Name.Contains(searchStoreName, StringComparison.OrdinalIgnoreCase) || x.s.Name == null);
            }
            else
            {
                productMappingsQuery = productMappingsQuery.Where(x =>
                    x.s.Name.Contains(searchStoreName, StringComparison.OrdinalIgnoreCase));
                insertEmptyDefault = false;
            }

        }

        if (!string.IsNullOrEmpty(searchProductName))
        {
            productMappingsQuery = productMappingsQuery.Where(x =>
                x.pm.NexportProductName.Contains(searchProductName, StringComparison.OrdinalIgnoreCase));
            insertEmptyDefault = false;
        }

        if (searchProductType != null)
        {
            productMappingsQuery = productMappingsQuery.Where(x => x.pm.Type == searchProductType);
            insertEmptyDefault = false;
        }

        var productMappingsList = await productMappingsQuery.Select(x => x.pm ?? new NexportProductMapping() { StoreId = x.s.Id, NexportCatalogId = Guid.Empty }).ToListAsync();

        // insert an empty value for default if there is no default for the product and it wasnt filtered out by the search
        if (insertEmptyDefault)
            productMappingsList.Insert(0, new NexportProductMapping { NexportCatalogId = Guid.Empty });


        return new PagedList<NexportProductMapping>(productMappingsList, pageIndex, pageSize);

    }

    public virtual async Task<IPagedList<NexportProductMapping>> GetAllNexportProductMappingsByCategoryIdAsync(int nopCategoryId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        //get product mappings for the product
        var productMappings = await _nexportProductMappingRepository.Table.Where(x => x.NopCategoryId == nopCategoryId).ToListAsync();

        return new PagedList<NexportProductMapping>(productMappings, pageIndex, pageSize);

    }

    public async Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItemByGuidAsync(Guid orderInvoiceItemId)
    {
        if (orderInvoiceItemId == Guid.Empty)
            throw new ArgumentException("invoice item Id cannot be empty Guid", nameof(orderInvoiceItemId));

        return await _nexportOrderInvoiceItemRepository.Table.SingleOrDefaultAsync(x =>
            x.InvoiceItemId == orderInvoiceItemId);
    }

    public async Task<IList<NexportGroupProductModel>> GetGroupProductModelForGroupId(Guid groupId)
    {
        //SELECT Product.Name,SUM(IIF(NexportOrderInvoiceItem.RedeemingUserId IS NULL,1,0)) AS Available,
        //    SUM(IIF(NexportOrderInvoiceItem.RedeemingUserId IS NOT NULL,IIF(NexportOrderInvoiceItem.UTCDateRedemption IS NULL,1,0),0)) As Awaiting,
        //    SUM(IIF(NexportOrderInvoiceItem.RedeemingUserId IS NOT NULL,IIF(NexportOrderInvoiceItem.UTCDateRedemption IS NOT NULL,1,0),0)) As Redeemed
        //FROM [Marketplace].[dbo].[GenericAttribute]
        //Join [Order] On GenericAttribute.EntityId = [Order].Id
        //    Join OrderItem On OrderItem.OrderId = [Order].Id
        //    Join Product On OrderItem.ProductId = Product.Id
        //Join NexportOrderInvoiceItem On NexportOrderInvoiceItem.OrderItemId = orderItem.Id
        //    --Join NexportOrderInvoiceItem On OrderItem.Id = NexportOrderInvoiceItem.OrderItemId AND OrderItem.OrderId = NexportOrderInvoiceItem.OrderId
        //where [key] = 'groupfororder' AND [value]= 'f78b50a2-d330-444f-8178-54effbf38cda'
        //group by product.Name

        var query = _genericAttributeRepository.Table
            .Where(x => x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""))
            .Join(_orderItemRepository.Table, gar => gar.EntityId, ori => ori.OrderId, (gar, ori) => ori)
            .Join(_productRepository.Table, ori => ori.ProductId, pr => pr.Id, (ori, pr) => new { ori, pr })
            .Join(_nexportOrderInvoiceItemRepository.Table, oripr => new { a = oripr.ori.OrderId, b = oripr.ori.Id },
                noii => new { a = noii.OrderId, b = noii.OrderItemId },
                (oripr, noii) => new { oripr, noii })
            .GroupBy(x => new { ProductId = x.oripr.pr.Id, Name = x.oripr.pr.Name })
            .Select(x =>
                new NexportGroupProductModel
                {
                    Id = x.Key.ProductId,
                    Name = x.Key.Name,
                    Available = x.Sum(y => y.noii.RedeemingUserId == null && y.noii.UtcDateRedemption == null ? 1 : 0),
                    Awaiting = x.Sum(y => y.noii.RedeemingUserId != null && y.noii.UtcDateRedemption == null ? 1 : 0),
                    Redeemed = x.Sum(y => y.noii.RedeemingUserId != null && y.noii.UtcDateRedemption != null ? 1 : 0),
                    GroupId = groupId

                });

        return await query.ToListAsync();
    }

    public async Task<IList<NexportOrderInvoiceItem>> GetInvoiceItemsForGroupIdAndProductIdAndRedeemingUserIdHasValue(Guid groupId,
        int productId)
    {
        var query = _genericAttributeRepository.Table
            .Where(x => x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""))
            .Join(_orderItemRepository.Table, gar => gar.EntityId, ori => ori.OrderId, (gar, ori) => ori)
            .Join(_productRepository.Table, ori => ori.ProductId, pr => pr.Id, (ori, pr) => new { ori, pr })
            .Where(x => x.pr.Id == productId)
            .Join(_nexportOrderInvoiceItemRepository.Table, oripr => new { a = oripr.ori.OrderId, b = oripr.ori.Id },
                noii => new { a = noii.OrderId, b = noii.OrderItemId },
                (oripr, noii) => noii)
            .Where(x => x.RedeemingUserId.HasValue);

        return await query.ToListAsync();
    }

    public async Task<NexportOrderInvoiceItem> GetFirstAvailableInvoiceItemForGroupIdAndProductId(Guid groupId,
        int productId)
    {
        var query = _genericAttributeRepository.Table
            .Where(x => x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""))
            .Join(_orderItemRepository.Table, gar => gar.EntityId, ori => ori.OrderId, (gar, ori) => ori)
            .Join(_productRepository.Table, ori => ori.ProductId, pr => pr.Id, (ori, pr) => new { ori, pr })
            .Where(x => x.pr.Id == productId)
            .Join(_nexportOrderInvoiceItemRepository.Table, oripr => new { a = oripr.ori.OrderId, b = oripr.ori.Id },
                noii => new { a = noii.OrderId, b = noii.OrderItemId },
                (oripr, noii) => noii)
            .Where(x => !(x.RedeemingUserId.HasValue && x.UtcDateRedemption.HasValue));

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<GenericAttribute>> GetAllGroupForOrdersAsync()
    {
        var query = _genericAttributeRepository.Table.AsEnumerable()
            .Where(x => x.Key == "GroupForOrder").GroupBy(x => x.Value).Select(x => x.FirstOrDefault());

        var result = await query.ToListAsync();
        return result ?? new List<GenericAttribute>();
    }

    public async Task<GenericAttribute> GetGroupByGroupIdAsync(Guid groupId)
    {
        var attr = _genericAttributeRepository.Table.FirstOrDefault(x => x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""));

        return attr;
    }

    public async Task<int> GetAvailableNexportGroupProductRedemptionsCountAsync(Guid? groupId, int productId,
        int? orderId = null, Customer customer = null, Store store = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));
        if (productId < 0)
            throw new ArgumentOutOfRangeException(nameof(productId));

        var orderQuery = _orderRepository.Table;

        if (store != null)
            orderQuery = orderQuery.Where(x => x.StoreId == store.Id);

        if (customer != null)
            orderQuery = orderQuery.Where(x => x.CustomerId == customer.Id);

        if (orderId != null)
            orderQuery = orderQuery!.Where(x => x.Id == orderId);

        var query = _wholesaleOrderInfoRepository.Table
            .Join(_nexportOrderInvoiceItemRepository.Table.Where(x => x.RedemptionStatusId == (int)NexportOrderInvoiceItemRedemptionStatus.Available),
                x => x.OrderId, nexportInvoiceItem => nexportInvoiceItem.OrderId,
                (wholesaleOrder, nexportInvoiceItem) => new { wholesaleOrder, nexportInvoiceItem })
            .Join(orderQuery, x => x.wholesaleOrder.OrderId, order => order.Id,
                (wholesaleOrderWithNexportInvoice, order) => new { wholesaleOrderWithNexportInvoice.wholesaleOrder, order })
            .Where(x => x.wholesaleOrder.NexportGroupId == groupId && x.wholesaleOrder.ProductId == productId)
            .Distinct()
            .Select(x => x.wholesaleOrder);

        var available = query.Sum(x => x.Available);

        return available;
    }

    public async Task<int> GetAvailableNexportRedemptionsByFundingPoolCountAsync(int? fundingPoolId)
    {
        var orderQuery = _orderRepository.Table;

        var query = _wholesaleOrderInfoRepository.Table
            .Join(
                _nexportOrderInvoiceItemRepository.Table.Where(x =>
                    x.RedemptionStatusId == (int)NexportOrderInvoiceItemRedemptionStatus.Available),
                x => x.OrderId, nexportInvoiceItem => nexportInvoiceItem.OrderId,
                (wholesaleOrder, nexportInvoiceItem) => new { wholesaleOrder, nexportInvoiceItem })
            .Join(orderQuery, x => x.wholesaleOrder.OrderId, order => order.Id,
                (wholesaleOrderWithNexportInvoice, order) =>
                    new { wholesaleOrderWithNexportInvoice.wholesaleOrder, order })
            .Where(x => x.wholesaleOrder.FundingPoolId == fundingPoolId)
            //.GroupBy(x => x.wholesaleOrder.FundingPoolId)
            .Distinct()
            .Select(x => x.wholesaleOrder);


        var available = query.Sum(x => x.Available);

        return available;
    }

    public async Task InsertOrUpdateWholesalePurchaseGroupAsync(WholesalePurchasingGroup wholesalePurchasingGroup)
    {
        if (wholesalePurchasingGroup == null)
            throw new ArgumentNullException(nameof(wholesalePurchasingGroup));

        var existingGroup =
            await _wholesalePurchasingGroupRepository.Table.FirstOrDefaultAsync(x => x.NexportGroupId == wholesalePurchasingGroup.NexportGroupId);

        if (existingGroup != null)
        {
            //update the group
            existingGroup.NexportGroupName = wholesalePurchasingGroup.NexportGroupName;
            existingGroup.NexportGroupShortName = wholesalePurchasingGroup.NexportGroupShortName;
            await _wholesalePurchasingGroupRepository.UpdateAsync(existingGroup);
        }
        else
        {
            //insert new group into table
            await _wholesalePurchasingGroupRepository.InsertAsync(wholesalePurchasingGroup);
        }
    }

    public async Task InsertWholesaleOrderInfoAsync(WholesaleOrderInfo wholesaleOrderInfo)
    {
        if (wholesaleOrderInfo == null)
            throw new ArgumentNullException(nameof(wholesaleOrderInfo));

        await _wholesaleOrderInfoRepository.InsertAsync(wholesaleOrderInfo);
    }

    public async Task UpdateWholesaleOrderInfoAsync(WholesaleOrderInfo wholesaleOrderInfo)
    {
        if (wholesaleOrderInfo == null)
            throw new ArgumentNullException(nameof(wholesaleOrderInfo));

        await _wholesaleOrderInfoRepository.UpdateAsync(wholesaleOrderInfo);
    }

    public async Task<WholesalePurchasingGroup> GetWholesalePurchaseGroupAsync(Guid groupId)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));

        var group = await _wholesalePurchasingGroupRepository
            .Table
            .FirstOrDefaultAsync(x => x.NexportGroupId == groupId);

        return group;
    }

    public async Task<WholesalePurchasingGroup> GetWholesalePurchaseGroupForOrderAsync(Order order)
    {

        if (order == null)
            throw new ArgumentNullException(nameof(order));

        var orderInfo = await GetWholesaleOrderInfoForOrderAsync(order.Id);

        if (orderInfo?.NexportGroupId == null)
            return null;

        var group = await GetWholesalePurchaseGroupAsync(orderInfo.NexportGroupId.Value);

        return group;
    }

    public async Task<WholesaleOrderInfo> GetWholesaleOrderInfoForOrderItemAsync(int orderId, int orderItemId)
    {
        if (orderId < 0)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (orderItemId < 0)
            throw new ArgumentOutOfRangeException(nameof(orderItemId));

        var info = await _wholesaleOrderInfoRepository.Table.FirstOrDefaultAsync(x => x.OrderId == orderId && x.OrderItemId == orderItemId);

        return info;
    }

    public async Task<WholesaleOrderInfo> GetWholesaleOrderInfoForOrderAsync(int orderId)
    {
        if (orderId < 0)
            throw new ArgumentOutOfRangeException(nameof(orderId));

        var info = await _wholesaleOrderInfoRepository.Table.FirstOrDefaultAsync(x => x.OrderId == orderId);

        return info;
    }

    public async Task<IList<Order>> FindOrdersForCustomerAsync(Customer customer, Store store = null)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<Order>> GetOrdersForCustomer(Customer customer, Store store = null)
    {
        if (store == null)
            store = await _storeContext.GetCurrentStoreAsync();

        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        var orders = await _orderRepository.Table.Where(x => x.CustomerId == customer.Id && x.StoreId == store.Id).ToListAsync();

        return orders;
    }

    public virtual async Task<IList<Customer>> SearchCustomersAsync(string searchNameAndEmail)
    {
        var query = _customerRepository.Table.Where(c => !c.Deleted && !c.IsSystemAccount && !string.IsNullOrWhiteSpace(c.Email));

        query = query.Where(c => (c.FirstName + " " + c.LastName).Contains(searchNameAndEmail) || c.Email.Contains(searchNameAndEmail));

        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
        {
            query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                    (x, y) => new { Customer = x, Mapping = y })
                .Where(z => z.Mapping.CustomerRoleId == registeredRole.Id)
                .Select(z => z.Customer)
                .Distinct();
        }

        query = query.OrderBy(c => c.Email);

        return await query.ToListAsync();
    }

    public async Task<IList<NexportProductMapping>> GetAllProductMappingsByCatalogIdAsync(Guid catalogId)
    {
        return await _nexportProductMappingRepository.Table.Where(x => x.NexportCatalogId == catalogId).ToListAsync();
    }

    public async Task<IList<WholesaleOrderInfo>> SearchGroupProductsAsync(Guid? groupId, string productName, Customer customer = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));

        var productQuery = _productRepository.Table;
        if (!string.IsNullOrWhiteSpace(productName))
            productQuery = productQuery.Where(x => x.Name.Contains(productName));

        var productIdList = await productQuery.Select(x => x.Id).ToListAsync();

        var orderQuery = _orderRepository.Table;
        if (customer != null)
        {
            orderQuery = orderQuery.Where(x => x.CustomerId == customer.Id);
        }

        var orderIdList = await orderQuery.Select(x => x.Id).ToListAsync();

        var wholesaleOrderInfoQuery =
            _wholesaleOrderInfoRepository.Table
                .Where(x =>
                    x.NexportGroupId == groupId && productIdList.Contains(x.ProductId) && orderIdList.Contains(x.OrderId))
                .GroupBy(x => x.ProductId)
                .Select(x =>
                    new WholesaleOrderInfo
                    {
                        NexportGroupId = x.FirstOrDefault()!.NexportGroupId,
                        OrderId = x.FirstOrDefault()!.OrderId,
                        OrderItemId = x.FirstOrDefault()!.OrderItemId,
                        ProductId = x.FirstOrDefault()!.ProductId,
                        Available = x.Sum(y => y.Available),
                        Awaiting = x.Sum(y => y.Awaiting),
                        Redeemed = x.Sum(y => y.Redeemed),
                        FundingPoolId = x.FirstOrDefault()!.FundingPoolId,
                        UtcRedeemByDate = x.FirstOrDefault()!.UtcRedeemByDate
                    });

        return await wholesaleOrderInfoQuery.ToListAsync();
    }

    public async Task<IList<NexportOrderInvoiceItem>> SearchGroupProductRedemptionsAsync(Guid? groupId, int productId,
        NexportOrderInvoiceItemRedemptionStatus? redemptionStatus,
        string customerName = null, string customerEmail = null, string purchaserName = null,
        DateTime? fromUtc = null, DateTime? toUtc = null, int? orderId = null, Store store = null,
        Customer customer = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group Id cannot be empty Guid", nameof(groupId));

        if (productId < 0)
            throw new ArgumentOutOfRangeException(nameof(productId));

        var invoiceItemQuery = _nexportOrderInvoiceItemRepository.Table;

        var orderQuery = _orderRepository.Table;

        if (customer != null)
        {
            orderQuery = orderQuery.Where(x => x.CustomerId == customer.Id);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(purchaserName))
            {
                var customerQuery = _customerRepository.Table;

                customerQuery = customerQuery.Where(x => x.FirstName.Contains(purchaserName) || x.LastName.Contains(purchaserName));
                var customerIdList = await customerQuery.Select(x => x.Id).ToListAsync();

                if (customerIdList.Any())
                {
                    orderQuery = orderQuery.Where(x => customerIdList.Contains(x.CustomerId));
                }
            }
        }

        if (store != null)
        {
            orderQuery = orderQuery.Where(x => x.StoreId == store.Id);
        }

        var orderIdList = await orderQuery.Select(x => x.Id).ToListAsync();

        invoiceItemQuery = invoiceItemQuery.Where(x => orderIdList.Contains(x.OrderId));

        if (orderId != null)
        {
            invoiceItemQuery = invoiceItemQuery.Where(x => x.OrderId == orderId);
        }

        if (fromUtc.HasValue)
            invoiceItemQuery = invoiceItemQuery.Where(x => fromUtc.Value <= x.UtcDateRedemption);
        if (toUtc.HasValue)
            invoiceItemQuery = invoiceItemQuery.Where(x => toUtc.Value >= x.UtcDateRedemption);

        if (!string.IsNullOrWhiteSpace(customerEmail) || !string.IsNullOrWhiteSpace(customerName))
        {
            var customerQuery = _customerRepository.Table;

            if (!string.IsNullOrWhiteSpace(customerEmail))
                customerQuery = customerQuery.Where(x => x.Email.Contains(customerEmail));

            if (!string.IsNullOrWhiteSpace(customerName))
                customerQuery = customerQuery.Where(x => x.FirstName.Contains(customerName) || x.LastName.Contains(customerName));

            var customerIdList = await customerQuery.Select(x => x.Id).ToListAsync();

            if (customerIdList.Any())
            {
                var nexportUserQuery =
                    _nexportUserMappingRepository.Table.Where(x => customerIdList.Contains(x.NopUserId));

                var nexportUserIdList = await nexportUserQuery.Select(x => x.NexportUserId).ToListAsync();

                invoiceItemQuery = invoiceItemQuery.Where(x =>
                    x.RedeemingUserId != null && nexportUserIdList.Contains(x.RedeemingUserId.Value));
            }
        }

        var orderInfoQuery = _wholesaleOrderInfoRepository.Table.Where(x => x.NexportGroupId == groupId && x.ProductId == productId);

        var orderInfoQueryIdList = await orderInfoQuery
            .Select(x => new { orderId = x.OrderId, orderItemId = x.OrderItemId }).ToListAsync();

        invoiceItemQuery = invoiceItemQuery.Where(x => orderInfoQueryIdList.Contains(new { orderId = x.OrderId, orderItemId = x.OrderItemId }));

        if (redemptionStatus != null)
            invoiceItemQuery = invoiceItemQuery.Where(x => x.RedemptionStatusId == (int)redemptionStatus);

        return await invoiceItemQuery.ToListAsync();
    }

    public async Task<IList<NexportOrderInvoiceItem>> SearchProductRedemptionsAsync(int? fundingPoolId,
        NexportOrderInvoiceItemRedemptionStatus? redemptionStatus,
        string customerName = null, string customerEmail = null, string purchaserName = null, string productName = null,
        DateTime? fromUtc = null, DateTime? toUtc = null, Store store = null, int? orderId = null)
    {
        var invoiceItemQuery = _nexportOrderInvoiceItemRepository.Table;

        var orderQuery = _orderRepository.Table;

        if (!string.IsNullOrWhiteSpace(purchaserName))
        {
            var customerQuery = _customerRepository.Table;

            customerQuery = customerQuery.Where(x => x.FirstName.Contains(purchaserName) || x.LastName.Contains(purchaserName));
            var customerIdList = await customerQuery.Select(x => x.Id).ToListAsync();

            if (customerIdList.Any())
            {
                orderQuery = orderQuery.Where(x => customerIdList.Contains(x.CustomerId));
            }
        }

        var orderIdList = await orderQuery.Select(x => x.Id).ToListAsync();

        invoiceItemQuery = invoiceItemQuery.Where(x => orderIdList.Contains(x.OrderId));

        if (orderId != null)
        {
            invoiceItemQuery = invoiceItemQuery.Where(x => x.OrderId == orderId);
        }

        if (fromUtc.HasValue)
            invoiceItemQuery = invoiceItemQuery.Where(x => fromUtc.Value <= x.UtcDateRedemption);
        if (toUtc.HasValue)
            invoiceItemQuery = invoiceItemQuery.Where(x => toUtc.Value >= x.UtcDateRedemption);

        if (!string.IsNullOrWhiteSpace(customerEmail) || !string.IsNullOrWhiteSpace(customerName))
        {
            var customerQuery = _customerRepository.Table;

            if (!string.IsNullOrWhiteSpace(customerEmail))
                customerQuery = customerQuery.Where(x => x.Email.Contains(customerEmail));

            if (!string.IsNullOrWhiteSpace(customerName))
                customerQuery = customerQuery.Where(x => x.FirstName.Contains(customerName) || x.LastName.Contains(customerName));

            var customerIdList = await customerQuery.Select(x => x.Id).ToListAsync();

            if (customerIdList.Any())
            {
                var nexportUserQuery =
                    _nexportUserMappingRepository.Table.Where(x => customerIdList.Contains(x.NopUserId));

                var nexportUserIdList = await nexportUserQuery.Select(x => x.NexportUserId).ToListAsync();

                invoiceItemQuery = invoiceItemQuery.Where(x =>
                    x.RedeemingUserId != null && nexportUserIdList.Contains(x.RedeemingUserId.Value));
            }
        }

        var orderInfoQuery = _wholesaleOrderInfoRepository.Table.Where(x => x.FundingPoolId == fundingPoolId);

        if (!string.IsNullOrWhiteSpace(productName))
        {
            var productQuery = _productRepository.Table;
            if (!string.IsNullOrWhiteSpace(productName))
                productQuery = productQuery.Where(x => x.Name.Contains(productName));

            var productIdList = await productQuery.Select(x => x.Id).ToListAsync();
            orderInfoQuery = orderInfoQuery.Where(x => productIdList.Contains(x.ProductId));
        }

        var orderInfoQueryIdList = await orderInfoQuery
            .Select(x => new { orderId = x.OrderId, orderItemId = x.OrderItemId }).ToListAsync();

        invoiceItemQuery = invoiceItemQuery.Where(x => orderInfoQueryIdList.Contains(new { orderId = x.OrderId, orderItemId = x.OrderItemId }));

        if (redemptionStatus != null)
            invoiceItemQuery = invoiceItemQuery.Where(x => x.RedemptionStatusId == (int)redemptionStatus);

        return await invoiceItemQuery.ToListAsync();
    }

    public async Task<bool> RedeemProductForCustomer(RedeemProductModel model)
    {
        if (model.ProductId == 0)
            throw new Exception("Product Id is invalid!");

        var invoiceItem = await FindNexportOrderInvoiceItemByGuidAsync(model.InvoiceItemId);
        if (invoiceItem == null)
            throw new Exception("Invoice item Id cannot be null!");

        try
        {
            var orderInfo = await GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);
            var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);
            var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);

            if (order == null)
                throw new Exception($"Order for invoice item {invoiceItem.InvoiceItemId} is missing!");

            if (orderItem == null)
                throw new Exception($"Order item for invoice item {invoiceItem.InvoiceItemId} is missing!");

            if (orderInfo is { Available: 0, ApprovalAwaiting: 0 })
                throw new Exception($"Order info for invoice item {invoiceItem.InvoiceItemId} is missing");

            var productMapping = await GetProductMappingByNopProductId(model.ProductId, order.StoreId)
                                 ?? await GetProductMappingByNopProductId(model.ProductId);
            if (productMapping != null)
            {
                // Set attributes so that processing and awaiting redemptions can display email and name
                await _genericAttributeService.SaveAttributeAsync(invoiceItem,
                    $"redeeming-user-email-for-invoice-{invoiceItem.Id}", model.Email, order.StoreId);
                await _genericAttributeService.SaveAttributeAsync(invoiceItem,
                    $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", model.FirstName, order.StoreId);
                await _genericAttributeService.SaveAttributeAsync(invoiceItem,
                    $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", model.LastName, order.StoreId);

                int? redeemingProductMappingId = null;
                // Get the correct product mapping details for open-ended product item
                if (model is { IsOpenEnded: true, RedeemingProductId: not null })
                {
                    var redeemingProductMapping = await GetProductMappingByNopProductId(model.RedeemingProductId.Value, model.StoreId)
                                                  ?? await GetProductMappingByNopProductId(model.RedeemingProductId.Value);
                    if (redeemingProductMapping != null)
                    {
                        redeemingProductMappingId = redeemingProductMapping.Id;
                        await _genericAttributeService.SaveAttributeAsync(orderItem,
                            $"ProductMapping-OpenEnded-Selected-{order.Id}-{orderItem.Id}",
                            JsonConvert.SerializeObject(redeemingProductMapping), order.StoreId);

                        var groupMembershipMappings = await GetProductGroupMembershipMappings(redeemingProductMapping.Id);
                        foreach (var groupMembershipMapping in groupMembershipMappings)
                        {
                            await _genericAttributeService.SaveAttributeAsync(orderItem,
                                $"ProductGroupMembershipMapping-OpenEnded-Selected-{order.Id}-{orderItem.Id}-{redeemingProductMapping.Id}-{groupMembershipMapping.Id}",
                                JsonConvert.SerializeObject(groupMembershipMapping), order.StoreId);
                        }
                    }
                }

                await _genericAttributeService.SaveAttributeAsync(invoiceItem, $"RedeemingPurchasingGroup-{invoiceItem.Id}", model.PurchasingGroupId, order.StoreId);

                if (model.ExtensionOption != null)
                {
                    await _genericAttributeService.SaveAttributeAsync(invoiceItem, $"IgnoreEnrollmentStatus-{invoiceItem.Id}", true, order.StoreId);
                }

                if (model.AssignmentType == "Instant")
                {
                    if (model.UserId == null)
                        throw new Exception("User Id of the redeeming user cannot be empty!");

                    if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ApprovalAwaiting)
                    {
                        orderInfo.ApprovalAwaiting--;
                    }
                    else
                    {
                        orderInfo.Available--;
                    }

                    orderInfo.ProcessingAvailable++;

                    invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable;
                    invoiceItem.RedeemingUserId = model.UserId;

                    await UpdateNexportOrderInvoiceItem(invoiceItem);
                    await UpdateWholesaleOrderInfoAsync(orderInfo);

                    await InsertNexportOrderInvoiceRedemptionQueueItem(
                        new NexportOrderInvoiceRedemptionQueueItem
                        {
                            OrderInvoiceItemId = invoiceItem.Id,
                            RedeemingUserId = model.UserId.Value,
                            ProductMappingId = productMapping.Id,
                            RedeemingProductMappingId = redeemingProductMappingId,
                            OrderItemId = invoiceItem.OrderItemId,
                            UtcDateCreated = DateTime.UtcNow,
                            UtcProcessingDate = model.UtcStartDate,
                            ManualApprovalAction = model.ExtensionOption
                        });
                }
                else
                {
                    if (model.Email == null)
                        throw new Exception("Email address cannot be empty when selecting redeeming by email method!");

                    await SendNewNexportManualRedemptionCustomerNotificationAsync(order, invoiceItem.Id, productMapping.Id,
                        _localizationSettings.DefaultAdminLanguageId, model.Email, model.FirstName, model.LastName);

                    invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Awaiting;
                    orderInfo.Available--;
                    orderInfo.Awaiting++;

                    await UpdateNexportOrderInvoiceItem(invoiceItem);
                    await UpdateWholesaleOrderInfoAsync(orderInfo);
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to redeem invoice item {invoiceItem.InvoiceItemId} for user {model.Email}", ex);
        }

        return false;
    }

    public async Task UnassignInvoiceItem(NexportOrderInvoiceItem invoiceItem)
    {
        try
        {
            var wholesaleOrderInfo = await GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);

            if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.Assigned && wholesaleOrderInfo is { Redeemed: > 0 })
            {
                var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                if (order != null)
                {
                    // reset processing and awaiting redemptions attributes for name and email
                    await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
                        $"redeeming-user-email-for-invoice-{invoiceItem.Id}", null, order.StoreId);
                    await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
                        $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", null, order.StoreId);
                    await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
                        $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", null, order.StoreId);
                }

                invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable;
                await UpdateNexportOrderInvoiceItem(invoiceItem);

                wholesaleOrderInfo.ProcessingAvailable++;
                wholesaleOrderInfo.Redeemed--;
                await UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (invoiceItem.RedeemingUserId != null)
                {
                    var targetedCustomerUserMapping = await FindUserMappingByNexportUserId(invoiceItem.RedeemingUserId.Value);
                    if (targetedCustomerUserMapping != null)
                    {
                        await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
                        {
                            InvoiceItemId = invoiceItem.InvoiceItemId,
                            CustomerId = currentCustomer.Id,
                            TargetedCustomerId = targetedCustomerUserMapping.NopUserId,
                            Description = "Invoice item had been scheduled for unassign",
                            Type = NexportRedemptionAuditLogTypeEnum.UnassignRedemption,
                            UtcDateCreated = DateTime.UtcNow
                        });
                    }
                }

                await InsertNexportOrderInvoiceResetRedemptionQueueItem(
                    new NexportOrderInvoiceResetRedemptionQueueItem
                    {
                        OrderInvoiceItemId = invoiceItem.Id,
                        UtcDateCreated = DateTime.UtcNow,
                        RetryCount = 0
                    });
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to insert invoice item {invoiceItem.InvoiceItemId} into the reset redemption queue", ex);
        }
    }

    public async Task RedeemAwaitingInvoiceItem(NexportOrderInvoiceItem invoiceItem, Guid nexportUserId, int productMappingId)
    {
        var orderInfo = await GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);
        if (orderInfo is { Awaiting: > 0 })
        {
            var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
            if (order != null)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);
                if (orderItem != null)
                {
                    invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting;
                    await UpdateNexportOrderInvoiceItem(invoiceItem);

                    orderInfo.Awaiting--;
                    orderInfo.ProcessingAwaiting++;
                    await UpdateWholesaleOrderInfoAsync(orderInfo);

                    await InsertNexportOrderInvoiceRedemptionQueueItem(
                        new NexportOrderInvoiceRedemptionQueueItem
                        {
                            OrderInvoiceItemId = invoiceItem.Id,
                            RedeemingUserId = nexportUserId,
                            ProductMappingId = productMappingId,
                            OrderItemId = invoiceItem.OrderItemId,
                            UtcDateCreated = DateTime.UtcNow
                        });
                }
            }
        }
    }

    public async Task CancelAwaitingInvoiceItem(NexportOrderInvoiceItem invoiceItem)
    {
        try
        {
            var wholesaleOrderInfo = await GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);

            if (wholesaleOrderInfo is { Awaiting: > 0 })
            {
                var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                if (order != null)
                {
                    // reset processing and awaiting redemptions attributes for name and email
                    await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
                        $"redeeming-user-email-for-invoice-{invoiceItem.Id}", null, order.StoreId);
                    await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
                        $"redeeming-user-first-name-for-invoice-{invoiceItem.Id}", null, order.StoreId);
                    await _genericAttributeService.SaveAttributeAsync<string>(invoiceItem,
                        $"redeeming-user-last-name-for-invoice-{invoiceItem.Id}", null, order.StoreId);
                }

                invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Available;
                invoiceItem.RedeemingUserId = null;
                await UpdateNexportOrderInvoiceItem(invoiceItem);

                wholesaleOrderInfo.Awaiting--;
                wholesaleOrderInfo.Available++;
                await UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
                {
                    InvoiceItemId = invoiceItem.InvoiceItemId,
                    CustomerId = currentCustomer.Id,
                    Description = "Awaiting invoice item had been cancelled",
                    Type = NexportRedemptionAuditLogTypeEnum.CancelAwaiting,
                    UtcDateCreated = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to cancel awaiting status for invoice item {invoiceItem.InvoiceItemId}", ex);
        }
    }

    public async Task ApproveRedemptionAssignment(NexportRedemptionAssignmentApprovalRequest approvalRequest,
        NexportOrderInvoiceItem invoiceItem, WholesaleOrderInfo wholesaleOrderInfo)
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            int? targetCustomerId = null;
            if (approvalRequest.RedemptionUserId != null)
            {
                var redeemingUserMapping = await FindUserMappingByNexportUserId(approvalRequest.RedemptionUserId.Value);
                if (redeemingUserMapping != null)
                {
                    targetCustomerId = redeemingUserMapping.NopUserId;
                }
            }

            await RedeemProductForCustomer(
                new RedeemProductModel
                {
                    ProductId = approvalRequest.ProductId,
                    RedeemingProductId = approvalRequest.RedeemingProductId,
                    AssignmentType = approvalRequest.RedemptionAssignmentType == NexportRedemptionAssignmentTypeStatus.Instant ? "Instant" : "Email",
                    Email = approvalRequest.RedemptionEmail,
                    FirstName = approvalRequest.RedemptionFirstName,
                    LastName = approvalRequest.RedemptionLastName,
                    InvoiceItemId = approvalRequest.InvoiceItemId,
                    UserId = approvalRequest.RedemptionUserId,
                    UtcStartDate = approvalRequest.UtcRedemptionStartDate,
                    StoreId = approvalRequest.StoreId,
                    PurchasingGroupId = approvalRequest.PurchasingGroupId,
                    IsOpenEnded = approvalRequest.IsOpenEnded,
                    ExtensionOption = approvalRequest.ExtensionOption
                });

            approvalRequest.Status = NexportRedemptionAssignmentApprovalRequestStatus.Accepted;
            approvalRequest.ApprovedByCustomerId = currentCustomer.Id;
            approvalRequest.UtcModifiedDate = DateTime.UtcNow;

            await UpdateNexportRedemptionAssignmentApprovalRequestAsync(approvalRequest);

            await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
            {
                InvoiceItemId = invoiceItem.InvoiceItemId,
                CustomerId = currentCustomer.Id,
                TargetedCustomerId = targetCustomerId,
                Description = "The assignment for the redemption had been approved",
                Type = NexportRedemptionAuditLogTypeEnum.ApprovalAwaiting,
                UtcDateCreated = DateTime.UtcNow
            });

            // Notify the customer that the assignment request has been accepted
            await SendNexportRedemptionAssignmentApprovalRequestCustomerNotificationAsync(
                approvalRequest, invoiceItem,
                NexportDefaults.REDEMPTION_ASSIGNMENT_APPROVAL_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to approve the redemption assignment for request #{approvalRequest.Id} ({invoiceItem.InvoiceItemId})", ex);
            throw;
        }
    }

    public async Task DenyRedemptionAssignment(NexportRedemptionAssignmentApprovalRequest approvalRequest,
        NexportOrderInvoiceItem invoiceItem, WholesaleOrderInfo wholesaleOrderInfo)
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            int? targetCustomerId = null;
            if (approvalRequest.RedemptionUserId != null)
            {
                var redeemingUserMapping = await FindUserMappingByNexportUserId(approvalRequest.RedemptionUserId.Value);
                if (redeemingUserMapping != null)
                {
                    targetCustomerId = redeemingUserMapping.NopUserId;
                }
            }

            approvalRequest.Status = NexportRedemptionAssignmentApprovalRequestStatus.Rejected;
            approvalRequest.UtcModifiedDate = DateTime.UtcNow;

            invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Available;

            wholesaleOrderInfo.Available++;
            wholesaleOrderInfo.ApprovalAwaiting--;

            await UpdateNexportOrderInvoiceItem(invoiceItem);
            await UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);
            await UpdateNexportRedemptionAssignmentApprovalRequestAsync(approvalRequest);

            await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
            {
                InvoiceItemId = invoiceItem.InvoiceItemId,
                CustomerId = currentCustomer.Id,
                TargetedCustomerId = targetCustomerId,
                Description = "The assignment for the redemption had been rejected",
                Type = NexportRedemptionAuditLogTypeEnum.ApprovalAwaiting,
                UtcDateCreated = DateTime.UtcNow
            });

            // Notify the customer that the assignment request has been rejected
            await SendNexportRedemptionAssignmentApprovalRequestCustomerNotificationAsync(
                approvalRequest, invoiceItem,
                NexportDefaults.REDEMPTION_UNASSIGNMENT_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to deny the redemption assignment for request #{approvalRequest.Id} ({invoiceItem.InvoiceItemId})", ex);
            throw;
        }
    }

    public async Task<bool> ProcessRefundingInvoiceItem(NexportOrderInvoiceItem invoiceItem, bool accept)
    {
        if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.Refunded)
            throw new NexportException("Cannot process refund for an invoice item that has already been refunded!");

        try
        {
            var wholesaleOrderInfo = await GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);

            if (accept)
            {
                if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund)
                    wholesaleOrderInfo.ProcessingRefund--;
                else
                    wholesaleOrderInfo.Available--;

                invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Refunded;
                wholesaleOrderInfo.Refunded++;

                await UpdateNexportOrderInvoiceItem(invoiceItem);
                await UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = invoiceItem.OrderId,
                    Note = $"Invoice item #{invoiceItem.InvoiceItemId} for order item #{invoiceItem.OrderItemId} has been refunded.",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
                {
                    InvoiceItemId = invoiceItem.InvoiceItemId,
                    CustomerId = currentCustomer.Id,
                    Description = "Invoice item had been refunded.",
                    Type = NexportRedemptionAuditLogTypeEnum.RefundRedemption,
                    UtcDateCreated = DateTime.UtcNow
                });
            }
            else
            {
                if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund)
                    wholesaleOrderInfo.ProcessingRefund--;

                var previousStatus = (NexportOrderInvoiceItemRedemptionStatus)await _genericAttributeService.GetAttributeAsync<int>(
                    invoiceItem, "RefundRequestInvoiceItemRedemptionStatus");

                invoiceItem.RedemptionStatus = previousStatus;
                wholesaleOrderInfo.Redeemed++;

                await UpdateNexportOrderInvoiceItem(invoiceItem);
                await UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = invoiceItem.OrderId,
                    Note = $"Refund request for invoice item #{invoiceItem.InvoiceItemId} within order item #{invoiceItem.OrderItemId} has been denied.",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
                {
                    InvoiceItemId = invoiceItem.InvoiceItemId,
                    CustomerId = currentCustomer.Id,
                    Description = "Refund request had been denied and the invoice item will not be refunded.",
                    Type = NexportRedemptionAuditLogTypeEnum.RefundRedemption,
                    UtcDateCreated = DateTime.UtcNow
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to refund Nexport order invoice item #{invoiceItem.Id} ({invoiceItem.InvoiceItemId})", ex);
        }

        return false;
    }

    public async Task RefundOrderItem(int quantity, int orderId, int orderItemId)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        try
        {
            var wholesaleOrderInfo = await GetWholesaleOrderInfoForOrderItemAsync(orderId, orderItemId);
            if (wholesaleOrderInfo != null)
            {
                var quantityToRefund = quantity <= wholesaleOrderInfo.Available ? quantity : wholesaleOrderInfo.Available;
                var orderInvoiceItems = await FindNexportOrderInvoiceItems(orderId, orderItemId);

                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                //TODO: Allow select individual invoice items to refund instead of randomizing

                var refundedInvoiceItems = orderInvoiceItems
                    .Where(x =>
                        x.RedemptionStatus != NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund
                        && x.RedemptionStatus != NexportOrderInvoiceItemRedemptionStatus.Refunded)
                    .Take(quantityToRefund);

                foreach (var invoiceItem in refundedInvoiceItems)
                {
                    if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund)
                        wholesaleOrderInfo.ProcessingRefund--;
                    else
                        wholesaleOrderInfo.Available--;

                    invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Refunded;
                    await UpdateNexportOrderInvoiceItem(invoiceItem);

                    await InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
                    {
                        InvoiceItemId = invoiceItem.InvoiceItemId,
                        CustomerId = currentCustomer.Id,
                        Description = "Invoice item had been refunded.",
                        Type = NexportRedemptionAuditLogTypeEnum.RefundRedemption,
                        UtcDateCreated = DateTime.UtcNow
                    });
                }

                //wholesaleOrderInfo.Available -= quantityToRefund;
                wholesaleOrderInfo.Refunded += quantityToRefund;
                await UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = orderId,
                    Note = $"Order item #{orderItemId} has been refunded. Total refund items: {quantity}.",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to refund order item #{orderItemId} in order #{orderId}", ex);
        }
    }

    public async Task<bool> HasWholesaleOrderInfo(Guid? groupId = null, Store store = null, Customer customer = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("groupId cannot be empty");

        var query = await FindWholesaleOrderInfoQuery(groupId, store, customer);

        return await query.AnyAsync();
    }

    public async Task<int> CountWholesaleOrderInfo(Guid? groupId = null, Store store = null, Customer customer = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("groupId cannot be empty");

        var query = await FindWholesaleOrderInfoQuery(groupId, store, customer);

        return await query.CountAsync();
    }

    public async Task<IQueryable<WholesaleOrderInfo>> FindWholesaleOrderInfoQuery(Guid? groupId = null, Store store = null, Customer customer = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("groupId cannot be empty");

        var orderInfoQuery = _wholesaleOrderInfoRepository.Table.Where(x => x.NexportGroupId == groupId);

        var orderIdList = new List<int>();
        IQueryable<Order> orderQuery = null;

        if (store != null)
            orderQuery = _orderRepository.Table.Where(x => x.StoreId == store.Id);

        if (customer != null)
            orderQuery = orderQuery != null ? orderQuery.Where(x => x.CustomerId == customer.Id) : _orderRepository.Table.Where(x => x.CustomerId == customer.Id);

        if (orderQuery != null)
            orderIdList.AddRange(await orderQuery.Select(x => x.Id).ToListAsync());

        if (store != null || customer != null)
            orderInfoQuery = orderInfoQuery.Where(x => orderIdList.Contains(x.OrderId));

        return orderInfoQuery;
    }

    public async Task MapProductToCategory(MapProductToCategoryModel model)
    {
        if (model.ProductId < 1)
            throw new ArgumentException("Product id cannot be less than 1");
        if (model.SelectedCategoryId < 1)
            throw new ArgumentException("Category id cannot be less than 1");

        var product = await _productService.GetProductByIdAsync(model.ProductId);

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var updateProductMapping = false;

        var productMapping = await GetProductMappingByNopProductId(product.Id, model.StoreId);

        if (productMapping != null)
        {
            updateProductMapping = true;
            productMapping.NexportCatalogId = Guid.Empty;
            productMapping.PricingModel = null;
            productMapping.PublishingModel = null;
            productMapping.CreditHours = null;
            productMapping.NexportSyllabusId = null;
            productMapping.NexportCatalogSyllabusLinkId = null;
            productMapping.UtcAvailableDate = null;
            productMapping.UtcEndDate = null;
            productMapping.UtcLastModifiedDate = null;
            productMapping.SectionCeus = null;
        }
        else
        {
            productMapping = new NexportProductMapping
            {
                NopProductId = product.Id
            };
        }

        productMapping.DisplayName = product.Name;
        productMapping.NexportProductName = "";
        productMapping.Type = NexportProductTypeEnum.OpenEnded;
        productMapping.NopCategoryId = model.SelectedCategoryId;
        productMapping.AssignWhenRedeemed = true;
        productMapping.StoreId = model.StoreId;

        if (updateProductMapping)
        {
            await UpdateNexportProductMapping(productMapping);
        }
        else
        {
            await InsertNexportProductMapping(productMapping);
        }
    }

    public async Task<IList<Product>> GetAllProductsByCategoryId(int? nopCategoryId)
    {
        if (nopCategoryId == null)
            throw new ArgumentNullException(nameof(nopCategoryId));

        var productIds = await _productCategoryMappingRepository.Table.Where(x => x.CategoryId == nopCategoryId).Select(x => x.ProductId).ToListAsync();

        var products = await _productRepository.Table.Where(x => productIds.Contains(x.Id)).ToListAsync();

        return products;
    }

    /// <summary>
    /// Gets all categories
    /// </summary>
    /// <param name="categoryName">Category name</param>
    /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <param name="overridePublished">
    /// null - process "Published" property according to "showHidden" parameter
    /// true - load only "Published" products
    /// false - load only "Unpublished" products
    /// </param>
    /// <param name="hasProductMapping"></param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the categories
    /// </returns>
    public virtual async Task<IPagedList<Category>> GetAllCategoriesAsync(string categoryName, int storeId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, bool? hasProductMapping = null)
    {
        var unsortedCategories = await _categoryRepository.GetAllAsync(async query =>
        {
            if (!showHidden)
                query = query.Where(c => c.Published);
            else if (overridePublished.HasValue)
                query = query.Where(c => c.Published == overridePublished.Value);

            if (!showHidden)
            {
                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
                query = await _aclService.ApplyAcl(query, customer);
            }

            if (hasProductMapping ?? false)
            {
                var mappingsQueryIdList = await _nexportProductMappingRepository.Table.Where(x => x.NopCategoryId != null).Select(x => x.NopCategoryId).ToListAsync();
                query = query.Where(c => mappingsQueryIdList.Contains(c.Id));
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(c => c.Name.Contains(categoryName));

            query = query.Where(c => !c.Deleted);

            return query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id);
        });

        //sort categories
        var sortedCategories = await SortCategoriesForTreeAsync(unsortedCategories);

        //paging
        return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
    }

    /// <summary>
    /// Sort categories for tree representation
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="parentId">Parent category identifier</param>
    /// <param name="ignoreCategoriesWithoutExistingParent">A value indicating whether categories without parent category in provided category list (source) should be ignored</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the sorted categories
    /// </returns>
    protected virtual async Task<IList<Category>> SortCategoriesForTreeAsync(IList<Category> source, int parentId = 0,
        bool ignoreCategoriesWithoutExistingParent = false)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = new List<Category>();

        foreach (var cat in source.Where(c => c.ParentCategoryId == parentId).ToList())
        {
            result.Add(cat);
            result.AddRange(await SortCategoriesForTreeAsync(source, cat.Id, true));
        }

        if (ignoreCategoriesWithoutExistingParent || result.Count == source.Count)
            return result;

        //find categories without parent in provided category source and insert them into result
        foreach (var cat in source)
            if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                result.Add(cat);

        return result;
    }

    public async Task<IPagedList<NexportGroupProduct>> GetWholesaleOrderInfosStatistics(string groupName,
        string shortName, string productName,
        NexportOrderInvoiceItemRedemptionStatus? redemptionStatus, int? customerId, int? storeId,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var orderQuery = _orderRepository.Table;

        if (storeId != null)
            orderQuery = orderQuery.Where(x => x.StoreId == storeId);

        if (customerId != null)
            orderQuery = orderQuery.Where(x => x.CustomerId == customerId);

        var groupsIdList = new List<Guid>();
        var noGroupStr = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Group.NoGroup");
        IQueryable<WholesalePurchasingGroup> groupsQuery = null;

        var includeGroupNotAssigned = false;

        if (!string.IsNullOrWhiteSpace(groupName))
        {
            groupsQuery = _wholesalePurchasingGroupRepository.Table
                .Where(x =>
                    x.NexportGroupName != null &&
                    x.NexportGroupName.Contains(groupName, StringComparison.OrdinalIgnoreCase));

            includeGroupNotAssigned = noGroupStr.Contains(groupName, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(shortName))
        {
            groupsQuery = groupsQuery != null
                ? groupsQuery.Where(x =>
                    x.NexportGroupShortName != null &&
                    x.NexportGroupShortName.Contains(shortName, StringComparison.OrdinalIgnoreCase))
                : _wholesalePurchasingGroupRepository.Table
                    .Where(x =>
                        x.NexportGroupShortName != null &&
                        x.NexportGroupShortName.Contains(shortName, StringComparison.OrdinalIgnoreCase));

            includeGroupNotAssigned = includeGroupNotAssigned || noGroupStr.Contains(shortName, StringComparison.OrdinalIgnoreCase);
        }

        if (groupsQuery != null)
            groupsIdList.AddRange(await groupsQuery.Select(x => x.NexportGroupId).ToListAsync());

        var wholesaleOrderQuery = _wholesaleOrderInfoRepository.Table;

        if (!string.IsNullOrWhiteSpace(groupName) || !string.IsNullOrWhiteSpace(shortName))
        {
            wholesaleOrderQuery = wholesaleOrderQuery
                .Where(x =>
                    (x.NexportGroupId != null && groupsIdList.Contains(x.NexportGroupId.Value)) ||
                    (x.NexportGroupId == null && includeGroupNotAssigned));
        }

        if (!string.IsNullOrWhiteSpace(productName))
        {
            var productsQuery = _productRepository.Table.Where(x => x.Name != null && x.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
            var productsIdList = await productsQuery.Select(x => x.Id).ToListAsync();
            wholesaleOrderQuery = wholesaleOrderQuery.Where(x => productsIdList.Contains(x.ProductId));
        }

        var orderInfoQuery = wholesaleOrderQuery
            .Join(orderQuery, x => x.OrderId, order => order.Id,
                (wholesaleOrderWithNexportInvoice, order) => new { wholesaleOrderWithNexportInvoice, order })
            .Select(x => x.wholesaleOrderWithNexportInvoice)
            .Distinct()
            .GroupBy(x => new { x.NexportGroupId, x.ProductId })
            .Select(x =>
                new NexportGroupProduct
                {
                    Id = x.FirstOrDefault().Id,
                    NexportGroupId = x.FirstOrDefault().NexportGroupId,
                    ProductId = x.FirstOrDefault()!.ProductId,
                    Available = x.Sum(y => y.Available),
                    Awaiting = x.Sum(y => y.Awaiting),
                    Redeemed = x.Sum(y => y.Redeemed),
                    Refunded = x.Sum(y => y.Refunded),
                    ProcessingAvailable = x.Sum(y => y.ProcessingAvailable),
                    ProcessingAwaiting = x.Sum(y => y.ProcessingAwaiting),
                    ProcessingRefund = x.Sum(y => y.ProcessingRefund),
                    ApprovalAwaiting = x.Sum(y => y.ApprovalAwaiting),
                    FundingPoolId = x.FirstOrDefault()!.FundingPoolId,
                    UtcRedeemByDate = x.FirstOrDefault()!.UtcRedeemByDate,
                }
            );

        if (redemptionStatus != null)
        {
            orderInfoQuery = redemptionStatus switch
            {
                NexportOrderInvoiceItemRedemptionStatus.Available => orderInfoQuery.Where(x => x.Available > 0),
                NexportOrderInvoiceItemRedemptionStatus.Awaiting => orderInfoQuery.Where(x => x.Awaiting > 0),
                NexportOrderInvoiceItemRedemptionStatus.Assigned => orderInfoQuery.Where(x => x.Redeemed > 0),
                NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable => orderInfoQuery.Where(x => x.ProcessingAvailable > 0),
                NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting => orderInfoQuery.Where(x => x.ProcessingAwaiting > 0),
                NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund => orderInfoQuery.Where(x => x.ProcessingRefund > 0),
                NexportOrderInvoiceItemRedemptionStatus.ApprovalAwaiting => orderInfoQuery.Where(x => x.ApprovalAwaiting > 0),
                NexportOrderInvoiceItemRedemptionStatus.Refunded => orderInfoQuery.Where(x => x.Refunded > 0),
                _ => orderInfoQuery
            };
        }

        return new PagedList<NexportGroupProduct>(await orderInfoQuery.ToListAsync(), pageIndex, pageSize);
    }

    public async Task<IPagedList<NexportGroupProduct>> GetAllWholesaleOrderInfosByFundingPoolsAsync(
        string fundingPoolName,
        NexportOrderInvoiceItemRedemptionStatus? redemptionStatus, int? customerId, int? storeId,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var orderQuery = _orderRepository.Table;

        if (storeId != null)
            orderQuery = orderQuery.Where(x => x.StoreId == storeId);

        if (customerId != null)
            orderQuery = orderQuery.Where(x => x.CustomerId == customerId);

        var wholesaleOrderQuery = _wholesaleOrderInfoRepository.Table;
        if (fundingPoolName != null)
        {
            var fundingPoolQuery = _nexportFundingPoolRepository.Table.Where(x => x.Name.Contains(fundingPoolName));

            var fundingPoolIds = await fundingPoolQuery.Select(x => x.Id).ToListAsync();
            wholesaleOrderQuery = wholesaleOrderQuery.Where(x => x.FundingPoolId != null && fundingPoolIds.Contains(x.FundingPoolId.Value));
        }

        var orderInfoQuery = wholesaleOrderQuery
            .Join(orderQuery, x => x.OrderId, order => order.Id,
                (wholesaleOrderWithNexportInvoice, order) => new { wholesaleOrderWithNexportInvoice, order })
            .Select(x => x.wholesaleOrderWithNexportInvoice)
            .Distinct()
            .GroupBy(x => new { x.FundingPoolId })
            .Select(x =>
                new NexportGroupProduct
                {
                    Id = x.FirstOrDefault().Id,
                    NexportGroupId = x.FirstOrDefault().NexportGroupId,
                    ProductId = x.FirstOrDefault()!.ProductId,
                    Available = x.Sum(y => y.Available),
                    Awaiting = x.Sum(y => y.Awaiting),
                    Redeemed = x.Sum(y => y.Redeemed),
                    Refunded = x.Sum(y => y.Refunded),
                    ProcessingAvailable = x.Sum(y => y.ProcessingAvailable),
                    ProcessingAwaiting = x.Sum(y => y.ProcessingAwaiting),
                    ProcessingRefund = x.Sum(y => y.ProcessingRefund),
                    ApprovalAwaiting = x.Sum(y => y.ApprovalAwaiting),
                    FundingPoolId = x.FirstOrDefault()!.FundingPoolId,
                    UtcRedeemByDate = x.FirstOrDefault()!.UtcRedeemByDate,
                }
            );

        if (redemptionStatus != null)
        {
            orderInfoQuery = redemptionStatus switch
            {
                NexportOrderInvoiceItemRedemptionStatus.Available => orderInfoQuery.Where(x => x.Available > 0),
                NexportOrderInvoiceItemRedemptionStatus.Awaiting => orderInfoQuery.Where(x => x.Awaiting > 0),
                NexportOrderInvoiceItemRedemptionStatus.Assigned => orderInfoQuery.Where(x => x.Redeemed > 0),
                NexportOrderInvoiceItemRedemptionStatus.ProcessingAvailable => orderInfoQuery.Where(x => x.ProcessingAvailable > 0),
                NexportOrderInvoiceItemRedemptionStatus.ProcessingAwaiting => orderInfoQuery.Where(x => x.ProcessingAwaiting > 0),
                NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund => orderInfoQuery.Where(x => x.ProcessingRefund > 0),
                NexportOrderInvoiceItemRedemptionStatus.ApprovalAwaiting => orderInfoQuery.Where(x => x.ApprovalAwaiting > 0),
                NexportOrderInvoiceItemRedemptionStatus.Refunded => orderInfoQuery.Where(x => x.Refunded > 0),
                _ => orderInfoQuery
            };
        }

        return new PagedList<NexportGroupProduct>(await orderInfoQuery.ToListAsync(), pageIndex, pageSize);
    }

    public async Task InsertRedemptionUnassignmentRequestAsync(NexportRedemptionUnassignmentRequest unassignmentRequest)
    {
        if (unassignmentRequest == null)
            throw new ArgumentNullException(nameof(unassignmentRequest));

        if (_nexportRedemptionUnassignmentRequestRepository.Table.Any(x =>
                x.InvoiceItemId == unassignmentRequest.InvoiceItemId &&
                x.RequestStatus == NexportRedemptionUnassignmentRequestStatus.Received))
            throw new Exception("Unable to add new unassignment request entity due to previous request status!");

        await _nexportRedemptionUnassignmentRequestRepository.InsertAsync(unassignmentRequest);
    }

    public async Task AddRedemptionUnassignmentRequestTokensAsync(IList<Token> tokens, NexportRedemptionUnassignmentRequest unassignmentRequest, NexportOrderInvoiceItem invoiceItem)
    {
        tokens.Add(new Token("UnassignmentRequest.Id", unassignmentRequest.Id));
        tokens.Add(new Token("UnassignmentRequest.InvoiceItemId", invoiceItem.Id));
        //tokens.Add(new Token("UnassignmentRequest.Reason", cancellationRequest.ReasonForCancellation));
        tokens.Add(new Token("UnassignmentRequest.CustomerComment",
            _htmlFormatter.FormatText(unassignmentRequest.CustomerComments, false, true, false, false, false, false), true));
        tokens.Add(new Token("UnassignmentRequest.StaffNotes",
            _htmlFormatter.FormatText(unassignmentRequest.StaffNotes, false, true, false, false, false, false), true));
        tokens.Add(new Token("UnassignmentRequest.Status", await _localizationService.GetLocalizedEnumAsync(unassignmentRequest.RequestStatus)));
    }

    /// <summary>
    /// Get EmailAccount to use with a message templates
    /// </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="languageId">Language identifier</param>
    /// <returns>EmailAccount</returns>
    private async Task<EmailAccount> GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = await (_emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ??
                                  _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                           (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
        return emailAccount;
    }

    public async Task<IList<int>> SendNewRedemptionUnassignmentRequestStoreOwnerNotificationAsync(NexportRedemptionUnassignmentRequest unassignmentRequest,
        NexportOrderInvoiceItem invoiceItem, int languageId)
    {
        if (unassignmentRequest == null)
            throw new ArgumentNullException(nameof(unassignmentRequest));

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(NexportDefaults.NEW_REDEMPTION_UNASSIGNMENT_REQUEST_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var customer = await _customerService.GetCustomerByIdAsync(unassignmentRequest.RequestedByCustomerId)
                       ?? throw new Exception($"Customer with Id {unassignmentRequest.RequestedByCustomerId} does not existed");

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
        await AddRedemptionUnassignmentRequestTokensAsync(commonTokens, unassignmentRequest, invoiceItem);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return await _workflowMessageService
                .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendNewRedemptionUnassignmentRequestCustomerNotificationAsync(NexportRedemptionUnassignmentRequest unassignmentRequest,
       NexportOrderInvoiceItem invoiceItem)
    {
        if (unassignmentRequest == null)
            throw new ArgumentNullException(nameof(unassignmentRequest));
        var invoiceOrder = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);

        var store = await _storeService.GetStoreByIdAsync(invoiceOrder.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

        var languageId = invoiceOrder.CustomerLanguageId;

        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(NexportDefaults.NEW_REDEMPTION_UNASSIGNMENT_REQUEST_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var customer = await _customerService.GetCustomerByIdAsync(unassignmentRequest.RequestedByCustomerId)
                       ?? throw new Exception($"Customer with Id {unassignmentRequest.RequestedByCustomerId} does not existed");

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
        await AddRedemptionUnassignmentRequestTokensAsync(commonTokens, unassignmentRequest, invoiceItem);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(invoiceOrder.BillingAddressId);

            var customerIsGuest = await _customerService.IsGuestAsync(customer);
            var toEmail = customerIsGuest
                ? billingAddress.Email
                : customer.Email;
            var toName = customerIsGuest
                ? billingAddress.FirstName
                : await _customerService.GetCustomerFullNameAsync(customer);

            return await _workflowMessageService
                .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IPagedList<NexportRedemptionUnassignmentRequest>> GetAllNexportRedemptionUnassignmentRequests(
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _nexportRedemptionUnassignmentRequestRepository.GetAllPagedAsync(x => x, pageIndex, pageSize);
    }

    public async Task<NexportRedemptionUnassignmentRequest> GetNexportRedemptionUnassignmentRequestByIdAsync(int? requestId)
    {
        return await _nexportRedemptionUnassignmentRequestRepository.GetByIdAsync(requestId);
    }

    public async Task<IList<NexportRedemptionUnassignmentRequestReason>> GetAllRedemptionUnassignmentRequestReasonsAsync()
    {
        var query =
            _nexportRedemptionUnassignmentRequestReasonRepository
                .Table
                .OrderBy(reason => reason.DisplayOrder)
                .ThenBy(reason => reason.Id);
        return await query.ToListAsync();
    }

    public async Task<NexportRedemptionUnassignmentRequestReason> GetNexportRedemptionUnassignmentRequestReasonByIdAsync(int? reasonId)
    {
        return await _nexportRedemptionUnassignmentRequestReasonRepository.GetByIdAsync(reasonId);
    }

    public async Task UpdateNexportRedemptionUnassignmentRequestReasonAsync(NexportRedemptionUnassignmentRequestReason unassignmentRequestReason)
    {
        if (unassignmentRequestReason == null)
            throw new ArgumentNullException(nameof(unassignmentRequestReason));

        await _nexportRedemptionUnassignmentRequestReasonRepository.UpdateAsync(unassignmentRequestReason);
    }

    public async Task DeleteUnassignmentRequestReasonAsync(NexportRedemptionUnassignmentRequestReason unassignmentRequestReason)
    {
        if (unassignmentRequestReason == null)
            throw new ArgumentNullException(nameof(unassignmentRequestReason));

        if (_nexportRedemptionUnassignmentRequestReasonRepository.Table.Count() == 1)
            throw new NopException("You cannot delete unassignment request reason. At least one unassignment request reason is required.");

        await _nexportRedemptionUnassignmentRequestReasonRepository.DeleteAsync(unassignmentRequestReason);
    }

    public async Task InsertNexportRedemptionUnassignmentRequestReasonAsync(NexportRedemptionUnassignmentRequestReason unassignmentRequestReason)
    {
        if (unassignmentRequestReason == null)
            throw new ArgumentNullException(nameof(unassignmentRequestReason));

        await _nexportRedemptionUnassignmentRequestReasonRepository.InsertAsync(unassignmentRequestReason);
    }

    public async Task DeleteNexportRedemptionUnassignmentRequestAsync(NexportRedemptionUnassignmentRequest unassignmentRequest)
    {
        if (unassignmentRequest == null)
            throw new ArgumentNullException(nameof(unassignmentRequest));

        await _nexportRedemptionUnassignmentRequestRepository.DeleteAsync(unassignmentRequest);
    }

    public async Task UpdateNexportRedemptionUnassignmentRequestAsync(NexportRedemptionUnassignmentRequest unassignmentRequest)
    {
        if (unassignmentRequest == null)
            throw new ArgumentNullException(nameof(unassignmentRequest));

        await _nexportRedemptionUnassignmentRequestRepository.UpdateAsync(unassignmentRequest);
    }

    public async Task<IList<int>> SendRedemptionUnassignmentRequestCustomerNotificationAsync(NexportRedemptionUnassignmentRequest unassignmentRequest,
        NexportOrderInvoiceItem invoiceItem, string template)
    {
        var invoiceOrder = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
        var languageId = invoiceOrder.CustomerLanguageId;
        if (unassignmentRequest == null)
            throw new ArgumentNullException(nameof(unassignmentRequest));

        var store = await _storeService.GetStoreByIdAsync(invoiceOrder.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(template, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var customer = await _customerService.GetCustomerByIdAsync(unassignmentRequest.RequestedByCustomerId)
                       ?? throw new Exception($"Customer with Id {unassignmentRequest.RequestedByCustomerId} does not existed");

        var commonTokens = new List<Token>();


        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
        await AddRedemptionUnassignmentRequestTokensAsync(commonTokens, unassignmentRequest, invoiceItem);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(invoiceOrder.BillingAddressId);

            var customerIsGuest = await _customerService.IsGuestAsync(customer);
            var toEmail = customerIsGuest
                ? billingAddress.Email
                : customer.Email;
            var toName = customerIsGuest
                ? billingAddress.FirstName
                : await _customerService.GetCustomerFullNameAsync(customer);

            return await _workflowMessageService
                .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IPagedList<NexportRedemptionUnassignmentRequest>> SearchUnassignmentRequestsAsync(int storeId = 0,
        int customerId = 0,
        NexportRedemptionUnassignmentRequestStatus? requestStatus = null,
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _nexportRedemptionUnassignmentRequestRepository.Table;

        //if (storeId > 0)
        //    query = query.Where(request => storeId == request.StoreId);
        if (customerId > 0)
            query = query.Where(request => customerId == request.RequestedByCustomerId);

        if (requestStatus.HasValue)
        {
            var returnStatusId = (int)requestStatus.Value;
            query = query.Where(request => (int)request.RequestStatus == returnStatusId);
        }

        if (createdFromUtc.HasValue)
            query = query.Where(request => createdFromUtc.Value <= request.UtcCreatedDate);
        if (createdToUtc.HasValue)
            query = query.Where(request => createdToUtc.Value >= request.UtcCreatedDate);

        query = query.OrderByDescending(request => request.UtcCreatedDate)
            .ThenByDescending(request => request.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<NexportRedemptionUnassignmentRequest> FindRecentNexportRedemptionUnassignmentRequest(
        Guid invoiceItemId)
    {
        return await _nexportRedemptionUnassignmentRequestRepository
            .Table
            .Where(x =>
                x.InvoiceItemId == invoiceItemId &&
                x.RequestStatus == NexportRedemptionUnassignmentRequestStatus.Received)
            .OrderByDescending(x => x.UtcCreatedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<NexportRedemptionAssignmentApprovalRequest> GetNexportRedemptionAssignmentApprovalRequestByIdAsync(int? requestId)
    {
        return await _nexportRedemptionAssignmentApprovalRequestRepository.GetByIdAsync(requestId);
    }

    public async Task InsertNexportRedemptionAssignmentApprovalRequestAsync(NexportRedemptionAssignmentApprovalRequest assignmentApprovalRequest)
    {
        if (assignmentApprovalRequest == null)
            throw new ArgumentNullException(nameof(assignmentApprovalRequest));

        await _nexportRedemptionAssignmentApprovalRequestRepository.InsertAsync(assignmentApprovalRequest);
    }

    public async Task UpdateNexportRedemptionAssignmentApprovalRequestAsync(NexportRedemptionAssignmentApprovalRequest assignmentApprovalRequest)
    {
        if (assignmentApprovalRequest == null)
            throw new ArgumentNullException(nameof(assignmentApprovalRequest));

        await _nexportRedemptionAssignmentApprovalRequestRepository.UpdateAsync(assignmentApprovalRequest);
    }

    public async Task DeleteNexportRedemptionAssignmentApprovalRequestAsync(NexportRedemptionAssignmentApprovalRequest assignmentApprovalRequest)
    {
        if (assignmentApprovalRequest == null)
            throw new ArgumentNullException(nameof(assignmentApprovalRequest));

        await _nexportRedemptionAssignmentApprovalRequestRepository.DeleteAsync(assignmentApprovalRequest);
    }

    public async Task<IList<int>> SendNexportRedemptionAssignmentApprovalRequestCustomerNotificationAsync(
        NexportRedemptionAssignmentApprovalRequest assignmentApprovalRequest,
        NexportOrderInvoiceItem invoiceItem, string template)
    {
        var invoiceOrder = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
        var languageId = invoiceOrder.CustomerLanguageId;
        if (assignmentApprovalRequest == null)
            throw new ArgumentNullException(nameof(assignmentApprovalRequest));

        var store = await _storeService.GetStoreByIdAsync(invoiceOrder.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(template, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var customer = await _customerService.GetCustomerByIdAsync(assignmentApprovalRequest.RequestedByCustomerId)
                       ?? throw new Exception($"Customer with Id {assignmentApprovalRequest.RequestedByCustomerId} does not existed");

        var commonTokens = new List<Token>();

        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
        await AddRedemptionAssignmentApprovalRequestTokensAsync(commonTokens, assignmentApprovalRequest, invoiceItem);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(invoiceOrder.BillingAddressId);

            var customerIsGuest = await _customerService.IsGuestAsync(customer);
            var toEmail = customerIsGuest
                ? billingAddress.Email
                : customer.Email;
            var toName = customerIsGuest
                ? billingAddress.FirstName
                : await _customerService.GetCustomerFullNameAsync(customer);

            return await _workflowMessageService
                .SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IPagedList<NexportRedemptionAssignmentApprovalRequest>> SearchNexportRedemptionAssignmentApprovalRequestsAsync(
        int storeId = 0,
        int customerId = 0,
        NexportRedemptionAssignmentApprovalRequestStatus? requestStatus = null,
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _nexportRedemptionAssignmentApprovalRequestRepository.Table;

        if (customerId > 0)
            query = query.Where(request => customerId == request.RequestedByCustomerId);

        if (requestStatus.HasValue)
        {
            var statusId = (int)requestStatus.Value;
            query = query.Where(request => (int)request.Status == statusId);
        }

        if (createdFromUtc.HasValue)
            query = query.Where(request => createdFromUtc.Value <= request.UtcCreatedDate);
        if (createdToUtc.HasValue)
            query = query.Where(request => createdToUtc.Value >= request.UtcCreatedDate);

        query = query
            .OrderByDescending(request => request.UtcCreatedDate)
            .ThenByDescending(request => request.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    protected async Task AddRedemptionAssignmentApprovalRequestTokensAsync(IList<Token> tokens, NexportRedemptionAssignmentApprovalRequest assignmentApprovalRequest, NexportOrderInvoiceItem invoiceItem)
    {
        tokens.Add(new Token("AssignmentApprovalRequest.Id", assignmentApprovalRequest.Id));
        tokens.Add(new Token("AssignmentApprovalRequest.InvoiceItemId", invoiceItem.Id));
        tokens.Add(new Token("AssignmentApprovalRequest.Notes",
            _htmlFormatter.FormatText(assignmentApprovalRequest.Notes, false, true, false, false, false, false), true));
        tokens.Add(new Token("AssignmentApprovalRequest.Status", await _localizationService.GetLocalizedEnumAsync(assignmentApprovalRequest.Status)));
    }

    public async Task InsertNexportRedemptionAuditLogAsync(NexportRedemptionAuditLog redemptionAuditLog)
    {
        if (redemptionAuditLog == null)
            throw new ArgumentNullException(nameof(redemptionAuditLog));

        await _nexportRedemptionAuditLogRepository.InsertAsync(redemptionAuditLog);
    }

    public async Task DeleteNexportRedemptionAuditLogAsync(NexportRedemptionAuditLog redemptionAuditLog)
    {
        if (redemptionAuditLog == null)
            throw new ArgumentNullException(nameof(redemptionAuditLog));

        await _nexportRedemptionAuditLogRepository.DeleteAsync(redemptionAuditLog);
    }

    public async Task<IPagedList<NexportRedemptionAuditLog>> SearchNexportRedemptionAuditLogs(
        Guid? invoiceItemId = null, int? customerId = null,
        NexportRedemptionAuditLogTypeEnum? logType = null,
        string customerName = null, string targetCustomerName = null,
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _nexportRedemptionAuditLogRepository.Table;

        if (invoiceItemId != null)
            query = query.Where(x => invoiceItemId == x.InvoiceItemId);

        if (customerId != null)
            query = query.Where(x => customerId == x.CustomerId);

        if (logType.HasValue)
        {
            query = query.Where(x => x.Type == logType.Value);
        }

        var customerQuery = _customerRepository.Table;

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            customerQuery = customerQuery.Where(x => x.FirstName.Contains(customerName) || x.LastName.Contains(customerName));
            var customerIdList = await customerQuery.Select(x => x.Id).ToListAsync();
            if (customerIdList.Count != 0)
            {
                var nexportUserQuery =
                    _nexportUserMappingRepository.Table.Where(x => customerIdList.Contains(x.NopUserId));

                var nexportUserIdList = await nexportUserQuery.Select(x => x.NopUserId).ToListAsync();

                query = query.Where(x => nexportUserIdList.Contains(x.CustomerId));
            }
        }

        if (!string.IsNullOrWhiteSpace(targetCustomerName))
        {
            customerQuery = customerQuery.Where(x => x.FirstName.Contains(targetCustomerName) || x.LastName.Contains(targetCustomerName));
            var customerIdList = await customerQuery.Select(x => x.Id).ToListAsync();
            if (customerIdList.Count != 0)
            {
                var nexportUserQuery =
                    _nexportUserMappingRepository.Table.Where(x => customerIdList.Contains(x.NopUserId));

                var nexportUserIdList = await nexportUserQuery.Select(x => x.NopUserId).ToListAsync();

                query = query.Where(x => x.TargetedCustomerId != null && nexportUserIdList.Contains(x.TargetedCustomerId.Value));
            }
        }

        if (createdFromUtc.HasValue)
            query = query.Where(x => createdFromUtc.Value <= x.UtcDateCreated);

        if (createdToUtc.HasValue)
            query = query.Where(x => createdToUtc.Value >= x.UtcDateCreated);

        query = query.OrderByDescending(x => x.UtcDateCreated).ThenByDescending(x => x.Id);

        var logs = await query.ToPagedListAsync(pageIndex, pageSize);

        return logs;
    }

    public virtual async Task<IPagedList<ReturnRequest>> SearchReturnRequestsAsync(int storeId = 0, int customerId = 0,
        int orderItemId = 0, string customNumber = "", ReturnRequestStatus? requestStatus = null,
        DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false,
        bool includeOnlyNexportPurchases = false)
    {
        var query = _returnRequestRepository.Table;
        if (storeId > 0)
            query = query.Where(rr => storeId == rr.StoreId);
        if (customerId > 0)
            query = query.Where(rr => customerId == rr.CustomerId);
        if (requestStatus.HasValue)
        {
            var returnStatusId = (int)requestStatus.Value;
            query = query.Where(rr => rr.ReturnRequestStatusId == returnStatusId);
        }

        if (orderItemId > 0)
            query = query.Where(rr => rr.OrderItemId == orderItemId);

        if (!string.IsNullOrEmpty(customNumber))
            query = query.Where(rr => rr.CustomNumber == customNumber);

        if (createdFromUtc.HasValue)
            query = query.Where(rr => createdFromUtc.Value <= rr.CreatedOnUtc);
        if (createdToUtc.HasValue)
            query = query.Where(rr => createdToUtc.Value >= rr.CreatedOnUtc);

        query = query.OrderByDescending(rr => rr.CreatedOnUtc).ThenByDescending(rr => rr.Id);

        var returnRequests = await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);

        return returnRequests;
    }
}