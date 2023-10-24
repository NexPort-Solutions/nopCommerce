using NexportApi.Model;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Services.Orders;
using ErrorCode = NexportApi.Model.ApiErrorEntity.ErrorCodeEnum;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IOrganizationService
{
    Task<OrganizationResponseItem?> GetOrganizationDetails(Guid orgId);
    Task<List<OrganizationResponseItem>> FindAllOrganizations(Guid baseOrgId);
    Task<List<OrganizationResponseItem>> FindAllOrganizationsUnderRootOrganization();
    Task<List<OrganizationModel>> FindRedemptionOrganizationsByCustomerId(int customerId, bool checkSubscription = false);
}

public class OrganizationService : IOrganizationService
{
    private readonly ISubscriptionService _subscription;
    private readonly NexportApiService _nexportApi;
    private readonly IUserMappingService _userMapping;
    private readonly IInvoiceService _invoiceService;
    private readonly IOrderService _order;
    private readonly IStoreContext _storeContext;
    private readonly HelperService _helper;

    public OrganizationService(
        ISubscriptionService subscription,
        NexportApiService nexportApi,
        IUserMappingService userMapping,
        IInvoiceService invoiceService,
        IOrderService order,
        IStoreContext storeContext,
        HelperService helper)
    {
        _subscription = subscription;
        _nexportApi = nexportApi;
        _userMapping = userMapping;
        _invoiceService = invoiceService;
        _order = order;
        _storeContext = storeContext;
        _helper = helper;
    }

    public Task<List<OrganizationResponseItem>> FindAllOrganizations(Guid baseOrgId) => _helper.GetAll((s, p) => _nexportApi.GetOrganizations((s.Url, s.Token), baseOrgId, p));
    public Task<List<OrganizationResponseItem>> FindAllOrganizationsUnderRootOrganization() => _helper.GetAll((s, p) => _nexportApi.GetOrganizations((s.Url, s.Token), s.RootOrg, p));

    public async Task<OrganizationResponseItem?> GetOrganizationDetails(Guid orgId)
    {
        var availableOrganizations = await FindAllOrganizations(orgId);
        return availableOrganizations.SingleOrDefault(item => item.OrgId == orgId);
    }

    public async Task<List<OrganizationModel>> FindRedemptionOrganizationsByCustomerId(int customerId, bool checkSubscription = false)
    {
        var organizationModelList = new List<OrganizationModel>();
        Guid? userId = null;
        if (checkSubscription)
        {
            userId = (await _userMapping.FindByCustomerId(customerId))?.UserId;
        }
        foreach (var orderId in (await _order.SearchOrdersAsync((await _storeContext.GetCurrentStoreAsync()).Id, customerId: customerId)).Select(order => order.Id))
        {
            foreach (var orderItemId in (await _order.GetOrderItemsAsync(orderId)).Select(orderItem => orderItem.Id))
            {
                if (await _invoiceService.FindOrderInvoiceItem(orderId, orderItemId) is not { UtcDateRedemption: not null } orderInvoiceItem
                    || await _invoiceService.GetInvoiceRedemption(orderInvoiceItem.InvoiceItemId) is not { ApiErrorEntity.ErrorCode: ErrorCode.NoError, OrganizationId: var organizationId }
                    || organizationModelList.Exists(organizationModel => organizationModel.OrgId == organizationId)
                    || (await FindAllOrganizations(organizationId)).Find(organizationResponseItem => organizationResponseItem.OrgId == organizationId) is not { } org)
                {
                    continue;
                }
                var model = new OrganizationModel
                {
                    OrgId = org.OrgId,
                    OrgName = org.Name,
                    OrgShortName = org.ShortName,
                    Subscription = userId is not null ? await _subscription.FindSubscription(userId.Value, org.OrgId) : null,
                };
                organizationModelList.Add(model);
            }
        }
        return organizationModelList;
    }
}
