using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.Nexport.Factories
{
    public partial interface INexportPluginModelFactory
    {
        Task<NexportProductMappingModel> PrepareNexportProductMappingModelAsync(NexportProductMapping productMapping,
            bool isEditable);

        Task<NexportProductMappingListSearchModel> PrepareNexportProductMappingListSearchModelAsync(
            NexportProductMappingListSearchModel searchModel, ProductModel productModel);

        Task<NexportProductMappingListModel> PrepareNexportProductMappingListModelAsync(
            NexportProductMappingListSearchModel searchModel, int nopProductId);

        Task<NexportProductGroupMembershipMappingListModel> PrepareNexportProductMappingGroupMembershipListModelAsync(
            NexportProductGroupMembershipMappingListSearchModel searchModel);

        Task<DuplicateNexportProductMappingModel> PrepareDuplicateNexportProductMappingModel(Product product);

        Task<AddNexportCustomerAdditionalInfoModel> PrepareAddNexportAdditionalInfoModel(Customer customer);

        Task<NexportCustomerAdditionalInfoModel> PrepareNexportAdditionalInfoModelAsync(Customer customer);

        Task<NexportCatalogListModel> PrepareNexportCatalogListModelAsync(NexportCatalogSearchModel searchModel);

        Task<NexportSyllabusListModel> PrepareNexportSyllabusListModelAsync(NexportSyllabusListSearchModel searchModel);

        Task<NexportLoginModel> PrepareNexportLoginModelAsync(bool? checkoutAsGuest);

        Task<NexportTrainingListModel> PrepareNexportTrainingListModelAsync(Customer customer);

        Task<NexportCustomerSupplementalInfoAnswersModel> PrepareNexportCustomerSupplementalInfoAnswersModelAsync(
            Customer customer, Store store);

        Task<NexportCustomerSupplementalInfoAnswerEditModel>
            PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(Customer customer, Store store,
                NexportSupplementalInfoQuestion question);

        Task<NexportCustomerSupplementalInfoAnsweredQuestionListModel>
            PrepareNexportSupplementalInfoQuestionListModelAsync(
                NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel);

        Task<NexportSupplementalInfoAnswerListModel> PrepareNexportSupplementalInfoAnswerListModelAsync(
            NexportSupplementalInfoAnswerListSearchModel searchModel);

        Task<NexportSupplementalInfoQuestionSearchModel> PrepareNexportSupplementalInfoQuestionSearchModelAsync(
            NexportSupplementalInfoQuestionSearchModel searchModel);

        Task<NexportSupplementalInfoQuestionListModel> PrepareNexportSupplementalInfoQuestionListModelAsync(
            NexportSupplementalInfoQuestionSearchModel searchModel);

        Task<NexportSupplementalInfoQuestionModel> PrepareNexportSupplementalInfoQuestionModelAsync(
            NexportSupplementalInfoQuestionModel model, NexportSupplementalInfoQuestion question);

        Task<NexportSupplementalInfoOptionSearchModel> PrepareNexportSupplementalInfoOptionSearchModelAsync(
            NexportSupplementalInfoOptionSearchModel searchModel,
            NexportSupplementalInfoQuestion question);

        Task<NexportSupplementalInfoOptionListModel> PrepareNexportSupplementalInfoOptionListModelAsync(
            NexportSupplementalInfoOptionSearchModel searchModel, NexportSupplementalInfoQuestion question);

        Task<NexportSupplementalInfoOptionModel> PrepareNexportSupplementalInfoOptionModelAsync(
            NexportSupplementalInfoOptionModel model, NexportSupplementalInfoQuestion question,
            NexportSupplementalInfoOption option);

        Task<NexportSupplementalInfoOptionGroupAssociationListModel>
            PrepareNexportSupplementalInfoOptionGroupAssociationListModelAsync(
                NexportSupplementalInfoOptionGroupAssociationListSearchModel searchModel);

        Task<NexportSupplementalInfoAnswerQuestionModel> PrepareNexportSupplementalInfoAnswerQuestionModelAsync(
            IList<int> questionIds, Customer customer, Store store);

        Task<NexportCustomerAdditionalSettingsModel> PrepareNexportCustomerAdditionalSettingsModelAsync();

        Task<NexportRegistrationFieldSearchModel> PrepareNexportRegistrationFieldSearchModelAsync(
            NexportRegistrationFieldSearchModel searchModel);

        Task<NexportRegistrationFieldCategoryListModel> PrepareNexportRegistrationFieldCategoryListModelAsync(
            NexportRegistrationFieldCategorySearchModel searchModel);

        Task<NexportRegistrationFieldCategoryModel> PrepareNexportRegistrationFieldCategoryModelAsync(
            NexportRegistrationFieldCategoryModel model,
            NexportRegistrationFieldCategory registrationFieldCategory);

        Task<NexportRegistrationFieldListModel> PrepareNexportRegistrationFieldListModelAsync(
            NexportRegistrationFieldSearchModel searchModel);

        Task<NexportRegistrationFieldModel> PrepareNexportRegistrationFieldModelAsync(
            NexportRegistrationFieldModel model,
            NexportRegistrationField registrationField, bool excludeProperties = false);

        Task<NexportRegistrationFieldOptionSearchModel> PrepareNexportRegistrationFieldOptionSearchModelAsync(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField);

        Task<NexportRegistrationFieldOptionListModel> PrepareNexportRegistrationFieldOptionListModelAsync(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField);

        Task<NexportRegistrationFieldOptionModel> PrepareNexportRegistrationFieldOptionModelAsync(
            NexportRegistrationFieldOptionModel model, NexportRegistrationField registrationField,
            NexportRegistrationFieldOption registrationFieldOption);

        Task<NexportCustomerRegistrationFieldsModel> PrepareNexportCustomerRegistrationFieldsModelAsync(Store store);

        Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(Store store);

        Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(
            Customer customer, Store store);

        Task<NexportCustomerRegistrationFieldAnswerListModel> PrepareNexportCustomerRegistrationFieldAnswerListModel(
            NexportCustomerRegistrationFieldAnswerListSearchModel searchModel);

        Task<NexportCustomerRegistrationFieldWithAnswersListModel>
            PrepareNexportCustomerRegistrationFieldWithAnswersListModel(
                NexportCustomerRegistrationFieldWithAnswersListSearchModel searchModel);

        Task<NexportCustomerRegistrationFieldAnswersEditModel> PrepareNexportCustomerRegistrationFieldAnswersEditModel(
            Customer customer, NexportRegistrationField registrationField);

        Task<NexportOrderInvoiceItemListModel> PrepareNexportOrderInvoiceItemListModelAsync(
            NexportOrderInvoiceItemSearchModel searchModel, bool excludeNonApproval = false);

        Task<NexportOrderInvoiceItemModel> PrepareNexportOrderInvoiceItemModelAsync(NexportOrderInvoiceItemModel model,
            NexportOrderInvoiceItem orderInvoiceItem);

        Task<StoreListModel> PrepareStoreListModel(NexportStoreSearchModel searchModel);

        Task<NexportOrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel);

        Task<WholesaleCreateModel> PrepareWholesaleOrderModelAsync();
        Task<OrderSummaryCartFooterModel> PrepareOrderSummaryCartFooterModel(
            OrderSummaryCartFooterModel orderSummaryCartFooterModel, Customer? customer, Store? store,IList<ShoppingCartItem?> cart);
        Task<CustomerNexportGroupsModel> PrepareCustomerNexportGroupsModelAsync(int customerId,int? page);
        Task<CustomerNexportGroupProductsModel> PrepareCustomerNexportGroupProductsModelAsync(Guid groupId, int? page);

    }
}
