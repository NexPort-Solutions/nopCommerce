namespace Nop.Plugin.Misc.Nexport.Extensions;

// Require a token so tokenless calls continue using nopCommerce's existing query extensions.
public static class NexportQueryExtensions
{
    public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken)
        => LinqToDB.AsyncExtensions.SingleOrDefaultAsync(query, cancellationToken);

    public static Task<List<T>> ToListAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken)
        => LinqToDB.AsyncExtensions.ToListAsync(query, cancellationToken);
}