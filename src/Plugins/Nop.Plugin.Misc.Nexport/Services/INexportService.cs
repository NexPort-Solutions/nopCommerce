using System;
using System.Collections.Generic;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;

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

        IPagedList<NexportProductMapping> GetProductMappingsPagination(int? nopProductId = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IPagedList<NexportProductMapping> GetProductMappingsPagination(Guid nexportProductId,
            NexportProductTypeEnum nexportProductType,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IList<NexportProductMapping> GetProductMappingsByStoreId(int storeId);

        NexportProductMapping GetProductMappingByNopProductId(int nopProductId, int? storeId = null);

        IList<NexportProductMapping> GetProductMappings(int? nopProductId = null, int? storeId = null);

        IList<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappings(int nexportProductMappingId);

        IPagedList<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappingsPagination(
            int nexportProductMappingId,
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

        void CopyProductMappings(Product originalProduct, Product copyingProduct);

        IPagedList<NexportSupplementalInfoQuestion> GetAllNexportSupplementalInfoQuestionsPagination(
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IList<NexportSupplementalInfoQuestion> GetAllNexportSupplementalInfoQuestions();

        NexportSupplementalInfoQuestion GetNexportSupplementalInfoQuestionById(int questionId);

        IList<NexportSupplementalInfoQuestion> GetNexportSupplementalInfoQuestionsByIds(int[] questionIds);

        void InsertNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question);

        void DeleteNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question);

        void DeleteNexportSupplementalInfoQuestions(IList<NexportSupplementalInfoQuestion> questions);

        void UpdateNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question);

        NexportSupplementalInfoOption GetNexportSupplementalInfoOptionById(int optionId);

        IList<NexportSupplementalInfoOption> GetNexportSupplementalInfoOptionsByQuestionId(int questionId,
            bool showHidden = false);

        void InsertNexportSupplementalInfoOption(NexportSupplementalInfoOption option);

        void DeleteNexportSupplementalInfoOption(NexportSupplementalInfoOption option);

        void UpdateNexportSupplementalInfoOption(NexportSupplementalInfoOption option);

        NexportSupplementalInfoQuestionMapping GetNexportSupplementalInfoQuestionMappingById(int questionMappingId);

        IList<NexportSupplementalInfoQuestionMapping> GetNexportSupplementalInfoQuestionMappingsByProductMappingId(
            int nexportProductMappingId);

        NexportSupplementalInfoQuestionMapping GetNexportSupplementalInfoQuestionMapping(int nexportProductMappingId,
            int questionId);

        void InsertNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping);

        void DeleteNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping);

        void UpdateNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping);

        IPagedList<NexportSupplementalInfoOptionGroupAssociation> GetNexportSupplementalInfoOptionGroupAssociationsPagination(
            int optionId, int pageIndex = 0, int pageSize = int.MaxValue);

        IList<NexportSupplementalInfoOptionGroupAssociation> GetNexportSupplementalInfoOptionGroupAssociations(
            int optionId, bool excludeInactive = false);

        NexportSupplementalInfoOptionGroupAssociation GetNexportSupplementalInfoOptionGroupAssociationById(
            int groupAssociationId);

        void InsertNexportSupplementalInfoOptionGroupAssociation(
            NexportSupplementalInfoOptionGroupAssociation groupAssociation);

        void DeleteNexportSupplementalInfoOptionGroupAssociation(
            NexportSupplementalInfoOptionGroupAssociation groupAssociation);

        void UpdateNexportSupplementalInfoOptionGroupAssociation(
            NexportSupplementalInfoOptionGroupAssociation groupAssociation);

        void InsertNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer);

        void DeleteNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer);

        void UpdateNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer);

        IList<NexportSupplementalInfoAnswer> GetNexportSupplementalInfoAnswers(int customerId, int storeId,
            int? questionId = null);

        IPagedList<NexportSupplementalInfoAnswer> GetNexportSupplementalInfoAnswersPagination(int customerId, int? questionId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        NexportSupplementalInfoAnswer GetNexportSupplementalInfoAnswerById(int answerId);

        IPagedList<NexportSupplementalInfoQuestion> GetNexportSupplementalInfoAnsweredQuestionsPagination(int customerId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership);

        void DeleteNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership);

        void InsertNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement);

        void DeleteNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement);

        IList<NexportRequiredSupplementalInfo> GetNexportRequiredSupplementalInfos(int customerId, int storeId, int? questionId = null);

        NexportRequiredSupplementalInfo GetNexportRequiredSupplementalInfoByNopProductId(int customerId, int storeId, int questionId);

        bool HasRequiredSupplementalInfo(int customerId, int storeId);

        bool HasUnprocessedAnswer(int orderId);

        void InsertNexportSupplementalInfoAnswerProcessingQueueItem(NexportSupplementalInfoAnswerProcessingQueueItem queueItem);

        void DeleteNexportSupplementalInfoAnswerProcessingQueueItem(NexportSupplementalInfoAnswerProcessingQueueItem queueItem);

        IList<NexportSupplementalInfoAnswerMembership> GetNexportSupplementalInfoAnswerMembershipsByAnswerId(int answerId);

        NexportSupplementalInfoAnswerMembership GetNexportSupplementalInfoAnswerMembership(Guid nexportMembershipId);

        void InsertNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem);

        void DeleteNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem);
    }
}
