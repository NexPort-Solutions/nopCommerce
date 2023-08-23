using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public interface INexportService
    {
        Task InsertNexportProductMapping(NexportProductMapping nexportProductMapping);

        Task InsertNexportProductGroupMembershipMapping(
            NexportProductGroupMembershipMapping nexportProductGroupMembershipMapping);

        Task<IPagedList<NexportProductMapping>> GetProductCatalogsByCatalogId(Guid catalogId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task<IPagedList<NexportProductMapping>> GetProductSectionsBySectionId(Guid sectionId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task<IPagedList<NexportProductMapping>> GetProductTrainingPlansByTrainingPlanId(Guid trainingPlanId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        NexportProductMapping FindProductCatalog(IList<NexportProductMapping> source, int productId,
            Guid catalogId, int? storeId = null);

        NexportProductMapping FindProductSection(IList<NexportProductMapping> source, int productId,
            Guid sectionId, int? storeId = null);

        NexportProductMapping FindProductTrainingPlan(IList<NexportProductMapping> source, int productId,
            Guid trainingPlan, int? storeId = null);

        Task<IList<NexportProductMapping>> GetProductMappingsByStoreId(int storeId);

        Task<NexportProductMapping> GetProductMappingByNopProductId(int nopProductId, int? storeId = null);

        Task<IList<NexportProductMapping>> GetProductMappings(int? nopProductId = null, int? storeId = null);

        Task<IList<NexportProductGroupMembershipMapping>> GetProductGroupMembershipMappings(
            int nexportProductMappingId);

        Task<IPagedList<NexportProductGroupMembershipMapping>> GetProductGroupMembershipMappingsPagination(
            int nexportProductMappingId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task<IList<Guid>> GetProductGroupMembershipIds(int nexportProductMappingId);

        Task<Dictionary<Guid, int>> FindMappingCountPerSyllabus(IList<GetSyllabiResponseItem> syllabusList);

        Task<NexportProductMapping> GetProductMappingById(int mappingId);

        Task DeleteNexportProductMapping(NexportProductMapping mapping);

        Task UpdateNexportProductMapping(NexportProductMapping mapping);

        Task<NexportProductGroupMembershipMapping> GetProductGroupMembershipMappingById(int mappingId);

        Task DeleteGroupMembershipMapping(NexportProductGroupMembershipMapping mapping);

        Task InsertNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem);

        Task DeleteNexportOrderProcessingQueueItem(NexportOrderProcessingQueueItem queueItem);

        Task InsertOrUpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        Task DeleteNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        Task UpdateNexportOrderInvoiceItem(NexportOrderInvoiceItem item);

        Task InsertNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem);

        Task DeleteNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem);

        Task UpdateNexportOrderInvoiceRedemptionQueueItem(NexportOrderInvoiceRedemptionQueueItem queueItem);

        //TODO @JS - this probably needs to go away in favor of the one that return list of invoiceitems
        Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItem(int orderId, int orderItemId);
        
        Task<IList<NexportOrderInvoiceItem>?> FindNexportOrderInvoiceItems(int orderId, int orderItemId);

        Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItemById(int orderInvoiceItemId);

        Task<IList<NexportOrderInvoiceItem>> GetNexportOrderInvoiceItems(Guid userId);

        Task<IPagedList<NexportOrderInvoiceItem>> GetNexportOrderInvoiceItems(int orderId,
            bool excludeNonApproval = false,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task MapNexportProduct(MapNexportProductModel model);

        Task InsertUserMapping(NexportUserMapping nexportUserMapping);

        Task DeleteUserMapping(NexportUserMapping nexportUserMapping);

        Task UpdateUserMapping(NexportUserMapping nexportUserMapping);

        Task<NexportUserMapping> FindUserMappingByCustomerId(int nopCustomerId);

        Task<NexportUserMapping> FindUserMappingByNexportUserId(Guid userId);

        Task<Guid?> FindExistingInvoiceForOrder(int orderId);

        Task<Guid?> FindExistingInvoiceItemForOrderItem(int orderId, int orderItemId);

        Task<bool> HasNexportOrderProcessingQueueItem(int orderId);

        Task<bool> HasNexportProductMapping(Order order);

        Task<bool> HasNexportProductMapping(int productId);

        Task<IList<int?>> GetStoreIdsPerProductMapping(int productId);

        Task CopyProductMappingsAsync(Product originalProduct, Product copyingProduct);

        Task DuplicateProductMappingAsync(NexportProductMapping productMapping, int storeId);

        #region Supplemental Info

        Task<IPagedList<NexportSupplementalInfoQuestion>> GetAllNexportSupplementalInfoQuestionsPagination(
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task<IList<NexportSupplementalInfoQuestion>> GetAllNexportSupplementalInfoQuestions();

        Task<NexportSupplementalInfoQuestion> GetNexportSupplementalInfoQuestionById(int questionId);

        Task<IList<NexportSupplementalInfoQuestion>> GetNexportSupplementalInfoQuestionsByIds(int[] questionIds);

        Task InsertNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question);

        Task DeleteNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question);

        Task DeleteNexportSupplementalInfoQuestions(IList<NexportSupplementalInfoQuestion> questions);

        Task UpdateNexportSupplementalInfoQuestion(NexportSupplementalInfoQuestion question);

        Task<NexportSupplementalInfoOption> GetNexportSupplementalInfoOptionById(int optionId);

        Task<IList<NexportSupplementalInfoOption>> GetNexportSupplementalInfoOptionsByQuestionId(int questionId,
            bool showHidden = false);

        Task InsertNexportSupplementalInfoOption(NexportSupplementalInfoOption option);

        Task DeleteNexportSupplementalInfoOption(NexportSupplementalInfoOption option);

        Task UpdateNexportSupplementalInfoOption(NexportSupplementalInfoOption option);

        Task<NexportSupplementalInfoQuestionMapping> GetNexportSupplementalInfoQuestionMappingById(
            int questionMappingId);

        Task<IList<NexportSupplementalInfoQuestionMapping>>
            GetNexportSupplementalInfoQuestionMappingsByProductMappingId(int nexportProductMappingId);

        Task<NexportSupplementalInfoQuestionMapping> GetNexportSupplementalInfoQuestionMapping(
            int nexportProductMappingId,
            int questionId);

        Task InsertNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping);

        Task DeleteNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping);

        Task UpdateNexportSupplementalInfoQuestionMapping(NexportSupplementalInfoQuestionMapping questionMapping);

        Task<IPagedList<NexportSupplementalInfoOptionGroupAssociation>> GetNexportSupplementalInfoOptionGroupAssociationsPagination(
            int optionId, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<NexportSupplementalInfoOptionGroupAssociation>> GetNexportSupplementalInfoOptionGroupAssociations(
            int optionId, bool excludeInactive = false);

        Task<NexportSupplementalInfoOptionGroupAssociation> GetNexportSupplementalInfoOptionGroupAssociationById(
            int groupAssociationId);

        Task InsertNexportSupplementalInfoOptionGroupAssociation(
            NexportSupplementalInfoOptionGroupAssociation groupAssociation);

        Task DeleteNexportSupplementalInfoOptionGroupAssociation(
            NexportSupplementalInfoOptionGroupAssociation groupAssociation);

        Task UpdateNexportSupplementalInfoOptionGroupAssociation(
            NexportSupplementalInfoOptionGroupAssociation groupAssociation);

        Task InsertNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer);

        Task DeleteNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer);

        Task UpdateNexportSupplementalInfoAnswer(NexportSupplementalInfoAnswer answer);

        Task<IList<NexportSupplementalInfoAnswer>> GetNexportSupplementalInfoAnswers(int customerId, int storeId,
            int? questionId = null);

        Task<IPagedList<NexportSupplementalInfoAnswer>> GetNexportSupplementalInfoAnswersPagination(int customerId,
            int? questionId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task<NexportSupplementalInfoAnswer> GetNexportSupplementalInfoAnswerById(int answerId);

        Task<IPagedList<NexportSupplementalInfoQuestion>> GetNexportSupplementalInfoAnsweredQuestionsPagination(
            int customerId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership);

        Task DeleteNexportSupplementalInfoAnswerMembership(NexportSupplementalInfoAnswerMembership answerMembership);

        Task InsertNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement);

        Task DeleteNexportRequiredSupplementalInfo(NexportRequiredSupplementalInfo requirement);

        Task<IList<NexportRequiredSupplementalInfo>> GetNexportRequiredSupplementalInfos(int customerId, int storeId,
            int? questionId = null);

        Task<NexportRequiredSupplementalInfo> GetNexportRequiredSupplementalInfoByNopProductId(int customerId,
            int storeId, int questionId);

        Task<bool> HasRequiredSupplementalInfo(int customerId, int storeId);

        Task<bool> HasUnprocessedAnswer(int orderId);

        Task InsertNexportSupplementalInfoAnswerProcessingQueueItem(
            NexportSupplementalInfoAnswerProcessingQueueItem queueItem);

        Task DeleteNexportSupplementalInfoAnswerProcessingQueueItem(
            NexportSupplementalInfoAnswerProcessingQueueItem queueItem);

        Task<IList<NexportSupplementalInfoAnswerMembership>> GetNexportSupplementalInfoAnswerMembershipsByAnswerId(
            int answerId);

        Task<NexportSupplementalInfoAnswerMembership> GetNexportSupplementalInfoAnswerMembership(
            Guid nexportMembershipId);

        Task InsertNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem);

        Task DeleteNexportGroupMembershipRemovalQueueItem(NexportGroupMembershipRemovalQueueItem queueItem);

        #endregion

        #region Registration Field

        Task<NexportRegistrationField> GetNexportRegistrationFieldById(int fieldId, int? categoryId = null);

        Task<IList<NexportRegistrationField>> GetNexportRegistrationFields(int storeId);

        Task<IList<NexportRegistrationField>> GetNexportRegistrationFieldsByCategoryId(int categoryId);

        Task<IPagedList<NexportRegistrationField>> GetNexportRegistrationFieldsPagination(IList<int> storeIds,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertNexportRegistrationField(NexportRegistrationField registrationField);

        Task DeleteNexportRegistrationField(NexportRegistrationField registrationField);

        Task UpdateNexportRegistrationField(NexportRegistrationField registrationField);

        Task<NexportRegistrationFieldOption> GetNexportRegistrationFieldOptionById(int fieldOptionId,
            int? fieldId = null);

        Task<IList<NexportRegistrationFieldOption>> GetNexportRegistrationFieldOptions(int? fieldId = null);

        Task<IPagedList<NexportRegistrationFieldOption>> GetNexportRegistrationFieldOptionsPagination(int fieldId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IPagedList<NexportRegistrationField>> GetNexportRegistrationFieldsWithAnswersPagination(int customerId,
            int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<NexportRegistrationField>> GetNexportRegistrationFieldsWithAnswers(int customerId,
            int? storeId = null);

        Task InsertNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption);

        Task DeleteNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption);

        Task UpdateNexportRegistrationFieldOption(NexportRegistrationFieldOption registrationFieldOption);

        Task<NexportRegistrationFieldCategory> GetNexportRegistrationFieldCategoryById(int fieldCategoryId);

        Task<IList<NexportRegistrationFieldCategory>> GetNexportRegistrationFieldCategories();

        Task<IList<NexportRegistrationFieldCategory>> GetNexportRegistrationFieldCategories(IList<int> fieldCategoryIds);

        Task<IPagedList<NexportRegistrationFieldCategory>> GetNexportRegistrationFieldCategoriesPagination(
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory);

        Task DeleteNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory);

        Task UpdateNexportRegistrationFieldCategory(NexportRegistrationFieldCategory registrationFieldCategory);

        Task<NexportRegistrationFieldStoreMapping> GetNexportRegistrationFieldStoreMappingById(int fieldStoreMappingId);

        Task<IList<NexportRegistrationFieldStoreMapping>> GetNexportRegistrationFieldStoreMappings(int fieldId);

        Task InsertNexportRegistrationFieldStoreMapping(NexportRegistrationFieldStoreMapping registrationFieldStoreMapping);

        Task DeleteNexportRegistrationFieldStoreMapping(NexportRegistrationFieldStoreMapping registrationFieldStoreMapping);

        Task<NexportRegistrationFieldAnswer> GetNexportRegistrationFieldAnswerById(int fieldAnswerId);

        Task<IList<NexportRegistrationFieldAnswer>> GetNexportRegistrationFieldAnswers(int customerId, int? fieldId = null);

        Task<IPagedList<NexportRegistrationFieldAnswer>> GetNexportRegistrationFieldAnswersPagination(int customerId,
            int? fieldId = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer);

        Task DeleteNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer);

        Task UpdateNexportRegistrationFieldAnswer(NexportRegistrationFieldAnswer registrationFieldAnswer);

        Task InsertNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem);

        Task DeleteNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem);

        Task UpdateNexportRegistrationFieldSynchronizationQueueItem(NexportRegistrationFieldSynchronizationQueueItem queueItem);

        Task<NexportRegistrationFieldAnswer> GetNexportRegistrationFieldAnswerByFieldOption(int customerId, int fieldId,
            int fieldOptionId);

        Task<bool> HasCustomRegistrationFieldRenderForStores(int fieldId, IList<int> storeIds, string customFieldRender);

        #endregion

        Task<IPagedList<NexportProductMapping>> GetAllNexportProductMappingsAsync(string searchProductName, NexportProductTypeEnum? searchproductType, string searchStoreName, int productId, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<NexportOrderInvoiceItem?> FindNexportOrderInvoiceItemByGuid(Guid? orderInvoiceItemId);

        Task<Customer> FindCustomerByGuid(Guid? customerId);

        Task<int> GetInvoiceItemCountForGroupByGuid(Guid groupId);

        Task<NexportOrderInvoiceItem> FindNexportOrderInvoiceItemByInvoiceItemGuid(Guid invoiceItemId);

        Task<IList<NexportGroupProductModel>> GetGroupProductModelForGroupId(Guid groupId);

        Task<IList<NexportOrderInvoiceItem>> GetInvoiceItemsForGroupIdAndProductIdAndRedeemingUserIdHasValue(Guid groupId, int productId);

    }
}
