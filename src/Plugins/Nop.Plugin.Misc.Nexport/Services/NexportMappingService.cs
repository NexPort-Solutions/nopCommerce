using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Customers;
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

            _nexportProductRepository.Insert(nexportProductMapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityInserted(nexportProductMapping);
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
                //var query = from np in _nexportProductRepository.Table
                //    join p in _productRepository.Table on np.NopProductId equals p.Id
                //    where np.NexportCatalogId == catalogId && !p.Deleted && (showHidden || p.Published)
                //    select np;

                var productQuery = (from p in _productRepository.Table
                    where !p.Deleted && (showHidden || p.Published)
                    select p.Id).ToList();

                var query = from np in _nexportProductRepository.Table
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

                var query = from np in _nexportProductRepository.Table
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

                var query = from np in _nexportProductRepository.Table
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

        public NexportProductMapping FindProductCatalog(IList<NexportProductMapping> source, int productId, Guid catalogId)
        {
            return source.FirstOrDefault(productCatalog => productCatalog.NopProductId == productId &&
                                                           productCatalog.NexportCatalogId == catalogId);
        }

        public NexportProductMapping FindProductSection(IList<NexportProductMapping> source, int productId, Guid sectionId)
        {
            return source.FirstOrDefault(productCatalog => productCatalog.NopProductId == productId &&
                                                           productCatalog.NexportSyllabusId == sectionId &&
                                                           productCatalog.Type==NexportProductTypeEnum.Section);
        }

        public NexportProductMapping FindProductTrainingPlan(IList<NexportProductMapping> source, int productId,
            Guid trainingPlanId)
        {
            return source.FirstOrDefault(productCatalog => productCatalog.NopProductId == productId &&
                                                           productCatalog.NexportSyllabusId == trainingPlanId &&
                                                           productCatalog.Type == NexportProductTypeEnum.TrainingPlan);
        }

        public IPagedList<NexportProductMapping> GetProductMappings(int pageIndex = 0, int pageSize = Int32.MaxValue, bool showHidden = false)
        {
            var key = string.Format(NexportProductDefaults.ProductMappingsAllCacheKey,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                showHidden, "", true);

            return _cacheManager.Get(key, () =>
            {
                var query = _nexportProductRepository.Table;

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

            return _cacheManager.Get(key, () => {
                var query = _nexportProductRepository.Table
                    .Where(np => np.Type == nexportProductType);

                switch (nexportProductType)
                {
                    case NexportProductTypeEnum.Catalog:
                        query = query.Where(np => np.NexportCatalogId == nexportProductId);
                        break;

                    case NexportProductTypeEnum.Section:
                        break;

                    case NexportProductTypeEnum.TrainingPlan:
                        break;

                    default:
                        goto case NexportProductTypeEnum.Catalog;
                }

                var nexportMappings = new PagedList<NexportProductMapping>(query, pageIndex, pageSize);
                return nexportMappings;
            });
        }

        public NexportProductMapping GetProductMappingByNopProductId(int nopProductId)
        {
            return nopProductId < 1 ? null : _nexportProductRepository.Table.SingleOrDefault(np => np.NopProductId == nopProductId);
        }

        public Dictionary<Guid, int> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList)
        {
            var result = new Dictionary<Guid, int>();
            foreach (var item in syllabusList)
            {
                var id = Guid.Parse(item.SyllabusId);
                var count = FindMappingCountPerSyllabi(id);
                result.Add(id, count);
            }

            return result;
        }

        public int FindMappingCountPerSyllabi(Guid syllabusId)
        {
            return (from np in _nexportProductRepository.Table
                where np.NexportSyllabusId == syllabusId
                select np.Id).Count();
        }

        public NexportProductMapping FindProductMappingById(int mappingId)
        {
            return mappingId == 0 ? null : _nexportProductRepository.GetById(mappingId);
        }

        public void DeleteMapping(NexportProductMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _nexportProductRepository.Delete(mapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityDeleted(mapping);
        }

        public void UpdateMapping(NexportProductMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _nexportProductRepository.Update(mapping);

            //cache
            _cacheManager.RemoveByPrefix(NexportProductDefaults.ProductMappingPatternCacheKey);

            //event notification
            _eventPublisher.EntityUpdated(mapping);
        }

        public void InsertNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            if (_nexportOrderProcessingQueueRepository.Table.Any(q => q.OrderId == queueItem.OrderId))
                return;

            _nexportOrderProcessingQueueRepository.Insert(queueItem);

            //event notification
            _eventPublisher.EntityInserted(queueItem);
        }

        public void DeleteNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem)
        {
            if(queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            _nexportOrderProcessingQueueRepository.Delete(queueItem);

            _eventPublisher.EntityDeleted(queueItem);
        }

        public void InsertNexportOrderInvoiceItem(NexportOrderInvoiceItem item)
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
                        var catalogDetails = FindCatalogDetails(model.NexportProductId);
                        var catalogCreditHours = FindCatalogCreditHours(model.NexportProductId);

                        var existingProductCatalogs = GetProductCatalogsByCatalogId(model.NexportProductId, showHidden: true);
                        foreach (var product in selectedProducts)
                        {
                            //whether product catalog with such parameters already exists
                            if (FindProductCatalog(existingProductCatalogs, product.Id, model.NexportProductId) != null)
                                continue;

                            int? accessLimit = null;
                            if (!string.IsNullOrEmpty(catalogDetails.AccessTimeLimit))
                                accessLimit = int.Parse(catalogDetails.AccessTimeLimit);

                            // Insert the new product catalog mapping
                            InsertNexportProductMapping(new NexportProductMapping
                            {
                                NopProductId = product.Id,
                                DisplayName = product.Name,
                                NexportProductName = catalogDetails.Name,
                                NexportCatalogId = model.NexportProductId,
                                PricingModel = (int?)catalogDetails.PricingModel.GetValueOrDefault(),
                                PublishingModel = (int?)catalogDetails.PublishingModel.GetValueOrDefault(),
                                Type = model.NexportProductType,
                                AccessTimeLimit = accessLimit,
                                CreditHours = catalogCreditHours.CreditHours
                            });
                        }

                        break;

                    case NexportProductTypeEnum.Section:
                        if (model.NexportSyllabusId == null)
                            throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                        var sectionDetails = FindSectionDetails(model.NexportSyllabusId.Value);

                        var existingProductSections = GetProductSectionsBySectionId(model.NexportSyllabusId.Value, showHidden: true);
                        foreach (var product in selectedProducts)
                        {
                            //whether product section with such parameters already exists
                            if (FindProductSection(existingProductSections, product.Id, model.NexportProductId) != null)
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
                                CreditHours = sectionDetails.CreditHours
                            });
                        }

                        break;

                    case NexportProductTypeEnum.TrainingPlan:
                        if (model.NexportSyllabusId == null)
                            throw new ArgumentNullException(nameof(model.NexportSyllabusId), "Syllabus Id cannot be null");

                        var trainingPlanDetails = FindTrainingPlanDetails(model.NexportSyllabusId.Value);

                        var existingProductTrainingPlan = GetProductTrainingPlansByTrainingPlanId(model.NexportSyllabusId.Value, showHidden: true);
                        foreach (var product in selectedProducts)
                        {
                            //whether product section with such parameters already exists
                            if (FindProductTrainingPlan(existingProductTrainingPlan, product.Id, model.NexportProductId) != null)
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
    }
}
