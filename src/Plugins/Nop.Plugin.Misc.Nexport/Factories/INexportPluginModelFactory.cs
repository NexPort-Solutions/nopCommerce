using System;
using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;

namespace Nop.Plugin.Misc.Nexport.Factories
{
    public partial interface INexportPluginModelFactory
    {
        NexportProductMappingModel PrepareNexportProductMappingModel(NexportProductMapping productMapping, bool isEditable);

        NexportProductMappingSearchModel PrepareNexportProductMappingSearchModel(
            NexportProductMappingSearchModel searchModel);

        MapProductToNexportProductListModel PrepareMapProductToNexportProductListModel(
            NexportProductMappingSearchModel searchModel);

        NexportProductMappingListModel PrepareNexportProductMappingListModel(
            NexportProductMappingSearchModel searchModel, Guid nexportProductId,
            NexportProductTypeEnum nexportProductType);

        NexportProductMappingListModel PrepareNexportProductMappingListModel(
            NexportProductMappingSearchModel searchModel, int nopProductId);

        NexportProductGroupMembershipMappingListModel PrepareNexportProductMappingGroupMembershipListModel(
            NexportProductGroupMembershipMappingSearchModel searchModel, int nexportProductMappingId);

        NexportCustomerAdditionalInfoModel PrepareNexportAdditionalInfoModel(Customer customer);

        NexportCatalogListModel PrepareNexportCatalogListModel(NexportCatalogSearchModel searchModel);

        NexportSyllabusListModel PrepareNexportSyllabusListModel(NexportSyllabusListSearchModel searchModel);

        NexportLoginModel PrepareNexportLoginModel(bool? checkoutAsGuest);

        NexportTrainingListModel PrepareNexportTrainingListModel(Customer customer);

        NexportCustomerSupplementalInfoAnswersModel PrepareNexportCustomerSupplementalInfoAnswersModel(Customer customer, Store store);

        NexportCustomerSupplementalInfoAnswerEditModel PrepareNexportCustomerSupplementalInfoAnswersEditModel(
            Customer customer, Store store, NexportSupplementalInfoQuestion question);

        NexportCustomerSupplementalInfoAnsweredQuestionListModel PrepareNexportSupplementalInfoQuestionListModel(
            NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel);

        NexportSupplementalInfoAnswerListModel PrepareNexportSupplementalInfoAnswerListModel(
            NexportSupplementalInfoAnswerListSearchModel searchModel);

        NexportSupplementalInfoQuestionSearchModel PrepareNexportSupplementalInfoQuestionSearchModel(
            NexportSupplementalInfoQuestionSearchModel searchModel);

        NexportSupplementalInfoQuestionListModel PrepareNexportSupplementalInfoQuestionListModel(
            NexportSupplementalInfoQuestionSearchModel searchModel);

        NexportSupplementalInfoQuestionModel PrepareNexportSupplementalInfoQuestionModel(
            NexportSupplementalInfoQuestionModel model, NexportSupplementalInfoQuestion question);

        NexportSupplementalInfoOptionSearchModel PrepareNexportSupplementalInfoOptionSearchModel(NexportSupplementalInfoOptionSearchModel searchModel,
            NexportSupplementalInfoQuestion question);

        NexportSupplementalInfoOptionListModel PrepareNexportSupplementalInfoOptionListModel(
            NexportSupplementalInfoOptionSearchModel searchModel, NexportSupplementalInfoQuestion question);

        NexportSupplementalInfoOptionModel PrepareNexportSupplementalInfoOptionModel(
            NexportSupplementalInfoOptionModel model, NexportSupplementalInfoQuestion question,
            NexportSupplementalInfoOption option);

        NexportSupplementalInfoOptionGroupAssociationListModel PrepareNexportSupplementalInfoOptionGroupAssociationListModel(
            NexportSupplementalInfoOptionGroupAssociationSearchModel searchModel, int optionId);

        NexportSupplementalInfoAnswerQuestionModel PrepareNexportSupplementalInfoAnswerQuestionModel(
            IList<int> questionIds, Customer customer, Store store);

        NexportCustomerAdditionalSettingsModel PrepareNexportCustomerAdditionalSettingsModel();

        NexportRegistrationFieldCategoryListModel PrepareNexportRegistrationFieldCategoryListModel(
            NexportRegistrationFieldCategorySearchModel searchModel);

        NexportRegistrationFieldCategoryModel PrepareNexportRegistrationFieldCategoryModel(NexportRegistrationFieldCategoryModel model,
            NexportRegistrationFieldCategory registrationFieldCategory);

        NexportRegistrationFieldListModel PrepareNexportRegistrationFieldListModel(
            NexportRegistrationFieldSearchModel searchModel);

        NexportRegistrationFieldModel PrepareNexportRegistrationFieldModel(NexportRegistrationFieldModel model,
            NexportRegistrationField registrationField, bool excludeProperties = false);

        NexportRegistrationFieldOptionSearchModel PrepareNexportRegistrationFieldOptionSearchModel(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField);

        NexportRegistrationFieldOptionListModel PrepareNexportRegistrationFieldOptionListModel(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField);

        NexportRegistrationFieldOptionModel PrepareNexportRegistrationFieldOptionModel(
            NexportRegistrationFieldOptionModel model, NexportRegistrationField registrationField,
            NexportRegistrationFieldOption registrationFieldOption);

        NexportCustomerRegistrationFieldsModel PrepareNexportCustomerRegistrationFieldsModel(Store store);

        NexportOrderInvoiceItemListModel PrepareNexportOrderInvoiceItemListModel(NexportOrderInvoiceItemSearchModel searchModel,
            bool excludeNonApproval = false);

        NexportOrderInvoiceItemModel PrepareNexportOrderInvoiceItemModel(NexportOrderInvoiceItemModel model,
            NexportOrderInvoiceItem orderInvoiceItem);
    }
}
