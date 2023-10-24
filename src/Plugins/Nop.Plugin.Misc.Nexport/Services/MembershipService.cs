using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IMembershipService
{
    Task<List<MemberShipInfo>> AddMemberships(Guid userId, IList<Guid> groupIds);
    Task<List<RemovedMembershipInfo>> RemoveMemberships(IList<Guid> membershipIds);
}

public class MembershipService : IMembershipService
{
    private readonly NexportApiService _nexportApi;
    private readonly HelperService _helper;

    public MembershipService(NexportApiService nexportApi, HelperService helper)
    {
        _nexportApi = nexportApi;
        _helper = helper;
    }

    public Task<List<MemberShipInfo>> AddMemberships(Guid userId, IList<Guid> groupIds)
        => _helper.Do(async s => (await _nexportApi.CreateMemberships((s.Url, s.Token), userId, groupIds))?.Memberships, new());

    public Task<List<RemovedMembershipInfo>> RemoveMemberships(IList<Guid> membershipIds)
        => _helper.Do(async s => (await _nexportApi.RemoveMemberships((s.Url, s.Token), membershipIds))?.Memberships, new());
}
