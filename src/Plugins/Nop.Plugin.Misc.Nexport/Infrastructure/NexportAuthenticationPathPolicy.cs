using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportAuthenticationPathPolicy
{
    private readonly HashSet<string> _paths;

    public NexportAuthenticationPathPolicy(IEnumerable<string> paths, bool allowEmpty = false)
    {
        ArgumentNullException.ThrowIfNull(paths);

        _paths = paths
            .Select(NormalizePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowEmpty && _paths.Count == 0)
            throw new InvalidOperationException("Authentication page paths cannot be empty.");
    }

    public bool Contains(PathString requestPath)
    {
        var path = requestPath.Value;
        if (string.IsNullOrEmpty(path))
            return false;

        if (path.EndsWith('/'))
            path = path[..^1];

        return _paths.Contains(path);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Authentication page paths cannot be empty.");

        var normalizedPath = path.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(normalizedPath) || normalizedPath == "/")
            throw new InvalidOperationException("The root path cannot be added to authentication page paths.");

        if (normalizedPath[0] != '/')
            throw new InvalidOperationException($"Authentication page path '{path}' must start with '/'.");

        if (normalizedPath.Contains('?') || normalizedPath.Contains('#') || normalizedPath.Contains(',') ||
            normalizedPath.Contains('*') || normalizedPath.Contains('\\'))
        {
            throw new InvalidOperationException(
                $"Authentication page path '{path}' cannot contain commas, queries, fragments, wildcards, or backslashes.");
        }

        return normalizedPath;
    }
}
