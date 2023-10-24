using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;

namespace Nop.Plugin.Misc.Nexport.Extensions;

public static class Extensions
{
#pragma warning disable
    private static readonly Regex _emailRegex = new("^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250.0));
#pragma warning restore

    public static (IEnumerable<T1>, IEnumerable<T2>) Unzip<T1, T2>(
        this IEnumerable<(T1, T2)> source,
        Func<(T1, T2), T1> f1,
        Func<(T1, T2), T2> f2
    ) => (source.Select(f1), source.Select(f2));

    public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
    {
        var values = new NameValueCollection();
        foreach (var pair in collection)
        {
            values.Add(pair.Key, pair.Value);
        }
        return values;
    }

    public static string? GetDisplayName<TEnum>(this TEnum enumValue)
        where TEnum : struct, Enum
        => enumValue.ToString() is { } name
            && enumValue.GetType().GetMember(name).First().GetCustomAttribute<DisplayAttribute>() is { } displayAttribute
            ? displayAttribute.GetName() : null;

    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        try
        {
            email = Regex.Replace(email, "(@)(.+)$", domainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200.0));
            return _emailRegex.IsMatch(email);
        }
        catch (Exception exception) when (exception is RegexMatchTimeoutException or ArgumentException)
        {
            return false;
        }

        static string domainMapper(Match match)
        {
            var ascii = new IdnMapping().GetAscii(match.Groups[2].Value);
            return match.Groups[1].Value + ascii;
        }
    }

    public static bool IsValidUrl(this string url) => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && uriResult is not null;

    public static bool IsValidDateFormat(this string dateStr, string format)
        => DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    public static string TruncateAtWord(this string? input, int length)
    {
        if (input is null || input.Length < length)
        {
            return string.Empty;
        }
        var nextSpaceIndex = input.LastIndexOf(' ', length);
        var index = nextSpaceIndex is not -1 ? nextSpaceIndex : length;
        return index == input.Length ? input : $"{input[..index].Trim()}…";
    }

    public static SelectList ToSelectList<TEnum>(this TEnum enumObject, bool markCurrentAsSelected = true, int[]? valuesToExclude = null)
        where TEnum : struct, Enum
    {
        var values = Enum.GetValues(typeof(TEnum))
            .OfType<TEnum>()
            .Select(enumValue => (enumValue, intValue: Convert.ToInt32(enumValue, CultureInfo.InvariantCulture)));
        if (valuesToExclude is not null)
        {
            values = values.Where(value => !valuesToExclude.Contains(value.intValue));
        }
        var selectList = values.Select(value => new Item(value.intValue, GetDisplayName(value.enumValue)));
        return new(selectList, nameof(Item.Id), nameof(Item.Name), markCurrentAsSelected ? Convert.ToInt32(enumObject, CultureInfo.InvariantCulture) : null);
    }

    public static Task<TSource?> OnlyOneOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>>? predicate = null)
        => predicate is null ? AsyncExtensions.SingleOrDefaultAsync(source) : AsyncExtensions.SingleOrDefaultAsync(source, predicate);

    public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T?> source)
        where T : class
        => source.Where(item => item is not null)!; // C#'s static analysis is not strong enough for this, but I think you can see that this is safe.

    public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T?> source)
        where T : struct
        => source.Where(item => item is not null).Select(item => item!.Value);

    public static IEnumerable<T2> FilterMap<T1, T2>(this IEnumerable<T1> source, Func<T1, T2?> f)
        => source.Select(f).Where(item => item is not null)!; // C#'s static analysis is not strong enough for this, but I think you can see that this is safe.

    public static IEnumerable<T2> FilterMap<T1, T2>(this IEnumerable<T1> source, Func<T1, T2?> f)
        where T2 : struct => source.FilterMap(i => f(i) ?? default);
}

internal record Item(int Id, string? Name);
