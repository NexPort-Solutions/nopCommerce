using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportRequestPathPolicy
{
    private readonly string[] _blockedFileExtensions;
    private readonly string[] _blockedRootPathPrefixes;

    public NexportRequestPathPolicy(
        IEnumerable<string> blockedRootPathPrefixes,
        IEnumerable<string> blockedFileExtensions)
    {
        ArgumentNullException.ThrowIfNull(blockedRootPathPrefixes);
        ArgumentNullException.ThrowIfNull(blockedFileExtensions);

        _blockedRootPathPrefixes = blockedRootPathPrefixes
            .Select(NormalizeRootPathPrefix)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _blockedFileExtensions = blockedFileExtensions
            .Select(NormalizeFileExtension)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public bool ShouldBlock(PathString requestPath)
    {
        var path = requestPath.Value;
        if (string.IsNullOrEmpty(path))
            return false;

        if (ContainsEnvironmentFileSegment(path) || ContainsBlockedFileExtension(path))
            return true;

        foreach (var prefix in _blockedRootPathPrefixes)
        {
            if (IsPathOrDescendant(path, prefix))
                return true;
        }

        return false;
    }

    private static string NormalizeFileExtension(string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
            throw new InvalidOperationException("Blocked request file extensions cannot be empty.");

        var normalizedExtension = fileExtension.Trim();
        if (normalizedExtension[0] != '.')
            normalizedExtension = $".{normalizedExtension}";

        if (normalizedExtension.Length == 1 || normalizedExtension.Skip(1).Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character != '.' && character != '-' && character != '_'))
        {
            throw new InvalidOperationException(
                $"Blocked request file extension '{fileExtension}' can contain only letters, digits, dots, hyphens, or underscores.");
        }

        return normalizedExtension;
    }

    private static string NormalizeRootPathPrefix(string pathPrefix)
    {
        if (string.IsNullOrWhiteSpace(pathPrefix))
            throw new InvalidOperationException("Additional blocked request path prefixes cannot be empty.");

        var normalizedPrefix = pathPrefix.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(normalizedPrefix) || normalizedPrefix == "/")
            throw new InvalidOperationException("The root path cannot be added to the blocked request path prefixes.");

        if (normalizedPrefix[0] != '/')
            throw new InvalidOperationException($"Blocked request path prefix '{pathPrefix}' must start with '/'.");

        if (normalizedPrefix.Contains('?') || normalizedPrefix.Contains('#') || normalizedPrefix.Contains(',') ||
            normalizedPrefix.Contains('*') || normalizedPrefix.Contains('\\'))
        {
            throw new InvalidOperationException(
                $"Blocked request path prefix '{pathPrefix}' cannot contain commas, queries, fragments, wildcards, or backslashes.");
        }

        return normalizedPrefix;
    }

    private static bool ContainsEnvironmentFileSegment(string path)
    {
        var pathSpan = path.AsSpan();
        var segmentStart = 0;

        while (segmentStart < pathSpan.Length)
        {
            while (segmentStart < pathSpan.Length && pathSpan[segmentStart] == '/')
                segmentStart++;

            if (segmentStart == pathSpan.Length)
                break;

            var separatorIndex = pathSpan[segmentStart..].IndexOf('/');
            var segment = separatorIndex < 0
                ? pathSpan[segmentStart..]
                : pathSpan.Slice(segmentStart, separatorIndex);

            if (segment.Equals(".env".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                segment.StartsWith(".env.".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;

            if (separatorIndex < 0)
                break;

            segmentStart += separatorIndex + 1;
        }

        return false;
    }

    private bool ContainsBlockedFileExtension(string path)
    {
        foreach (var extension in _blockedFileExtensions)
        {
            var searchIndex = 0;

            while (searchIndex < path.Length)
            {
                var extensionIndex = path.IndexOf(extension, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (extensionIndex < 0)
                    break;

                var suffixIndex = extensionIndex + extension.Length;
                if (suffixIndex == path.Length || path[suffixIndex] == '/')
                    return true;

                searchIndex = suffixIndex;
            }
        }

        return false;
    }

    private static bool IsPathOrDescendant(string path, string candidate)
    {
        if (!path.StartsWith(candidate, StringComparison.OrdinalIgnoreCase))
            return false;

        return path.Length == candidate.Length || path[candidate.Length] == '/';
    }
}