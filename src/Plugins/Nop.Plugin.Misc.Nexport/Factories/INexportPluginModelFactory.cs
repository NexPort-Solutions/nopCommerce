using NexportApi.Model;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Enrollment;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.Products;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.Refund;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.Plugins;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Stores;

namespace Nop.Plugin.Misc.Nexport.Factories;

public partial interface INexportPluginModelFactory
{
    Task<NexportPluginResourceListModel> PrepareNexportPluginResourceListModelAsync(NexportPluginResourceListSearchModel searchModel);

    Task<NexportProductMappingModel> PrepareNexportProductMappingModelAsync(NexportProductMapping productMapping, bool isEditable);

    Task<NexportProductMappingListSearchModel> PrepareNexportProductMappingListSearchModelAsync(NexportProductMappingListSearchModel searchModel, ProductModel productModel);

    Task<NexportProductMappingListModel> PrepareNexportProductMappingListModelAsync(NexportProductMappingListSearchModel searchModel, int nopProductId);

    Task<NexportProductGroupMembershipMappingListModel> PrepareNexportProductMappingGroupMembershipListModelAsync(NexportProductGroupMembershipMappingListSearchModel searchModel);

    Task<DuplicateNexportProductMappingModel> PrepareDuplicateNexportProductMappingModel(Product product);

    Task<AddNexportCustomerAdditionalInfoModel> PrepareAddNexportAdditionalInfoModel(Customer customer);

    Task<NexportCustomerAdditionalInfoModel> PrepareNexportAdditionalInfoModelAsync(Customer customer);

    Task<NexportCatalogListModel> PrepareNexportCatalogListModelAsync(NexportCatalogSearchModel searchModel);

    Task<NexportSyllabusListModel> PrepareNexportSyllabusListModelAsync(NexportSyllabusListSearchModel searchModel);

    Task<NexportLoginModel> PrepareNexportLoginModelAsync(bool? checkoutAsGuest);

    Task<NexportTrainingListModel> PrepareNexportTrainingListModelAsync(Customer customer);

    Task<NexportEnrollmentListModel> PrepareNexportEnrollmentListModelAsync(NexportEnrollmentListSearchModel searchModel);

    Task<NexportCustomerSupplementalInfoAnswersModel> PrepareNexportCustomerSupplementalInfoAnswersModelAsync(Customer customer, Store store);

    Task<NexportCustomerSupplementalInfoAnswerEditModel> PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(
        Customer customer, Store store, NexportSupplementalInfoQuestion question);

    Task<NexportCustomerSupplementalInfoAnsweredQuestionListModel> PrepareNexportSupplementalInfoQuestionListModelAsync(
        NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel);

    Task<NexportSupplementalInfoAnswerListModel> PrepareNexportSupplementalInfoAnswerListModelAsync(NexportSupplementalInfoAnswerListSearchModel searchModel);

    Task<NexportSupplementalInfoQuestionSearchModel> PrepareNexportSupplementalInfoQuestionSearchModelAsync(NexportSupplementalInfoQuestionSearchModel searchModel);

    Task<NexportSupplementalInfoQuestionListModel> PrepareNexportSupplementalInfoQuestionListModelAsync(NexportSupplementalInfoQuestionSearchModel searchModel);

    Task<NexportSupplementalInfoQuestionModel> PrepareNexportSupplementalInfoQuestionModelAsync(NexportSupplementalInfoQuestionModel model, NexportSupplementalInfoQuestion question);

    Task<NexportSupplementalInfoOptionSearchModel> PrepareNexportSupplementalInfoOptionSearchModelAsync(
        NexportSupplementalInfoOptionSearchModel searchModel, NexportSupplementalInfoQuestion question);

    Task<NexportSupplementalInfoOptionListModel> PrepareNexportSupplementalInfoOptionListModelAsync(
        NexportSupplementalInfoOptionSearchModel searchModel, NexportSupplementalInfoQuestion question);

    Task<NexportSupplementalInfoOptionModel> PrepareNexportSupplementalInfoOptionModelAsync(
        NexportSupplementalInfoOptionModel model, NexportSupplementalInfoQuestion question, NexportSupplementalInfoOption option);

    Task<NexportSupplementalInfoOptionGroupAssociationListModel> PrepareNexportSupplementalInfoOptionGroupAssociationListModelAsync(
        NexportSupplementalInfoOptionGroupAssociationListSearchModel searchModel);

    Task<NexportSupplementalInfoAnswerQuestionModel> PrepareNexportSupplementalInfoAnswerQuestionModelAsync(
        IList<int> questionIds, Customer customer, Store store);

    Task<NexportCustomerAdditionalSettingsModel> PrepareNexportCustomerAdditionalSettingsModelAsync();

    Task<NexportRegistrationFieldSearchModel> PrepareNexportRegistrationFieldSearchModelAsync(
        NexportRegistrationFieldSearchModel searchModel);

    Task<NexportRegistrationFieldCategoryListModel> PrepareNexportRegistrationFieldCategoryListModelAsync(
        NexportRegistrationFieldCategorySearchModel searchModel);

    Task<NexportRegistrationFieldCategoryModel> PrepareNexportRegistrationFieldCategoryModelAsync(
        NexportRegistrationFieldCategoryModel model, NexportRegistrationFieldCategory registrationFieldCategory);

    Task<NexportRegistrationFieldListModel> PrepareNexportRegistrationFieldListModelAsync(
        NexportRegistrationFieldSearchModel searchModel);

    Task<NexportRegistrationFieldModel> PrepareNexportRegistrationFieldModelAsync(
        NexportRegistrationFieldModel model, NexportRegistrationField registrationField, bool excludeProperties = false);

    Task<NexportRegistrationFieldOptionSearchModel> PrepareNexportRegistrationFieldOptionSearchModelAsync(
        NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField);

    Task<NexportRegistrationFieldOptionListModel> PrepareNexportRegistrationFieldOptionListModelAsync(
        NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField);

    Task<NexportRegistrationFieldOptionModel> PrepareNexportRegistrationFieldOptionModelAsync(
        NexportRegistrationFieldOptionModel model, NexportRegistrationField registrationField,
        NexportRegistrationFieldOption registrationFieldOption);

    Task<NexportCustomerRegistrationFieldsModel> PrepareNexportCustomerRegistrationFieldsModelAsync(Store store);

    Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(Store store);

    Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(Customer customer, Store store);

    Task<NexportCustomerRegistrationFieldAnswerListModel> PrepareNexportCustomerRegistrationFieldAnswerListModel(
        NexportCustomerRegistrationFieldAnswerListSearchModel searchModel);

    Task<NexportCustomerRegistrationFieldWithAnswersListModel> PrepareNexportCustomerRegistrationFieldWithAnswersListModel(
        NexportCustomerRegistrationFieldWithAnswersListSearchModel searchModel);

    Task<NexportCustomerRegistrationFieldAnswersEditModel> PrepareNexportCustomerRegistrationFieldAnswersEditModel(
        Customer customer, NexportRegistrationField registrationField);

    Task<NexportOrderInvoiceItemListModel> PrepareNexportOrderInvoiceItemListModelAsync(
        NexportOrderInvoiceItemSearchModel searchModel, bool excludeNonApproval = false);

    Task<NexportOrderInvoiceItemModel> PrepareNexportOrderInvoiceItemModelAsync(NexportOrderInvoiceItemModel model,
        NexportOrderInvoiceItem orderInvoiceItem);

    Task<StoreListModel> PrepareStoreListModel(NexportStoreSearchModel searchModel);

    Task<NexportOrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel);

    Task<WholesaleOrderModel> PrepareWholesaleOrderModelAsync(WholesaleOrderModel model);

    Task<WholesaleOrderPurchasingProducts> PrepareWholesaleOrderPurchasingProductsListAsync(IList<int> productIds);

    Task<WholesaleOrderProductListModel> PrepareWholesaleOrderProductListModelAsync(WholesaleOrderProductSearchModel searchModel);

    Task<WholesaleOrderProductSearchModel> PrepareWholesaleOrderProductSearchModel(int storeId);

    Task<WholesaleOrderPaymentInfoModel> PrepareWholesaleOrderPaymentInfoModelAsync(string paymentSystemName);

    Task<OrderSummaryCartFooterModel> PrepareOrderSummaryCartFooterModel(
        OrderSummaryCartFooterModel orderSummaryCartFooterModel, Customer customer, Store store, IList<ShoppingCartItem> cart);

    Task<NexportGroupProductListModel> PrepareNexportGroupProductListModelAsync(NexportGroupProductListSearchModel searchModel, Customer currentCustomer);

    Task<NexportGroupProductRedemptionListModel> PrepareNexportGroupProductRedemptionListModelAsync(
        NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId, Customer currentCustomer, int? orderId = null);

    Task<NexportGroupProductListSearchModel> PrepareNexportGroupProductListSearchModelAsync(Guid groupId);

    Task<NexportGroupProductListSearchModel> PrepareNexportGroupProductListSearchModelAsync(int? customerId = null,
        int? productId = null, int? statusId = null);

    Task<NexportGroupProductRedemptionListSearchModel> PrepareNexportGroupProductRedemptionListSearchModelAsync(
        Guid? groupId, int productId, int? orderId = null);

    Task<NexportPurchasesByFundingPoolListSearchModel> PrepareNexportPurchasesByFundingPoolListSearchModelAsync(int? customerId = null,
        int? productId = null, int? statusId = null);

    Task<NexportPurchasesByFundingPoolListModel> PrepareNexportPurchasesByFundingPoolListModelAsync(NexportPurchasesByFundingPoolListSearchModel searchModel);

    Task<NexportProductRedemptionListSearchModel> PrepareNexportPurchasesByFundingPoolsRedemptionListSearchModelAsync(int? fundingPoolId);

    Task<NexportGroupProductRedemptionListModel> PrepareNexportWholesalePurchasesByFundingPoolsRedemptionListModelAsync(
        NexportProductRedemptionListSearchModel searchModel, int? orderId = null);

    Task<NexportUserAssignmentListModel> PrepareNexportUserAssignmentListModelAsync(NexportUserAssignmentListSearchModel searchModel);

    Task<RedeemProductModel> PrepareRedeemProductModel(Guid? groupId, Guid invoiceItemId, int productId);

    Task<RedeemByEmailModel> PrepareRedeemByEmailModel(int? invoiceItemId, string email, int? productMappingId);

    Task<ProductStepModel> PrepareProductStepModel(int productId, Guid invoiceItemId);

    Task<OptionStepModel> PrepareOptionStepModel(bool isOpenEndedProduct = false);

    Task<ConfirmStepModel> PrepareConfirmStepModel(DateTime? utcStartDate, int? storeId, Guid? purchasingGroupId,
        string purchasingGroupName, int? extensionOption = null, bool requireApproval = false);

    Task<RedeemActionModel> PrepareRedeemActionModel(bool isAdminView, NexportEnrollmentResponseItemModel enrollment);

    Task<MapProductToCategoryModel> PrepareMapProductToCategoryModel();

    Task<NexportProductMappingListModel> PrepareNexportCategoryProductMappingListModelAsync(NexportCategoryProductMappingListSearchModel searchModel);

    Task<NexportCategorySearchModel> PrepareCategorySearchModelAsync(NexportCategorySearchModel searchModel);

    Task<NexportCategoryListModel> PrepareCategoryListModelAsync(NexportCategorySearchModel searchModel);

    Task<NexportProductRedemptionStatusesModel> PrepareNexportProductRedemptionStatusesModel(Customer customer, int productId, int storeId);

    Task<SubmitRedemptionUnassignmentRequestModel> PrepareSubmitUnassignmentRequestModel(Guid? groupId, Guid? invoiceItemId, int? productId, int? customerId);

    Task<NexportRedemptionRequestUnassignmentListModel> PrepareNexportRedemptionUnassignmentRequestListModel(
        NexportRedemptionUnassignmentRequestListSearchModel searchModel);

    Task<NexportRedemptionUnassignmentRequestReasonListModel> PrepareRedemptionUnassignmentRequestReasonListModelAsync(
        NexportRedemptionUnassignmentRequestReasonSearchModel searchModel);

    Task<NexportRedemptionUnassignmentRequestReasonModel> PrepareRedemptionUnassignmentRequestReasonModelAsync(
        NexportRedemptionUnassignmentRequestReasonModel model,
        NexportRedemptionUnassignmentRequestReason cancellationRequestReason, bool excludeProperties = false);

    Task<NexportRedemptionUnassignmentRequestModel> PrepareRedemptionUnassignmentRequestModelAsync(
        NexportRedemptionUnassignmentRequestModel model, NexportRedemptionUnassignmentRequest unassignmentRequest,
        bool excludeProperties = false);

    Task<SubmitRedemptionUnassignmentRequestModel> PrepareSubmitRedemptionUnassignmentRequestModelAsync(SubmitRedemptionUnassignmentRequestModel model);

    Task<NexportRedemptionUnassignmentRequestListSearchModel> PrepareRedemptionUnassignmentRequestSearchModelAsync(
        NexportRedemptionUnassignmentRequestListSearchModel searchModel);

    Task<NexportRedemptionAssignmentApprovalRequestListSearchModel> PrepareNexportRedemptionAssignmentApprovalRequestSearchModelAsync(
        NexportRedemptionAssignmentApprovalRequestListSearchModel searchModel);

    Task<NexportRedemptionAssignmentApprovalRequestListModel> PrepareNexportRedemptionAssignmentApprovalRequestListModel(
        NexportRedemptionAssignmentApprovalRequestListSearchModel searchModel);

    Task<NexportRedemptionAssignmentApprovalRequestModel> PrepareNexportRedemptionAssignmentApprovalRequestModelAsync(
        NexportRedemptionAssignmentApprovalRequestModel model, NexportRedemptionAssignmentApprovalRequest assignmentApprovalRequest,
        bool excludeProperties = false);

    Task<NexportFundingPoolSearchModel> PrepareNexportFundingPoolSearchModelAsync(NexportFundingPoolSearchModel searchModel);

    Task<NexportFundingPoolListModel> PrepareNexportFundingPoolListModelAsync(NexportFundingPoolSearchModel searchModel);

    Task<NexportFundingPoolModel> PrepareNexportFundingPoolModelAsync(NexportFundingPoolModel model, NexportFundingPool fundingPool);

    Task<NexportEnrollmentListSearchModel> PrepareListNexportUserEnrollments(Customer customer, Guid organizationId);

    Task<NexportReturnRequestSearchModel> PrepareNexportReturnRequestSearchModelAsync(NexportReturnRequestSearchModel searchModel);

    Task<NexportReturnRequestListModel> PrepareNexportReturnRequestListModelAsync(NexportReturnRequestSearchModel searchModel);

    Task<NexportReturnRequestModel> PrepareNexportReturnRequestModelAsync(NexportReturnRequestModel model, ReturnRequest returnRequest,
        bool excludeProperties = false);

    Task<SubmitInvoiceItemRefundRequestModel> PrepareInvoiceItemRefundRequestModel(Guid invoiceItemId);

    Task<SubmitInvoiceItemRefundRequestModel> PrepareSubmitInvoiceItemRefundRequestModelAsync(SubmitInvoiceItemRefundRequestModel model);

    Task<NexportRedemptionAuditLogListSearchModel> PrepareNexportRedemptionAuditLogListSearchModelAsync(Guid invoiceItemId);

    Task<NexportRedemptionAuditLogListModel> PrepareNexportRedemptionAuditLogListModelAsync(NexportRedemptionAuditLogListSearchModel searchModel);

    Task<NexportRefundRequestListModel> PrepareNexportCustomerRegistrationFieldAnswerListModel(NexportRefundRequestListSearchModel searchModel);
}