namespace Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;

public record ModelStateTransferValue(string Key, string? AttemptedValue, object? RawValue, ICollection<string> ErrorMessages);
