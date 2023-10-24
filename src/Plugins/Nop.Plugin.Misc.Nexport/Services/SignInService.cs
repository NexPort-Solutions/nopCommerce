using NexportApi.Model;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ISignInService
{
    Task<string?> SignInAsync(Guid orgId, Guid userId);
    Task<string?> SignInClassroomAsync(Guid enrollmentId);
}

public class SignInService : ISignInService
{
    private readonly IStoreContext _storeContext;
    private readonly NexportApiService _nexportApi;
    private readonly HelperService _helper;

    public SignInService(IStoreContext storeContext, NexportApiService nexportApi, HelperService helper)
    {
        _storeContext = storeContext;
        _nexportApi = nexportApi;
        _helper = helper;
    }

    public Task<string?> SignInAsync(Guid orgId, Guid userId)
        => _helper.Do(async s => await Url(_nexportApi.SingleSignOn((s.Url, s.Token), orgId, userId, (await _storeContext.GetCurrentStoreAsync()).Url)));

    public Task<string?> SignInClassroomAsync(Guid enrollmentId)
        => _helper.Do(async s => await Url(_nexportApi.ClassroomSingleSignOn((s.Url, s.Token), enrollmentId, (await _storeContext.GetCurrentStoreAsync()).Url)));

    private static async Task<string?> Url(Task<SsoResponse?> response)
        => await response is { ApiErrorEntity.ErrorCode: ApiErrorEntity.ErrorCodeEnum.NoError, Url: var url } ? url : null;
}
