using System;
using System.Collections.Generic;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
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

        IList<NexportOrderInvoiceItem> GetNexportOrderInvoiceItems(Guid userId);

        IPagedList<NexportOrderInvoiceItem> GetNexportOrderInvoiceItems(int orderId, bool excludeNonApproval = false,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

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

        #region Supplemental Info

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

        #endregion

        #region Registration Field

        NexportRegistrationField GetNexportRegistrationFieldById(int fieldId, int? categoryId = null);

        IList<NexportRegistrationField> GetNexportRegistrationFields(int storeId);

        IList<NexportRegistrationField> GetNexportRegistrationFieldsByCategoryId(int categoryId);

        IPagedList<NexportRegistrationField> GetNexportRegistrationFieldsPagination(
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertNexportRegistrationField(NexportRegistrationField registrationField);

        void DeleteNexportRegistrationField(NexportRegistrationField registrationField);

        void UpdateNexportRegistrationField(NexportRegistrationField registrationField);

        NexportRegistrationFieldOption GetNexportRegistrationFieldOptionById(int fieldOptionId, int? fieldId = null);

        IList<NexportRegistrationFieldOption> GetNexportRegistrationFieldOptions(int? fieldId = null);

        IPagedList<NexportRegistrationFieldOption> GetNexportRegistrationFieldOptionsPagination(int fieldId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption);

        void DeleteNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption);

        void UpdateNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption);

        NexportRegistrationFieldCategory GetNexportRegistrationFieldCategoryById(int fieldCategoryId);

        IList<NexportRegistrationFieldCategory> GetNexportRegistrationFieldCategories();

        IList<NexportRegistrationFieldCategory> GetNexportRegistrationFieldCategories(IList<int> fieldCategoryIds);

        IPagedList<NexportRegistrationFieldCategory> GetNexportRegistrationFieldCategoriesPagination(
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory);

        void DeleteNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory);

        void UpdateNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory);

        NexportRegistrationFieldStoreMapping GetNexportRegistrationFieldStoreMappingById(int fieldStoreMappingId);

        IList<NexportRegistrationFieldStoreMapping> GetNexportRegistrationFieldStoreMappings(int fieldId);

        void InsertNexportRegistrationFieldStoreMapping(NexportRegistrationFieldStoreMapping registrationFieldStoreMapping);

        void DeleteNexportRegistrationFieldStoreMapping(NexportRegistrationFieldStoreMapping registrationFieldStoreMapping);

        NexportRegistrationFieldAnswer GetNexportRegistrationFieldAnswerById(int fieldAnswerId);

        IList<NexportRegistrationFieldAnswer> GetNexportRegistrationFieldAnswers(int customerId);

        IPagedList<NexportRegistrationFieldAnswer> GetNexportRegistrationFieldAnswersPagination(int customerId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer);

        void DeleteNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer);

        void UpdateNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer);

        void InsertNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem);

        void DeleteNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem);

        void UpdateNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem);

        bool HasCustomRegistrationFieldRenderForStores(int fieldId, IList<int> storeIds, string customFieldRender);

        #endregion
    }
}
