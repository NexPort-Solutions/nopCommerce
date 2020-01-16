using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models;

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

        public IPagedList<NexportProductMapping> GetProductMappings(int? nopProductId = null, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
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

        public IPagedList<NexportProductMapping> GetProductMappings(Guid nexportProductId, NexportProductTypeEnum nexportProductType,
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

        public IPagedList<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappings(int nexportProductMappingId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (nexportProductMappingId == 0)
            {
                return new PagedList<NexportProductGroupMembershipMapping>(new List<NexportProductGroupMembershipMapping>(), pageIndex, pageSize);
            }

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

            _logger.Information($"Invoice redemption {queueItem.OrderInvoiceItemId} for user {queueItem.RedeemingUserId} has been scheduled.");
            _nexportOrderInvoiceRedemptionQueueRepository.Insert(queueItem);

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
                                UtcLastModifiedDate = sectionDetails.UtcDateLastModified,
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
                                UtcLastModifiedDate = trainingPlanDetails.UtcDateLastModified,
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
            return nopCustomerId <= 0 ? null : _nexportUserMappingRepository.Table.SingleOrDefault(np => np.NopUserId == nopCustomerId);
        }

        public NexportUserMapping FindUserMappingByNexportUserId(Guid userId)
        {
            return userId == Guid.Empty
                ? null
                : _nexportUserMappingRepository.Table.SingleOrDefault(np => np.NexportUserId == userId);
        }

        public Guid? FindExistingInvoiceForOrder(int orderId)
        {
            return orderId <= 0
                ? null
                : _nexportOrderInvoiceItemRepository.Table
                    .FirstOrDefault(i => i.OrderId == orderId)?.InvoiceId;
        }

        public Guid? FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId)
        {
            if (orderId <= 0)
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
            return productId <= 0
                ? new List<int?>()
                : _nexportProductMappingRepository.Table.Where(np => np.NopProductId == productId)
                    .Select(np => np.StoreId).ToList();
        }
    }
}
