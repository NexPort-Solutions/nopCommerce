using System;
using System.Collections.Generic;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public interface INexportService
    {
        void InsertNexportProductMapping(NexportProductMapping nexportProductMapping);

        void InsertNexportProductGroupMembershipMapping(NexportProductGroupMembershipMapping nexportProductGroupMembershipMapping);

        IPagedList<NexportProductMapping> GetProductCatalogsByCatalogId(Guid catalogId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductSectionsBySectionId(Guid sectionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductTrainingPlansByTrainingPlanId(Guid trainingPlanId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        NexportProductMapping FindProductCatalog(IList<NexportProductMapping> source, int productId, Guid catalogId, int? storeId = null);

        NexportProductMapping FindProductSection(IList<NexportProductMapping> source, int productId, Guid sectionId, int? storeId = null);

        NexportProductMapping FindProductTrainingPlan(IList<NexportProductMapping> source, int productId, Guid trainingPlan, int? storeId = null);

        IPagedList<NexportProductMapping> GetProductMappings(int? nopProductId = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductMappings(Guid nexportProductId, NexportProductTypeEnum nexportProductType,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IList<NexportProductMapping> GetProductMappingsByStoreId(int storeId);

        NexportProductMapping GetProductMappingByNopProductId(int nopProductId, int? storeId = null);


        IPagedList<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappings(int nexportProductMappingId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IList<Guid> GetProductGroupMembershipIds(int nexportProductMappingId);

        Dictionary<Guid, int> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList);

        NexportProductMapping GetProductMappingById(int mappingId);

        void DeleteNexportProductMapping(NexportProductMapping mapping);

        void UpdateNexportProductMapping(NexportProductMapping mapping);

        NexportProductGroupMembershipMapping GetProductGroupMembershipMappingById(int mappingId);

        void DeleteGroupMembershipMapping(NexportProductGroupMembershipMapping mapping);

        void InsertNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem);

        void DeleteNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem);

        void InsertOrUpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        void DeleteNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        void UpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        void InsertNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem);

        void DeleteNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem);

        void UpdateNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem);

        NexportOrderInvoiceItem FindNexportOrderInvoiceItem(int orderId, int orderItemId);

        NexportOrderInvoiceItem FindNexportOrderInvoiceItemById(int orderInvoiceItemId);

        void MapNexportProduct(MapProductToNexportProductModel model);

        void MapNexportProduct(MapNexportProductModel model);

        void InsertUserMapping(NexportUserMapping nexportUserMapping);

        void DeleteUserMapping(NexportUserMapping nexportUserMapping);

        void UpdateUserMapping(NexportUserMapping nexportUserMapping);

        NexportUserMapping FindUserMappingByCustomerId(int nopCustomerId);

        NexportUserMapping FindUserMappingByNexportUserId(Guid userId);

        Guid? FindExistingInvoiceForOrder(int orderId);

        Guid? FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId);

        bool HasNexportOrderProcessingQueueItem(int orderId);

        bool HasNexportProductMapping(Order order);

        bool HasNexportProductMapping(int productId);

        IList<int?> GetStoreIdsPerProductMapping(int productId);
    }
}
