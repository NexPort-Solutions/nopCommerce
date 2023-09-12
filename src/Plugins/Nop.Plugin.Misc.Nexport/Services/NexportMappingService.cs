using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Infrastructure.CustomExceptions;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;

namespace Nop.Plugin.Misc.Nexport.Services
{
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

        public async Task<NexportProductMapping?> GetProductMappingByNopProductId(int nopProductId, int? storeId = null)
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

            var key = _cacheManager.PrepareKeyForDefaultCache(NexportIntegrationDefaults.ProductGroupMembershipMappingsAllCacheKey,
                (await _storeContext.GetCurrentStoreAsync()).Id,
                string.Join(",", await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync())),
                false, "", false);

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

        public async Task InsertOrUpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (await _nexportOrderInvoiceItemRepository.Table.AnyAsync(q => q.OrderId == item.OrderId &&
                                                                             q.OrderItemId == item.OrderItemId && q.InvoiceItemId == item.InvoiceItemId))
            {
                try
                {
                    //TODO @JS this should change when we have the reset redemption api call
                    var currentInvoiceItems = await FindNexportOrderInvoiceItems(item.OrderId, item.OrderItemId);
                    NexportOrderInvoiceItem? invoiceToUpdate = null;
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
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Cannot update new invoice item for the order item {item.OrderItemId} in order {item.OrderId}", ex);
                }
            }
            else
            {
                try
                {
                    await _nexportOrderInvoiceItemRepository.InsertAsync(item);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Cannot add new Nexport order invoice item for the order item {item.OrderItemId} in order {item.OrderId}", ex);
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

        public async Task UpdateNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            await _nexportOrderInvoiceRedemptionQueueRepository.UpdateAsync(queueItem);
        }

        public async Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItem(int orderId, int orderItemId)
        {
            if (orderId < 1)
                return null;

            return orderItemId < 1
                ? null
                : await _nexportOrderInvoiceItemRepository
                    .Table.SingleOrDefaultAsync(o => o.OrderId == orderId && o.OrderItemId == orderItemId);
        }

        public async Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItemByInvoiceItemGuid(Guid invoiceItemId)
        {
            if (invoiceItemId == Guid.Empty)
                return null;

            return await _nexportOrderInvoiceItemRepository.Table.SingleOrDefaultAsync(o => o.InvoiceItemId == invoiceItemId);
        }

        public async Task<IList<NexportOrderInvoiceItem>?> FindNexportOrderInvoiceItems(int orderId, int orderItemId)
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

                switch (model.NexportProductType)
                {
                    case NexportProductTypeEnum.Catalog:
                        var catalogDetails = await GetCatalogDetailsAsync(model.NexportProductId);
                        var catalogCreditHours = await GetCatalogCreditHoursAsync(model.NexportProductId);

                        productMapping.NexportProductName = catalogDetails?.Name;
                        productMapping.NexportCatalogId = model.NexportCatalogId;
                        productMapping.PricingModel = catalogDetails?.PricingModel;
                        productMapping.PublishingModel = catalogDetails?.PublishingModel;
                        productMapping.CreditHours = catalogCreditHours?.CreditHours;
                        if (model.AssignWhenRedeemed.HasValue && model.AssignWhenRedeemed.Value)
                            productMapping.AssignWhenRedeemed = model.AssignWhenRedeemed.Value;

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

            var left = _storeRepository.Table.Where(s=>!s.Deleted)
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

        public async Task<IList<Order>> GetOrdersForGroupId(Guid groupId)
        {
            var orders = await _genericAttributeRepository.Table.Where(x =>
                x.Key == "GroupForOrder" && x.Value.Contains($"\"Id\":\"{groupId}\""))
                    .Join(_orderRepository.Table,
                gar => gar.EntityId, or => or.Id, (gar, or) => or).ToListAsync();
            return orders;
        }

        public async Task<int> GetInvoiceItemCountForGroupByGuid(Guid groupId)
        {
            var count = await _genericAttributeRepository.Table.Where(x =>
                    x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""))
                .Join(_orderRepository.Table,
                    gar => gar.EntityId, or => or.Id, (gar, or) => new { gar, or })
                .Join(_orderItemRepository.Table, garor => garor.or.Id, ori => ori.OrderId,
                    (garor, ori) => ori).Join(_nexportOrderInvoiceItemRepository.Table, ori => ori.Id, inv => inv.OrderItemId, (ori, inv) => inv).CountAsync();
            return count;
        }

        public async Task<NexportOrderInvoiceItem?> FindNexportOrderInvoiceItemByGuid(Guid? orderInvoiceItemId)
        {
            if (orderInvoiceItemId == null)
                throw new ArgumentNullException(nameof(orderInvoiceItemId));

            return _nexportOrderInvoiceItemRepository.Table.SingleOrDefault(x =>
                x.InvoiceItemId == orderInvoiceItemId);
        }
        public async Task<Customer> FindCustomerByGuid(Guid? customerId)
        {
            if (customerId == null)
                throw new ArgumentNullException(nameof(customerId));

            return await _customerService.GetCustomerByGuidAsync(customerId.Value);
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
                .Where(x=>x.RedeemingUserId.HasValue);

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
                .Where(x=>!(x.RedeemingUserId.HasValue && x.UtcDateRedemption.HasValue));

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IList<GenericAttribute?>> GetAllGroupForOrdersAsync()
        {
            var query = _genericAttributeRepository.Table.AsEnumerable()
                .Where(x => x.Key == "GroupForOrder").GroupBy(x => x.Value).Select(x => x.FirstOrDefault());
            var result = await query.ToListAsync();
            return result ?? new List<GenericAttribute?>();

        }

        public async Task<GenericAttribute?> GetGroupByGroupIdAsync(Guid groupId)
        {

            var attr = _genericAttributeRepository.Table.FirstOrDefault(x => x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""));

            return attr;
        }

        public async Task<int> GetAvailableNexportGroupProductRedemptionsCount(Guid groupId, int productId) 
        {
            var query = _genericAttributeRepository.Table
                .Where(x => x.Key == "GroupForOrder" && x.Value.Contains("{\"Id\":\"" + groupId + "\""))
                .Join(_orderItemRepository.Table, gar => gar.EntityId, ori => ori.OrderId, (gar, ori) => ori)
                .Join(_productRepository.Table, ori => ori.ProductId, pr => pr.Id, (ori, pr) => new { ori, pr })
                .Where(x => x.pr.Id == productId)
                .Join(_nexportOrderInvoiceItemRepository.Table, oripr => new { a = oripr.ori.OrderId, b = oripr.ori.Id },
                    noii => new { a = noii.OrderId, b = noii.OrderItemId },
                    (oripr, noii) => noii)
                .Where(x=>x.RedeemingUserId==null);

            return await query.CountAsync();
        }
    }
}