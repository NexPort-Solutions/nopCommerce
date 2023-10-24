using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

#pragma warning disable IDE1006, CA1715, CA1040 // Naming Styles, empty interfaces

public interface Result<in TOkay, in TError> { }
public interface Result<in TError> : Result<object, TError> { }
public record Ok<TOkay>(TOkay Okay) : Result<TOkay, object>;
public record Ok : Result<object>;
public record Err<TError>(TError Error) : Result<TError>;

public static class Result
{
    [MustUseReturnValue]
    public static Ok<TOkay> Okay<TOkay>(TOkay okay) => new(okay);

    [MustUseReturnValue]
    public static Ok Okay() => new();

    [MustUseReturnValue]
    public static Err<TError> Error<TError>(TError error) => new(error);

    [DoesNotReturn]
    public static IActionResult Absurd() => throw new UnreachableException();
}

public record class Unit
{
    public static Unit Instance { get; } = new();
    private Unit() { }
}
