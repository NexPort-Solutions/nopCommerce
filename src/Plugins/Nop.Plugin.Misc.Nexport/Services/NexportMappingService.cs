using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public partial class NexportService : INexportService
    {
        public void InsertNexportProductMapping(NexportProductMapping nexportProductMapping)
        {
            if (nexportProductMapping == null)
                throw new ArgumentNullException(nameof(nexportProductMapping));

            if (_nexportProductMappingRepository.Table.Any(m =>
                m.NopProductId == nexportProductMapping.NopProductId &&
                m.StoreId == nexportProductMapping.StoreId))
                return;

            _nexportProductMappingRepository.Insert(nexportProductMapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityInserted(nexportProductMapping);
        }

        public void InsertNexportProductGroupMembershipMapping(
            NexportProductGroupMembershipMapping nexportProductGroupMembershipMapping)
        {
            if (nexportProductGroupMembershipMapping == null)
                throw new ArgumentNullException(nameof(nexportProductGroupMembershipMapping));

            if (_nexportProductGroupMembershipMappingRepository.Table.Any(
                m => m.NexportProductMappingId == nexportProductGroupMembershipMapping.NexportProductMappingId &&
                m.NexportGroupId == nexportProductGroupMembershipMapping.NexportGroupId))
                return;

            _nexportProductGroupMembershipMappingRepository.Insert(nexportProductGroupMembershipMapping);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductGroupMembershipMappingPatternCacheKey);

            _eventPublisher.EntityInserted(nexportProductGroupMembershipMapping);
        }

        public IPagedList<NexportProductMapping> GetProductCatalogsByCatalogId(Guid catalogId, int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            if (catalogId == Guid.Empty)
            {
                return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
            }

            var key = NexportProductDefaults.GetProductMappingCatalogAllByCatalogIdCacheKey(
                showHidden, catalogId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                //var query = from np in _nexportProductMappingRepository.Table
                //    join p in _productRepository.Table on np.NopProductId equals p.Id
                //    where np.NexportCatalogId == catalogId && !p.Deleted && (showHidden || p.Published)
                //    select np;

                var productQuery = (from p in _productRepository.Table
                                    where !p.Deleted && (showHidden || p.Published)
                                    select p.Id).ToList();

                var query = from np in _nexportProductMappingRepository.Table
                            where np.NexportCatalogId == catalogId && productQuery.Contains(np.NopProductId)
                            select np;
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

                var productCatalogs = new PagedList<NexportProductMapping>(query, pageIndex, pageSize);
                return productCatalogs;
            });
        }

        public IPagedList<NexportProductMapping> GetProductSectionsBySectionId(Guid sectionId, int pageIndex = 0, int pageSize = Int32.MaxValue,
            bool showHidden = false)
        {
            if (sectionId == Guid.Empty)
            {
                return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
            }

            var key = NexportProductDefaults.GetProductMappingSectionAllBySectionIdCacheKey(
                showHidden, sectionId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
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

                var productCatalogs = new PagedList<NexportProductMapping>(query, pageIndex, pageSize);
                return productCatalogs;
            });
        }

        public IPagedList<NexportProductMapping> GetProductTrainingPlansByTrainingPlanId(Guid trainingPlanId, int pageIndex = 0, int pageSize = Int32.MaxValue,
            bool showHidden = false)
        {
            if (trainingPlanId == Guid.Empty)
            {
                return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
            }

            var key = NexportProductDefaults.GetProductMappingTrainingPlanAllByTrainingPlanIdCacheKey(
                showHidden, trainingPlanId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var productQuery = (from p in _productRepository.Table
                                    where !p.Deleted && (showHidden || p.Published)
                                    select p.Id).ToList();

                var query = from np in _nexportProductMappingRepository.Table
                            where np.NexportSyllabusId == trainingPlanId &&
                                  np.Type == NexportProductTypeEnum.TrainingPlan &&
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

                var productCatalogs = new PagedList<NexportProductMapping>(query, pageIndex, pageSize);
                return productCatalogs;
            });
        }

        public NexportProductMapping FindProductCatalog(IList<NexportProductMapping> source, int productId, Guid catalogId, int? storeId = null)
        {
            return source.FirstOrDefault(productCatalog => productCatalog.NopProductId == productId &&
                                                           productCatalog.NexportCatalogId == catalogId &&
                                                           productCatalog.StoreId == storeId);
        }

        public NexportProductMapping FindProductSection(IList<NexportProductMapping> source, int productId, Guid sectionId, int? storeId = null)
        {
            return source.FirstOrDefault(productSectionMapping => productSectionMapping.NopProductId == productId &&
                                                           productSectionMapping.NexportSyllabusId == sectionId &&
                                                           productSectionMapping.Type == NexportProductTypeEnum.Section &&
                                                           productSectionMapping.StoreId == storeId);
        }

        public NexportProductMapping FindProductTrainingPlan(IList<NexportProductMapping> source, int productId, Guid trainingPlanId, int? storeId = null)
        {
            return source.FirstOrDefault(productTrainingPlanMapping => productTrainingPlanMapping.NopProductId == productId &&
                                                           productTrainingPlanMapping.NexportSyllabusId == trainingPlanId &&
                                                           productTrainingPlanMapping.Type == NexportProductTypeEnum.TrainingPlan &&
                                                           productTrainingPlanMapping.StoreId == storeId);
        }

        public IPagedList<NexportProductMapping> GetProductMappingsPagination(int? nopProductId = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var key = string.Format(NexportProductDefaults.ProductMappingsAllCacheKey,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                showHidden, "", true);

            return _cacheManager.Get(key, () =>
            {
                var query = _nexportProductMappingRepository.Table;

                if (nopProductId != null)
                    query = query.Where(np => np.NopProductId == nopProductId);

                var nexportMappings = new PagedList<NexportProductMapping>(query, pageIndex, pageSize);

                return nexportMappings;
            });
        }

        public IPagedList<NexportProductMapping> GetProductMappingsPagination(Guid nexportProductId,
            NexportProductTypeEnum nexportProductType,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (nexportProductId == Guid.Empty)
            {
                return new PagedList<NexportProductMapping>(new List<NexportProductMapping>(), pageIndex, pageSize);
            }

            var key = string.Format(NexportProductDefaults.ProductMappingsAllCacheKey,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                showHidden, "", false);

            return _cacheManager.Get(key, () =>
            {
                var query = _nexportProductMappingRepository.Table
                    .Where(np => np.Type == nexportProductType);

                switch (nexportProductType)
                {
                    case NexportProductTypeEnum.Catalog:
                        query = query.Where(np => np.NexportCatalogId == nexportProductId);
                        break;

                    case NexportProductTypeEnum.Section:
                    case NexportProductTypeEnum.TrainingPlan:
                        query = query.Where(np =>
                            np.NexportCatalogSyllabusLinkId == nexportProductId);
                        break;

                    default:
                        goto case NexportProductTypeEnum.Catalog;
                }

                var nexportMappings = new PagedList<NexportProductMapping>(query, pageIndex, pageSize);
                return nexportMappings;
            });
        }

        public IList<NexportProductMapping> GetProductMappingsByStoreId(int storeId)
        {
            return storeId < 1 ?
                new List<NexportProductMapping>() :
                _nexportProductMappingRepository.Table.Where(np => np.StoreId == storeId).ToList();
        }

        public NexportProductMapping GetProductMappingByNopProductId(int nopProductId, int? storeId = null)
        {
            return nopProductId < 1 ?
                null :
                _nexportProductMappingRepository.Table.SingleOrDefault(np => np.NopProductId == nopProductId && np.StoreId == storeId);
        }

        public IList<NexportProductMapping> GetProductMappings(int? nopProductId = null, int? storeId = null)
        {
            var key = string.Format(NexportProductDefaults.ProductMappingsAllCacheKey,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                false, "", true);

            return _cacheManager.Get(key, () =>
            {
                var query = _nexportProductMappingRepository.Table;

                if (nopProductId != null)
                    query = query.Where(np => np.NopProductId == nopProductId);

                if (storeId != null)
                    query = query.Where(np => np.StoreId == storeId);

                return query.ToList();
            });
        }

        public IList<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappings(
            int nexportProductMappingId)
        {
            if (nexportProductMappingId < 1)
                return new List<NexportProductGroupMembershipMapping>();

            var key = string.Format(NexportProductDefaults.ProductGroupMembershipMappingsAllCacheKey,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                false, "", false);

            return _cacheManager.Get(key, () =>
            {
                return _nexportProductGroupMembershipMappingRepository.Table.Where(np =>
                    np.NexportProductMappingId == nexportProductMappingId).ToList();
            });
        }

        public IPagedList<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappingsPagination(
            int nexportProductMappingId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (nexportProductMappingId < 1)
                return new PagedList<NexportProductGroupMembershipMapping>(new List<NexportProductGroupMembershipMapping>(), pageIndex, pageSize);

            var key = string.Format(NexportProductDefaults.ProductGroupMembershipMappingsAllCacheKey,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                showHidden, "", false);

            return _cacheManager.Get(key, () =>
            {
                var query = _nexportProductGroupMembershipMappingRepository.Table.Where(np =>
                    np.NexportProductMappingId == nexportProductMappingId);

                var nexportGroupMembershipMappings =
                    new PagedList<NexportProductGroupMembershipMapping>(query, pageIndex, pageSize);

                return nexportGroupMembershipMappings;
            });
        }

        public IList<Guid> GetProductGroupMembershipIds(int nexportProductMappingId)
        {
            return nexportProductMappingId < 1 ? new List<Guid>() :
                _nexportProductGroupMembershipMappingRepository.Table.Where(np => np.NexportProductMappingId == nexportProductMappingId)
                    .Select(g => g.NexportGroupId).ToList();
        }

        public Dictionary<Guid, int> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList)
        {
            var result = new Dictionary<Guid, int>();
            foreach (var item in syllabusList)
            {
                var count = FindMappingCountPerSyllabi(item.SyllabusId);
                result.Add(item.SyllabusId, count);
            }

            return result;
        }

        public int FindMappingCountPerSyllabi(Guid syllabusId)
        {
            return (from np in _nexportProductMappingRepository.Table
                    where np.NexportSyllabusId == syllabusId
                    select np.Id).Count();
        }

        public bool HasDefaultMapping(int nopProductId)
        {
            return nopProductId > 0 && _nexportProductMappingRepository.Table.Any(np => np.NopProductId == nopProductId && np.StoreId == null);
        }

        public bool HasProductMappingForStore(int nopProductId, int storeId)
        {
            if (nopProductId <= 0 || storeId <= 0)
                return false;

            return _nexportProductMappingRepository.Table.Any(np =>
                np.NopProductId == nopProductId && np.StoreId == storeId);
        }

        public bool HasProductMappingForNopProduct(int nopProductId, Guid catalogId, Guid? syllabusId)
        {
            return _nexportProductMappingRepository.Table.Any(np => np.NopProductId == nopProductId &&
                np.NexportCatalogId == catalogId && np.NexportSyllabusId == syllabusId);
        }

        public NexportProductMapping GetProductMappingById(int mappingId)
        {
            return mappingId == 0 ? null : _nexportProductMappingRepository.GetById(mappingId);
        }

        public void DeleteNexportProductMapping(NexportProductMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _nexportProductMappingRepository.Delete(mapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityDeleted(mapping);
        }

        public void UpdateNexportProductMapping(NexportProductMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _nexportProductMappingRepository.Update(mapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityUpdated(mapping);
        }

        public NexportProductGroupMembershipMapping GetProductGroupMembershipMappingById(int mappingId)
        {
            return mappingId == 0 ? null : _nexportProductGroupMembershipMappingRepository.GetById(mappingId);
        }

        public void DeleteGroupMembershipMapping(NexportProductGroupMembershipMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _nexportProductGroupMembershipMappingRepository.Delete(mapping);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductGroupMembershipMappingPatternCacheKey);

            _eventPublisher.EntityDeleted(mapping);
        }

        public void InsertNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            if (_nexportOrderProcessingQueueRepository.Table.Any(q => q.OrderId == queueItem.OrderId))
                return;

            _logger.Information($"Order {queueItem.OrderId} has been added to the processing queue and awaiting to be processed.");
            _nexportOrderProcessingQueueRepository.Insert(queueItem);

            //event notification
            _eventPublisher.EntityInserted(queueItem);
        }

        public void DeleteNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportOrderProcessingQueueRepository.Delete(queueItem);

            _eventPublisher.EntityDeleted(queueItem);
        }

        public void InsertOrUpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_nexportOrderInvoiceItemRepository.Table.Any(q => q.OrderId == item.OrderId &&
                                                                  q.OrderItemId == item.OrderItemId))
            {
                try
                {
                    var currentInvoiceItem = FindNexportOrderInvoiceItem(item.OrderId, item.OrderItemId);
                    currentInvoiceItem.InvoiceItemId = item.InvoiceItemId;

                    _nexportOrderInvoiceItemRepository.Update(currentInvoiceItem);

                    _eventPublisher.EntityUpdated(currentInvoiceItem);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Cannot update new invoice item for the order item {item.OrderItemId} in order {item.OrderId}", ex);
                }
            }
            else
            {
                try
                {
                    _nexportOrderInvoiceItemRepository.Insert(item);

                    _eventPublisher.EntityInserted(item);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Cannot add new Nexport order invoice item for the order item {item.OrderItemId} in order {item.OrderId}", ex);
                }
            }
        }

        public void InsertNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            if (_nexportOrderInvoiceRedemptionQueueRepository.Table.Any(q => q.OrderInvoiceItemId == queueItem.OrderInvoiceItemId))
                return;

            _nexportOrderInvoiceRedemptionQueueRepository.Insert(queueItem);
            _logger.Information($"Invoice redemption {queueItem.OrderInvoiceItemId} for user {queueItem.RedeemingUserId} has been scheduled.");

            //event notification
            _eventPublisher.EntityInserted(queueItem);
        }

        public void DeleteNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _nexportOrderInvoiceItemRepository.Delete(item);

            _eventPublisher.EntityDeleted(item);
        }

        public void UpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _nexportOrderInvoiceItemRepository.Update(item);

            _eventPublisher.EntityUpdated(item);
        }

        public void DeleteNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportOrderInvoiceRedemptionQueueRepository.Delete(queueItem);

            _eventPublisher.EntityDeleted(queueItem);
        }

        public void UpdateNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportOrderInvoiceRedemptionQueueRepository.Update(queueItem);

            _eventPublisher.EntityUpdated(queueItem);
        }

        public NexportOrderInvoiceItem FindNexportOrderInvoiceItem(int orderId, int orderItemId)
        {
            if (orderId < 1)
                return null;

            return orderItemId < 1 ? null : _nexportOrderInvoiceItemRepository.Table.SingleOrDefault(o =>
                o.OrderId == orderId && o.OrderItemId == orderItemId);
        }

        public NexportOrderInvoiceItem FindNexportOrderInvoiceItemById(int orderInvoiceItemId)
        {
            return orderInvoiceItemId < 1 ? null : _nexportOrderInvoiceItemRepository.GetById(orderInvoiceItemId);
        }

        public void MapNexportProduct(MapProductToNexportProductModel model)
        {
            //get selected products
            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                switch (model.NexportProductType)
                {
                    case NexportProductTypeEnum.Catalog:
                        var catalogDetails = GetCatalogDetails(model.NexportProductId);
                        var catalogCreditHours = GetCatalogCreditHours(model.NexportProductId);

                        var existingProductCatalogs = GetProductCatalogsByCatalogId(model.NexportProductId, showHidden: true);
                        foreach (var product in selectedProducts)
                        {
                            //whether product catalog with such parameters already exists
                            if (FindProductCatalog(existingProductCatalogs, product.Id, model.NexportProductId) != null)
                                continue;

                            //int? accessLimit = null;
                            //if (!string.IsNullOrEmpty(catalogDetails.AccessTimeLimit))
                            //    accessLimit = int.Parse(catalogDetails.AccessTimeLimit);

                            // Insert the new product catalog mapping
                            InsertNexportProductMapping(new NexportProductMapping
                            {
                                NopProductId = product.Id,
                                DisplayName = product.Name,
                                NexportProductName = catalogDetails.Name,
                                NexportCatalogId = model.NexportProductId,
                                PricingModel = catalogDetails.PricingModel,
                                PublishingModel = catalogDetails.PublishingModel,
                                Type = model.NexportProductType,
                                CreditHours = catalogCreditHours.CreditHours
                            });
                        }

                        break;

                    case NexportProductTypeEnum.Section:
                        if (model.NexportSyllabusId == null)
                            throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                        var sectionDetails = GetSectionDetails(model.NexportSyllabusId.Value);

                        var existingProductSections = GetProductSectionsBySectionId(model.NexportSyllabusId.Value, showHidden: true);
                        foreach (var product in selectedProducts)
                        {
                            //whether product section with such parameters already exists
                            if (FindProductSection(existingProductSections, product.Id, model.NexportSyllabusId.Value) != null)
                                continue;

                            // Insert the new product section mapping
                            InsertNexportProductMapping(new NexportProductMapping
                            {
                                NopProductId = product.Id,
                                DisplayName = product.Name,
                                NexportProductName = sectionDetails.Title,
                                NexportCatalogId = model.NexportCatalogId,
                                NexportSyllabusId = model.NexportSyllabusId,
                                NexportCatalogSyllabusLinkId = model.NexportProductId,
                                UtcAvailableDate = sectionDetails.EnrollmentStart,
                                UtcEndDate = sectionDetails.EnrollmentEnd,
                                Type = model.NexportProductType,
                                CreditHours = sectionDetails.CreditHours,
                                SectionCeus = sectionDetails.SectionCeus
                            });
                        }

                        break;

                    case NexportProductTypeEnum.TrainingPlan:
                        if (model.NexportSyllabusId == null)
                            throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                        var trainingPlanDetails = GetTrainingPlanDetails(model.NexportSyllabusId.Value);

                        var existingProductTrainingPlan = GetProductTrainingPlansByTrainingPlanId(model.NexportSyllabusId.Value, showHidden: true);
                        foreach (var product in selectedProducts)
                        {
                            //whether product section with such parameters already exists
                            if (FindProductTrainingPlan(existingProductTrainingPlan, product.Id, model.NexportSyllabusId.Value) != null)
                                continue;

                            // Insert the new product section mapping
                            InsertNexportProductMapping(new NexportProductMapping
                            {
                                NopProductId = product.Id,
                                DisplayName = product.Name,
                                NexportProductName = trainingPlanDetails.Name,
                                NexportCatalogId = model.NexportCatalogId,
                                NexportSyllabusId = model.NexportSyllabusId,
                                NexportCatalogSyllabusLinkId = model.NexportProductId,
                                UtcAvailableDate = trainingPlanDetails.EnrollmentStart,
                                UtcEndDate = trainingPlanDetails.EnrollmentEnd,
                                Type = model.NexportProductType,
                                CreditHours = trainingPlanDetails.CreditHours
                            });
                        }

                        break;

                    default:
                        goto case NexportProductTypeEnum.Catalog;
                }
            }
        }

        public void MapNexportProduct(MapNexportProductModel model)
        {
            //get selected products
            var product = _productService.GetProductById(model.NopProductId);
            if (product != null)
            {
                var updateProductMapping = false;

                var productMapping = GetProductMappingByNopProductId(product.Id, model.StoreId);
                if (productMapping != null)
                {
                    productMapping.Type = model.NexportProductType;
                    updateProductMapping = true;
                }
                else
                {
                    productMapping = new NexportProductMapping()
                    {
                        NopProductId = product.Id,
                        DisplayName = product.Name,
                        Type = model.NexportProductType
                    };
                }

                switch (model.NexportProductType)
                {
                    case NexportProductTypeEnum.Catalog:
                        var catalogDetails = GetCatalogDetails(model.NexportProductId);
                        var catalogCreditHours = GetCatalogCreditHours(model.NexportProductId);

                        productMapping.NexportProductName = catalogDetails.Name;
                        productMapping.NexportCatalogId = model.NexportCatalogId;
                        productMapping.PricingModel = catalogDetails.PricingModel;
                        productMapping.PublishingModel = catalogDetails.PublishingModel;
                        productMapping.CreditHours = catalogCreditHours.CreditHours;

                        break;

                    case NexportProductTypeEnum.Section:
                        if (model.NexportSyllabusId == null)
                            throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                        var sectionDetails = GetSectionDetails(model.NexportSyllabusId.Value);

                        productMapping.NexportProductName = sectionDetails.Title;
                        productMapping.NexportCatalogId = model.NexportCatalogId;
                        productMapping.NexportSyllabusId = model.NexportSyllabusId;
                        productMapping.NexportCatalogSyllabusLinkId = model.NexportProductId;
                        productMapping.UtcAvailableDate = sectionDetails.EnrollmentStart;
                        productMapping.UtcEndDate = sectionDetails.EnrollmentEnd;
                        productMapping.UtcLastModifiedDate = sectionDetails.UtcDateLastModified;
                        productMapping.CreditHours = sectionDetails.CreditHours;
                        productMapping.SectionCeus = sectionDetails.SectionCeus;

                        break;

                    case NexportProductTypeEnum.TrainingPlan:
                        if (model.NexportSyllabusId == null)
                            throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                        var trainingPlanDetails = GetTrainingPlanDetails(model.NexportSyllabusId.Value);

                        productMapping.NexportProductName = trainingPlanDetails.Name;
                        productMapping.NexportCatalogId = model.NexportCatalogId;
                        productMapping.NexportSyllabusId = model.NexportSyllabusId;
                        productMapping.NexportCatalogSyllabusLinkId = model.NexportProductId;
                        productMapping.UtcAvailableDate = trainingPlanDetails.EnrollmentStart;
                        productMapping.UtcEndDate = trainingPlanDetails.EnrollmentEnd;
                        productMapping.UtcLastModifiedDate = trainingPlanDetails.UtcDateLastModified;
                        productMapping.CreditHours = trainingPlanDetails.CreditHours;

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
                    UpdateNexportProductMapping(productMapping);
                }
                else
                {
                    InsertNexportProductMapping(productMapping);
                }
            }
        }

        public void InsertUserMapping(NexportUserMapping nexportUserMapping)
        {
            if (nexportUserMapping == null)
                throw new ArgumentNullException(nameof(nexportUserMapping));

            _nexportUserMappingRepository.Insert(nexportUserMapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.UserMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityInserted(nexportUserMapping);
        }

        public void DeleteUserMapping(NexportUserMapping nexportUserMapping)
        {
            if (nexportUserMapping == null)
                throw new ArgumentNullException(nameof(nexportUserMapping));

            _nexportUserMappingRepository.Delete(nexportUserMapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.UserMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityDeleted(nexportUserMapping);
        }

        public void UpdateUserMapping(NexportUserMapping nexportUserMapping)
        {
            if (nexportUserMapping == null)
                throw new ArgumentNullException(nameof(nexportUserMapping));

            _nexportUserMappingRepository.Update(nexportUserMapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.UserMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityUpdated(nexportUserMapping);
        }

        public NexportUserMapping FindUserMappingByCustomerId(int nopCustomerId)
        {
            return nopCustomerId < 1 ? null : _nexportUserMappingRepository.Table.SingleOrDefault(np => np.NopUserId == nopCustomerId);
        }

        public NexportUserMapping FindUserMappingByNexportUserId(Guid userId)
        {
            return userId == Guid.Empty
                ? null
                : _nexportUserMappingRepository.Table.SingleOrDefault(np => np.NexportUserId == userId);
        }

        public Guid? FindExistingInvoiceForOrder(int orderId)
        {
            return orderId < 1
                ? null
                : _nexportOrderInvoiceItemRepository.Table
                    .FirstOrDefault(i => i.OrderId == orderId)?.InvoiceId;
        }

        public Guid? FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId)
        {
            if (orderId < 1)
                return null;

            return orderItemId <= 0
                ? null
                : _nexportOrderInvoiceItemRepository.Table.FirstOrDefault(i =>
                    i.OrderId == orderId && i.OrderItemId == orderItemId)?.InvoiceItemId;

        }

        public bool HasNexportOrderProcessingQueueItem(int orderId)
        {
            return orderId > 0 && _nexportOrderProcessingQueueRepository.Table.Any(q => q.OrderId == orderId);
        }

        public bool HasNexportProductMapping(Order order)
        {
            if (order == null)
                return false;

            var items = order.OrderItems.ToArray();

            return items.Where((t, i) => _nexportProductMappingRepository.Table.Any(p => p.NopProductId == items[i].ProductId)).Any();
        }

        public bool HasNexportProductMapping(int productId)
        {
            return productId > 0 && _nexportProductMappingRepository.Table.Any(p => p.NopProductId == productId);
        }

        public IList<int?> GetStoreIdsPerProductMapping(int productId)
        {
            return productId < 1
                ? new List<int?>()
                : _nexportProductMappingRepository.Table.Where(np => np.NopProductId == productId)
                    .Select(np => np.StoreId).ToList();
        }

        public void CopyProductMappings(Product originalProduct, Product copyingProduct)
        {
            if (originalProduct == null)
                throw new ArgumentNullException(nameof(originalProduct));

            if (copyingProduct == null)
                throw new ArgumentNullException(nameof(originalProduct));

            var mappings = GetProductMappings(originalProduct.Id);
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

                InsertNexportProductMapping(newMapping);

                var groupMembershipMappings = GetProductGroupMembershipMappings(mapping.Id);
                foreach (var groupMembershipMapping in groupMembershipMappings)
                {
                    var newGroupMembershipMapping = new NexportProductGroupMembershipMapping
                    {
                        NexportGroupId = groupMembershipMapping.NexportGroupId,
                        NexportGroupName = groupMembershipMapping.NexportGroupName,
                        NexportGroupShortName = groupMembershipMapping.NexportGroupShortName,
                        NexportProductMappingId = newMapping.Id
                    };

                    InsertNexportProductGroupMembershipMapping(newGroupMembershipMapping);
                }
            }
        }

        public IPagedList<NexportSupplementalInfoQuestion> GetAllNexportSupplementalInfoQuestionsPagination(
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var key = string.Format(NexportProductDefaults.SupplementalInfoQuestionAllCacheKey, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from question in _nexportSupplementalInfoQuestionRepository.Table
                            select question;

                var questions = new PagedList<NexportSupplementalInfoQuestion>(query, pageIndex, pageSize);
                return questions;
            });
        }

        public IList<NexportSupplementalInfoQuestion> GetAllNexportSupplementalInfoQuestions()
        {
            return GetAllNexportSupplementalInfoQuestionsPagination().ToList();
        }

        public NexportSupplementalInfoQuestion GetNexportSupplementalInfoQuestionById(int questionId)
        {
            return questionId > 0 ? _nexportSupplementalInfoQuestionRepository.GetById(questionId) : null;
        }

        public IList<NexportSupplementalInfoQuestion> GetNexportSupplementalInfoQuestionsByIds(int[] questionIds)
        {
            if (questionIds == null || questionIds.Length == 0)
                return new List<NexportSupplementalInfoQuestion>();

            var questions = (
                from question in _nexportSupplementalInfoQuestionRepository.Table
                where questionIds.Contains(question.Id)
                select question).ToList();

            return questionIds
                .Select(id => questions.Find(x => x.Id == id))
                .Where(question => question != null).ToList();
        }

        public void InsertNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            _nexportSupplementalInfoQuestionRepository.Insert(question);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoQuestionPatternCacheKey);

            _eventPublisher.EntityInserted(question);
        }

        public void DeleteNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            _nexportSupplementalInfoQuestionRepository.Delete(question);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoQuestionPatternCacheKey);

            _eventPublisher.EntityDeleted(question);
        }

        public void DeleteNexportSupplementalInfoQuestions(IList<NexportSupplementalInfoQuestion> questions)
        {
            if (questions == null)
                throw new ArgumentNullException(nameof(questions));

            foreach (var question in questions)
            {
                DeleteNexportSupplementalInfoQuestion(question);
            }
        }

        public void UpdateNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            question.UtcDateModified = DateTime.UtcNow;

            _nexportSupplementalInfoQuestionRepository.Update(question);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoQuestionPatternCacheKey);

            _eventPublisher.EntityUpdated(question);
        }

        public NexportSupplementalInfoOption GetNexportSupplementalInfoOptionById(int optionId)
        {
            return optionId > 0 ? _nexportSupplementalInfoOptionRepository.GetById(optionId) : null;
        }

        public IList<NexportSupplementalInfoOption> GetNexportSupplementalInfoOptionsByQuestionId(int questionId,
            bool showHidden = false)
        {
            if (questionId <= 0)
                return new List<NexportSupplementalInfoOption>();

            var query = _nexportSupplementalInfoOptionRepository.Table
                .Where(opt => opt.QuestionId == questionId);

            return showHidden ? query.ToList() : query.Where(opt => !opt.Deleted).ToList();
        }

        public void InsertNexportSupplementalInfoOption(NexportSupplementalInfoOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            _nexportSupplementalInfoOptionRepository.Insert(option);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoOptionPatternCacheKey);

            _eventPublisher.EntityInserted(option);
        }

        public void DeleteNexportSupplementalInfoOption(NexportSupplementalInfoOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            option.Deleted = true;

            UpdateNexportSupplementalInfoOption(option);

            _eventPublisher.EntityDeleted(option);
        }

        public void UpdateNexportSupplementalInfoOption(NexportSupplementalInfoOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            option.UtcDateModified = DateTime.UtcNow;

            _nexportSupplementalInfoOptionRepository.Update(option);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoOptionPatternCacheKey);

            _eventPublisher.EntityUpdated(option);
        }

        public NexportSupplementalInfoQuestionMapping GetNexportSupplementalInfoQuestionMappingById(int questionMappingId)
        {
            return questionMappingId > 0
                ? _nexportSupplementalInfoQuestionMappingRepository.GetById(questionMappingId)
                : null;
        }

        public IList<NexportSupplementalInfoQuestionMapping>
            GetNexportSupplementalInfoQuestionMappingsByProductMappingId(int nexportProductMappingId)
        {
            return nexportProductMappingId > 0
                ? _nexportSupplementalInfoQuestionMappingRepository.Table.Where(qm =>
                    qm.ProductMappingId == nexportProductMappingId).ToList()
                : new List<NexportSupplementalInfoQuestionMapping>();
        }

        public NexportSupplementalInfoQuestionMapping GetNexportSupplementalInfoQuestionMapping(int nexportProductMappingId,
            int questionId)
        {
            if (nexportProductMappingId < 1 || questionId < 1)
                return null;

            return _nexportSupplementalInfoQuestionMappingRepository.Table.FirstOrDefault(qm =>
                    qm.ProductMappingId == nexportProductMappingId && qm.QuestionId == questionId);
        }

        public void InsertNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping)
        {
            if (questionMapping == null)
                throw new ArgumentNullException(nameof(questionMapping));

            if (_nexportSupplementalInfoQuestionMappingRepository.Table.Any(qm =>
                qm.QuestionId == questionMapping.QuestionId &&
                qm.ProductMappingId == questionMapping.ProductMappingId))
                return;

            _nexportSupplementalInfoQuestionMappingRepository.Insert(questionMapping);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoQuestionMappingPatternCacheKey);

            _eventPublisher.EntityInserted(questionMapping);
        }

        public void DeleteNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping)
        {
            if (questionMapping == null)
                throw new ArgumentNullException(nameof(questionMapping));

            _nexportSupplementalInfoQuestionMappingRepository.Delete(questionMapping);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoQuestionMappingPatternCacheKey);

            _eventPublisher.EntityDeleted(questionMapping);
        }

        public void UpdateNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping)
        {
            if (questionMapping == null)
                throw new ArgumentNullException(nameof(questionMapping));

            _nexportSupplementalInfoQuestionMappingRepository.Update(questionMapping);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoQuestionMappingPatternCacheKey);

            _eventPublisher.EntityUpdated(questionMapping);
        }

        public IPagedList<NexportSupplementalInfoOptionGroupAssociation> GetNexportSupplementalInfoOptionGroupAssociationsPagination(
            int optionId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (optionId < 1)
                return new PagedList<NexportSupplementalInfoOptionGroupAssociation>(
                    new List<NexportSupplementalInfoOptionGroupAssociation>(), pageIndex, pageSize);

            var key = string.Format(NexportProductDefaults.SupplementalInfoOptionGroupAssociationsAllCacheKey,
                _storeContext.CurrentStore.Id);

            return _cacheManager.Get(key, () =>
            {
                var query =
                    _nexportSupplementalInfoOptionGroupAssociationRepository
                        .Table.Where(ga => ga.OptionId == optionId);

                var groupAssociation =
                    new PagedList<NexportSupplementalInfoOptionGroupAssociation>(query, pageIndex, pageSize);

                return groupAssociation;
            });
        }

        public IList<NexportSupplementalInfoOptionGroupAssociation> GetNexportSupplementalInfoOptionGroupAssociations(
            int optionId, bool excludeInactive = false)
        {
            if (optionId < 1)
                return new List<NexportSupplementalInfoOptionGroupAssociation>();

            var query = _nexportSupplementalInfoOptionGroupAssociationRepository.Table
                .Where(a => a.OptionId == optionId);

            if (excludeInactive)
                query = query.Where(a => a.IsActive);

            return query.ToList();
        }

        public NexportSupplementalInfoOptionGroupAssociation GetNexportSupplementalInfoOptionGroupAssociationById(
            int groupAssociationId)
        {
            return groupAssociationId < 1
                ? null
                : _nexportSupplementalInfoOptionGroupAssociationRepository.GetById(groupAssociationId);
        }

        public void InsertNexportSupplementalInfoOptionGroupAssociation(NexportSupplementalInfoOptionGroupAssociation groupAssociation)
        {
            if (groupAssociation == null)
                throw new ArgumentNullException(nameof(groupAssociation));

            if (_nexportSupplementalInfoOptionGroupAssociationRepository.Table.Any(ga =>
                ga.OptionId == groupAssociation.OptionId &&
                ga.NexportGroupId == groupAssociation.NexportGroupId))
                return;

            _nexportSupplementalInfoOptionGroupAssociationRepository.Insert(groupAssociation);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoOptionGroupAssociationPatternCacheKey);

            _eventPublisher.EntityInserted(groupAssociation);
        }

        public void DeleteNexportSupplementalInfoOptionGroupAssociation(NexportSupplementalInfoOptionGroupAssociation groupAssociation)
        {
            if (groupAssociation == null)
                throw new ArgumentNullException(nameof(groupAssociation));

            _nexportSupplementalInfoOptionGroupAssociationRepository.Delete(groupAssociation);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoOptionGroupAssociationPatternCacheKey);

            _eventPublisher.EntityDeleted(groupAssociation);
        }

        public void UpdateNexportSupplementalInfoOptionGroupAssociation(NexportSupplementalInfoOptionGroupAssociation groupAssociation)
        {
            if (groupAssociation == null)
                throw new ArgumentNullException(nameof(groupAssociation));

            _nexportSupplementalInfoOptionGroupAssociationRepository.Update(groupAssociation);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoOptionGroupAssociationPatternCacheKey);

            _eventPublisher.EntityUpdated(groupAssociation);
        }

        public void InsertNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            if (_nexportSupplementalInfoAnswerRepository.Table.Any(a =>
                a.OptionId == answer.OptionId &&
                a.QuestionId == answer.QuestionId &&
                a.StoreId == answer.StoreId))
                return;

            _nexportSupplementalInfoAnswerRepository.Insert(answer);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoAnswerPatternCacheKey);

            _eventPublisher.EntityInserted(answer);
        }

        public void DeleteNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            _nexportSupplementalInfoAnswerRepository.Delete(answer);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoAnswerPatternCacheKey);

            _eventPublisher.EntityDeleted(answer);
        }

        public void UpdateNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            _nexportSupplementalInfoAnswerRepository.Update(answer);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoAnswerPatternCacheKey);

            _eventPublisher.EntityUpdated(answer);
        }

        public IList<NexportSupplementalInfoAnswer> GetNexportSupplementalInfoAnswers(int customerId, int storeId,
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

            return query.ToList();
        }

        public IPagedList<NexportSupplementalInfoAnswer> GetNexportSupplementalInfoAnswersPagination(int customerId, int? questionId = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (customerId < 1)
                return new PagedList<NexportSupplementalInfoAnswer>(
                    new List<NexportSupplementalInfoAnswer>(), pageIndex, pageSize);

            return _cacheManager.Get(NexportProductDefaults.SupplementalInfoAnswerAllCacheKey, () =>
            {
                var query =
                    _nexportSupplementalInfoAnswerRepository
                        .Table.Where(a => a.CustomerId == customerId);

                if (questionId != null)
                    query = query.Where(a => a.QuestionId == questionId);

                var answers =
                    new PagedList<NexportSupplementalInfoAnswer>(query, pageIndex, pageSize);

                return answers;
            });
        }

        public NexportSupplementalInfoAnswer GetNexportSupplementalInfoAnswerById(int answerId)
        {
            return answerId < 1
                ? null
                : _nexportSupplementalInfoAnswerRepository.GetById(answerId);
        }

        public IPagedList<NexportSupplementalInfoQuestion> GetNexportSupplementalInfoAnsweredQuestionsPagination(int customerId, int pageIndex = 0,
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

            var questions =
                new PagedList<NexportSupplementalInfoQuestion>(query, pageIndex, pageSize);

            return questions;
        }

        public void InsertNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership)
        {
            if (answerMembership == null)
                throw new ArgumentNullException(nameof(answerMembership));

            if (_nexportSupplementalInfoAnswerMembershipRepository.Table.Any(am =>
                am.AnswerId == answerMembership.AnswerId &&
                am.NexportMembershipId == answerMembership.NexportMembershipId))
                return;

            _nexportSupplementalInfoAnswerMembershipRepository.Insert(answerMembership);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoAnswerMembershipPatternCacheKey);

            _eventPublisher.EntityInserted(answerMembership);
        }

        public void DeleteNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership)
        {
            if (answerMembership == null)
                throw new ArgumentNullException(nameof(answerMembership));

            _nexportSupplementalInfoAnswerMembershipRepository.Delete(answerMembership);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoAnswerMembershipPatternCacheKey);

            _eventPublisher.EntityDeleted(answerMembership);
        }

        public IList<NexportSupplementalInfoAnswerMembership> GetNexportSupplementalInfoAnswerMembershipsByAnswerId(int answerId)
        {
            return answerId < 1
                ? new List<NexportSupplementalInfoAnswerMembership>()
                : _nexportSupplementalInfoAnswerMembershipRepository.Table
                    .Where(am => am.AnswerId == answerId).ToList();
        }

        public NexportSupplementalInfoAnswerMembership GetNexportSupplementalInfoAnswerMembership(
            Guid nexportMembershipId)
        {
            return _nexportSupplementalInfoAnswerMembershipRepository.Table
                    .FirstOrDefault(am => am.NexportMembershipId == nexportMembershipId);
        }

        public void InsertNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement)
        {
            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));

            if (_nexportRequiredSupplementalInfoRepository.Table.Any(r =>
                r.CustomerId == requirement.CustomerId &&
                r.StoreId == requirement.StoreId &&
                r.QuestionId == requirement.QuestionId))
                return;

            _nexportRequiredSupplementalInfoRepository.Insert(requirement);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoRequiredPatternCacheKey);

            _eventPublisher.EntityInserted(requirement);
        }

        public void DeleteNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement)
        {
            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));

            _nexportRequiredSupplementalInfoRepository.Delete(requirement);

            _cacheManager.RemoveByPrefix(NexportProductDefaults.SupplementalInfoRequiredPatternCacheKey);

            _eventPublisher.EntityDeleted(requirement);
        }

        public IList<NexportRequiredSupplementalInfo> GetNexportRequiredSupplementalInfos(int customerId, int storeId, int? questionId = null)
        {
            if (customerId < 1 || storeId < 1)
                return new List<NexportRequiredSupplementalInfo>();

            var query = _nexportRequiredSupplementalInfoRepository.Table
                .Where(r => r.CustomerId == customerId && r.StoreId == storeId);

            if (questionId != null)
                query = query.Where(r => r.QuestionId == questionId);

            return query.ToList();
        }

        public NexportRequiredSupplementalInfo GetNexportRequiredSupplementalInfoByNopProductId(int customerId, int storeId, int questionId)
        {
            if (customerId < 1 || storeId < 1 || questionId < 1)
                return null;

            return _nexportRequiredSupplementalInfoRepository.Table
                .FirstOrDefault(r => r.CustomerId == customerId &&
                                     r.StoreId == storeId && r.QuestionId == questionId);
        }

        public bool HasRequiredSupplementalInfo(int customerId, int storeId)
        {
            if (customerId < 1 || storeId < 1)
                return false;

            return _nexportRequiredSupplementalInfoRepository.TableNoTracking.Any(r =>
                r.CustomerId == customerId && r.StoreId == storeId);
        }

        public bool HasUnprocessedAnswer(int orderId)
        {
            if (orderId < 1)
                return false;

            var order = _orderService.GetOrderById(orderId);

            if (order == null || order.Deleted || order.OrderStatus != OrderStatus.Complete)
                return false;

            var nexportProductMappings = order.OrderItems
                .Select(item =>
                    GetProductMappingByNopProductId(item.ProductId, _storeContext.CurrentStore.Id))
                .Where(mapping => mapping != null).ToList();

            return nexportProductMappings.Select(t =>
                    GetNexportSupplementalInfoQuestionMappingsByProductMappingId(t.Id)
                        .Select(x => x.QuestionId)
                        .ToList())
                        .Select(questions =>
                            GetNexportSupplementalInfoAnswers(order.CustomerId, order.StoreId)
                                .Where(x =>
                                    questions.Contains(x.QuestionId))
                                .Any(x => x.Status == NexportSupplementalInfoAnswerStatus.NotProcessed))
                                    .FirstOrDefault();

        }

        public void InsertNexportSupplementalInfoAnswerProcessingQueueItem(NexportSupplementalInfoAnswerProcessingQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportSupplementalInfoAnswerProcessingQueueRepository.Insert(queueItem);

            _eventPublisher.EntityInserted(queueItem);
        }

        public void DeleteNexportSupplementalInfoAnswerProcessingQueueItem(NexportSupplementalInfoAnswerProcessingQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportSupplementalInfoAnswerProcessingQueueRepository.Delete(queueItem);

            _eventPublisher.EntityDeleted(queueItem);
        }

        public void InsertNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportGroupMembershipRemovalQueueRepository.Insert(queueItem);

            _eventPublisher.EntityInserted(queueItem);
        }

        public void DeleteNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportGroupMembershipRemovalQueueRepository.Delete(queueItem);

            _eventPublisher.EntityDeleted(queueItem);
        }
    }
}
