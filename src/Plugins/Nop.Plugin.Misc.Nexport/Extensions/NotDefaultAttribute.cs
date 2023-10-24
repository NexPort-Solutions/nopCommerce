using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Extensions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotDefaultAttribute : ValidationAttribute
{
    public const string DEFAULT_ERROR_MESSAGE = "Property {0} must have a valid value.";

    public NotDefaultAttribute()
        : base(DEFAULT_ERROR_MESSAGE) { }

    public override bool IsValid(object? value)
        => value is ValueType && !value.Equals(Activator.CreateInstance(value.GetType()));
}
