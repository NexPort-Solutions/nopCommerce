using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IUserMappingService
{
    Task DeleteUserMapping(UserMapping userMapping);
    Task<UserMapping?> FindByCustomerId(int nopCustomerId);
    Task<UserMapping?> FindUserMappingByUserId(Guid userId);
    Task UpdateUserMapping(UserMapping userMapping);
    Task<Result<Unit, UserMapping>> InsertUserMapping(UserMapping userMapping);
}

public class UserMappingService : IUserMappingService
{
    private readonly IRepository<UserMapping> _userMappings;

    public UserMappingService(IRepository<UserMapping> userMappingRepository)
    {
        _userMappings = userMappingRepository;
    }

    public async Task DeleteUserMapping(UserMapping userMapping) => await _userMappings.DeleteAsync(userMapping);
    public async Task UpdateUserMapping(UserMapping userMapping) => await _userMappings.UpdateAsync(userMapping);

    public Task<UserMapping?> FindByCustomerId(int nopCustomerId)
        => _userMappings.Table.OnlyOneOrDefault(userMapping => userMapping.NopUserId == nopCustomerId);

    public Task<UserMapping?> FindUserMappingByUserId(Guid userId)
        => _userMappings.Table.OnlyOneOrDefault(userMapping => userMapping.UserId == userId);

    public async Task<Result<Unit, UserMapping>> InsertUserMapping(UserMapping userMapping)
    {
        if (await _userMappings.Table.FirstOrDefaultAsync(user => user.UserId == userMapping.UserId) is { } existingMapping)
        {
            return Error(existingMapping);
        }
        if (await _userMappings.Table.AnyAsync(user => user.NopUserId == userMapping.NopUserId))
        {
            return Okay(Unit.Instance);
        }
        await _userMappings.InsertAsync(userMapping);
        return Okay(Unit.Instance);
    }
}
