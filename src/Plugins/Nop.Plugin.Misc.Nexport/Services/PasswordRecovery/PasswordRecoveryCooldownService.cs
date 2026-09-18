using Nop.Core.Domain.Customers;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.Nexport.Services.PasswordRecovery;

public sealed class PasswordRecoveryCooldownService : IPasswordRecoveryCooldownService
{
    private readonly IGenericAttributeService _genericAttributeService;

    public PasswordRecoveryCooldownService(IGenericAttributeService genericAttributeService)
    {
        _genericAttributeService = genericAttributeService;
    }

    public async Task<PasswordRecoveryCooldownState> GetStateAsync(Customer customer, int cooldownMinutes, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(customer);

        if (cooldownMinutes is <= 0 or > 60)
            return PasswordRecoveryCooldownState.Inactive;

        var sentUtc = await _genericAttributeService.GetAttributeAsync<DateTime?>(
            customer, NexportDefaults.PasswordRecoveryCooldownSentUtcAttribute);

        if (!sentUtc.HasValue)
            return PasswordRecoveryCooldownState.Inactive;

        var expiresUtc = sentUtc.Value.AddMinutes(cooldownMinutes);
        if (expiresUtc <= utcNow)
            return PasswordRecoveryCooldownState.Inactive;

        var remainingSeconds = Math.Max(1, (int)Math.Ceiling((expiresUtc - utcNow).TotalSeconds));
        return new PasswordRecoveryCooldownState(true, expiresUtc, remainingSeconds);
    }

    public async Task RecordSuccessfulSendAsync(Customer customer, DateTime sentUtc)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await _genericAttributeService.SaveAttributeAsync(
            customer, NexportDefaults.PasswordRecoveryCooldownSentUtcAttribute, sentUtc);
    }
}
