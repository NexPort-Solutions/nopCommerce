using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ISubscriptionService
{
    Task<List<SubscriptionResponse>> FindAllSubscriptions(Guid userId);
    Task<SubscriptionResponse?> FindSubscription(Guid userId, Guid orgId);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly NexportApiService _nexportApi;
    private readonly HelperService _helper;

    public SubscriptionService(NexportApiService nexportApi, HelperService helper)
    {
        _nexportApi = nexportApi;
        _helper = helper;
    }

    public Task<SubscriptionResponse?> FindSubscription(Guid userId, Guid orgId)
        => _helper.Do(s => _nexportApi.GetSubscription((s.Url, s.Token), userId, orgId));

    public Task<List<SubscriptionResponse>> FindAllSubscriptions(Guid userId)
        => _helper.GetAll((s, p) => _nexportApi.GetSubscriptions((s.Url, s.Token), userId, p));
}
