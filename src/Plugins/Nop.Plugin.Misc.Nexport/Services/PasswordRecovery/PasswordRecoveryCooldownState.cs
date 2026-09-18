namespace Nop.Plugin.Misc.Nexport.Services.PasswordRecovery;

public sealed record PasswordRecoveryCooldownState(bool IsActive, DateTime? ExpiresUtc, int RemainingSeconds)
{
    public static PasswordRecoveryCooldownState Inactive => new(false, null, 0);
}
