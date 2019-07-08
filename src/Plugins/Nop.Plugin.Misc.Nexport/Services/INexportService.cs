using System;
using System.Collections.Generic;
using NexportApi.Model;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public interface INexportService
    {
        void InsertNexportProductMapping(NexportProductMapping nexportProductMapping);

        IPagedList<NexportProductMapping> GetProductCatalogsByCatalogId(Guid catalogId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductSectionsBySectionId(Guid sectionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductTrainingPlansByTrainingPlanId(Guid trainingPlanId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        NexportProductMapping FindProductCatalog(IList<NexportProductMapping> source, int productId, Guid catalogId);

        NexportProductMapping FindProductSection(IList<NexportProductMapping> source, int productId, Guid sectionId);

        NexportProductMapping FindProductTrainingPlan(IList<NexportProductMapping> source, int productId, Guid trainingPlan);

        IPagedList<NexportProductMapping> GetProductMappings(
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductMappings(Guid nexportProductId, NexportProductTypeEnum nexportProductType,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        NexportProductMapping GetProductMappingByNopProductId(int nopProductId);

        Dictionary<Guid, int> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList);

        NexportProductMapping FindProductMappingById(int mappingId);

        void DeleteMapping(NexportProductMapping mapping);

        void UpdateMapping(NexportProductMapping mapping);

        void InsertNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem);

        void DeleteNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem);

        void InsertNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        void DeleteNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        void UpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        NexportOrderInvoiceItem FindNexportOrderInvoiceItem(int orderId, int orderItemId);

        NexportOrderInvoiceItem FindNexportOrderInvoiceItemById(int orderInvoiceItemId);

        void MapNexportProduct(MapProductToNexportProductModel model);

        void InsertUserMapping(NexportUserMapping nexportUserMapping);

        void DeleteUserMapping(NexportUserMapping nexportUserMapping);

        void UpdateUserMapping(NexportUserMapping nexportUserMapping);

        NexportUserMapping FindUserMappingByCustomerId(int nopCustomerId);
    }
}
