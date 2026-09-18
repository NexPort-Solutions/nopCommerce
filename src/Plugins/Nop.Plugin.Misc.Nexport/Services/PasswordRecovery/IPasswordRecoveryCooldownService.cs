using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.Nexport.Services.PasswordRecovery;

public interface IPasswordRecoveryCooldownService
{
    Task<PasswordRecoveryCooldownState> GetStateAsync(Customer customer, int cooldownMinutes, DateTime utcNow);

    Task RecordSuccessfulSendAsync(Customer customer, DateTime sentUtc);
}
