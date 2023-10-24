using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Nexport.Extensions;

public static class GetRecord
{
    #region OrThrow
    public static async Task<T> OrThrow<T, TId>(
        Func<TId, Task<T?>> getter,
        [NotNull] TId? id,
        [CallerArgumentExpression(nameof(id))] string? idName = "UnknownExpression")
        where TId : struct
        => await getter(id ?? throw new($"GroupGuid {idName} not provided,")) ?? throw new($"{nameof(T)} not found.");

    public static async Task<T> OrThrow<T, TId>(
        Func<TId, TId, Task<T?>> getter,
        [NotNull] TId? id1,
        [NotNull] TId? id2,
        [CallerArgumentExpression(nameof(id1))] string? id1Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id2))] string? id2Name = "UnknownExpression")
        where TId : struct
        => await getter(
            id1 ?? throw new($"GroupGuid {id1Name} not provided,"),
            id2 ?? throw new($"GroupGuid {id2Name} not provided,"))
            ?? throw new($"{nameof(T)} not found.");

    public static async Task<T> OrThrow<T, TId>(
        Func<TId, TId, TId, Task<T?>> getter,
        [NotNull] TId? id1,
        [NotNull] TId? id2,
        [NotNull] TId? id3,
        [CallerArgumentExpression(nameof(id1))] string? id1Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id2))] string? id2Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id3))] string? id3Name = "UnknownExpression")
        where TId : struct
        => await getter(
            id1 ?? throw new($"GroupGuid {id1Name} not provided,"),
            id2 ?? throw new($"GroupGuid {id2Name} not provided,"),
            id3 ?? throw new($"GroupGuid {id3Name} not provided,"))
            ?? throw new($"{nameof(T)} not found.");

    public static async Task<T> OrThrow<T, TId>(
        Func<TId, TId, TId, TId, Task<T?>> getter,
        [NotNull] TId? id1,
        [NotNull] TId? id2,
        [NotNull] TId? id3,
        [NotNull] TId? id4,
        [CallerArgumentExpression(nameof(id1))] string? id1Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id2))] string? id2Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id3))] string? id3Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id3))] string? id4Name = "UnknownExpression")
        where TId : struct
        => await getter(
            id1 ?? throw new($"GroupGuid {id1Name} not provided,"),
            id2 ?? throw new($"GroupGuid {id2Name} not provided,"),
            id3 ?? throw new($"GroupGuid {id3Name} not provided,"),
            id4 ?? throw new($"GroupGuid {id4Name} not provided,"))
            ?? throw new($"{nameof(T)} not found.");

    #endregion OrThrow
    #region OrDefault

    public static Task<T?> OrDefault<T, TId>(
        Func<TId, Task<T?>> getter,
        [NotNull] TId? id,
        [CallerArgumentExpression(nameof(id))] string? idName = "UnknownExpression")
        where TId : struct
        => getter(id ?? throw new ArgumentNullException($"GroupGuid {idName} not provided,"));

    public static T OrDefault<T, TId>(
        Func<TId, TId, T> getter,
        [NotNull] TId? id1,
        [NotNull] TId? id2,
        [CallerArgumentExpression(nameof(id1))] string? id1Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id2))] string? id2Name = "UnknownExpression")
        where TId : struct
        => getter(
            id1 ?? throw new ArgumentNullException($"GroupGuid {id1Name} not provided,"),
            id2 ?? throw new ArgumentNullException($"GroupGuid {id2Name} not provided,"));

    public static T OrDefault<T, TId>(
        Func<TId, TId, TId, T> getter,
        [NotNull] TId? id1,
        [NotNull] TId? id2,
        [NotNull] TId? id3,
        [CallerArgumentExpression(nameof(id1))] string? id1Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id2))] string? id2Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id3))] string? id3Name = "UnknownExpression")
        where TId : struct
        => getter(
            id1 ?? throw new ArgumentNullException($"GroupGuid {id1Name} not provided,"),
            id2 ?? throw new ArgumentNullException($"GroupGuid {id2Name} not provided,"),
            id3 ?? throw new ArgumentNullException($"GroupGuid {id3Name} not provided,"));

    public static T OrDefault<T, TId>(
        Func<TId, TId, TId, TId, T> getter,
        [NotNull] TId? id1,
        [NotNull] TId? id2,
        [NotNull] TId? id3,
        [NotNull] TId? id4,
        [CallerArgumentExpression(nameof(id1))] string? id1Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id2))] string? id2Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id3))] string? id3Name = "UnknownExpression",
        [CallerArgumentExpression(nameof(id3))] string? id4Name = "UnknownExpression")
        where TId : struct
        => getter(
            id1 ?? throw new ArgumentNullException($"GroupGuid {id1Name} not provided,"),
            id2 ?? throw new ArgumentNullException($"GroupGuid {id2Name} not provided,"),
            id3 ?? throw new ArgumentNullException($"GroupGuid {id3Name} not provided,"),
            id4 ?? throw new ArgumentNullException($"GroupGuid {id4Name} not provided,"));
    #endregion OrDefault
}

public static class ViewUtilities
{
    public static string GetControllerName<TController>()
        where TController : Controller
        => typeof(TController).Name[..typeof(TController).Name.LastIndexOf(nameof(Controller), StringComparison.OrdinalIgnoreCase)];
}

public record JQueryObject<T>(
    [property: JsonProperty("label")] string Label,
    [property: JsonProperty("value")] T Value);
