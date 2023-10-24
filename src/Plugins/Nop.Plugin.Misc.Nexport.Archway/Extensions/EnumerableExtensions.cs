namespace Nop.Plugin.Misc.Nexport.Archway.Extensions;

public static class EnumerableExtensions
{
    public static async IAsyncEnumerable<TResult> FilterMapAwaitAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<TResult?>> selector)
    {
        foreach (var item in source)
        {
            if (await selector(item) is TResult result)
            {
                yield return result;
            }
        }
    }
}
