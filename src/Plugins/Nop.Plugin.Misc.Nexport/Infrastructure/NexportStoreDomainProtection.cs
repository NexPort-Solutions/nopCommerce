using System.Collections.Frozen;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Configuration;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

// Admits authorities from store URLs using a worker-local snapshot; request evaluation never reads SQL.
// This is an admission check, not a replacement for nopCommerce store selection. Hosts entries are
// audited but cannot authorize extra domains. The singleton also runs the background refresh loop.
public sealed class NexportStoreDomainProtection : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);
    private const int MaximumExamples = 20;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NexportStoreDomainProtection> _logger;
    private readonly NexportRequestProtectionConfig _configuration;
    // Capacity one coalesces store events. A single refresh loop owns all database reads.
    private readonly SemaphoreSlim _refreshSignal = new(0, 1);
    private readonly object _observationLock = new();
    private readonly HashSet<string> _examples = new(StringComparer.OrdinalIgnoreCase);
    private readonly Meter _meter = new("Nop.Plugin.Misc.Nexport.RequestProtection");
    private readonly Counter<long> _misses;
    private readonly Counter<long> _refreshFailures;
    private readonly string _worker = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;
    private Snapshot _snapshot; // Last usable allow-list; replaced atomically, never mutated by requests.
    private Snapshot _lastAttempt; // Audit details from even an empty/invalid refresh, without enforcing it.
    private string _lastFailure;
    private long _observationCount;
    private int _databaseInstalled;
    private int _disposed;

    public NexportStoreDomainProtection(IServiceScopeFactory scopeFactory, ILogger<NexportStoreDomainProtection> logger,
        NexportRequestProtectionConfig configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        _misses = _meter.CreateCounter<long>("nexport.request_protection.store_host_misses");
        _refreshFailures = _meter.CreateCounter<long>("nexport.request_protection.store_host_refresh_failures");
        _meter.CreateObservableGauge("nexport.request_protection.store_host_snapshot_age_seconds", () =>
        {
            var snapshot = Volatile.Read(ref _snapshot);
            var age = snapshot == null ? -1 : Math.Max(0, (DateTimeOffset.UtcNow - snapshot.LoadedUtc).TotalSeconds);
            return new Measurement<double>(age, new KeyValuePair<string, object>("worker", _worker));
        });
        _meter.CreateObservableGauge("nexport.request_protection.store_host_snapshot_available", () =>
            new Measurement<int>(Volatile.Read(ref _snapshot) == null ? 0 : 1,
                new KeyValuePair<string, object>("worker", _worker)));
    }

    private bool Enabled => _configuration.Enabled &&
        _configuration.StoreHostGuardMode != NexportStoreHostGuardMode.Disabled;

    // Checks Request.Host exactly, including any port. Observe records misses but permits them.
    // Before the first usable snapshot (or while uninstalled), requests deliberately fail open.
    public bool ShouldReject(HttpContext context)
    {
        var snapshot = Volatile.Read(ref _snapshot);
        if (!Enabled || Volatile.Read(ref _databaseInstalled) == 0 || snapshot == null)
            return false;

        var authority = context.Request.Host.Value;
        if (snapshot.Hosts.Contains(authority))
            return false;

        // Host examples are useful during rollout only. Enforcement uses counters, so hostile traffic
        // cannot generate recurring log summaries or contend on the observation lock.
        if (_configuration.StoreHostGuardMode == NexportStoreHostGuardMode.Observe)
        {
            lock (_observationLock)
            {
                _observationCount++;
                if (_examples.Count < MaximumExamples && !_examples.Contains(authority))
                    _examples.Add(IsValidAuthority(authority) ? SanitizeExample(authority) : "<invalid-authority>");
            }
        }

        _misses.Add(1,
            new KeyValuePair<string, object>("mode", _configuration.StoreHostGuardMode.ToString()),
            new KeyValuePair<string, object>("worker", _worker));
        return _configuration.StoreHostGuardMode == NexportStoreHostGuardMode.Enforce;
    }

    // Signals a local refresh without reading stores or waiting for SQL on the caller's thread.
    // Other workers discover the change through their periodic refresh.
    public void RequestRefresh()
    {
        if (!Enabled || Volatile.Read(ref _disposed) != 0)
            return;

        try
        {
            _refreshSignal.Release();
        }
        catch (SemaphoreFullException)
        {
            // A refresh is already pending.
        }
        catch (ObjectDisposedException)
        {
            // Store events can race application shutdown.
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Enabled)
            return;

        // Do not block host startup on a database read in .NET 8 BackgroundService.StartAsync.
        await Task.Yield();
        await Task.WhenAll(RefreshLoopAsync(stoppingToken), ReportLoopAsync(stoppingToken));
    }

    private async Task RefreshLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var succeeded = await RefreshAsync(cancellationToken);
            if (succeeded)
                await _refreshSignal.WaitAsync(RefreshInterval, cancellationToken);
            else
                // Events cannot create a retry storm while the database is unavailable.
                await Task.Delay(RefreshInterval, cancellationToken);
        }
    }

    // Builds a complete replacement off the request path. Failure retains the last good snapshot;
    // an empty result must not accidentally deny every configured store.
    private async Task<bool> RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var installed = DataSettingsManager.IsDatabaseInstalled();
            Volatile.Write(ref _databaseInstalled, installed ? 1 : 0);
            if (!installed)
            {
                Volatile.Write(ref _snapshot, null);
                Volatile.Write(ref _lastAttempt, null);
                Volatile.Write(ref _lastFailure, null);
                return true;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            // This service bypasses the shared store cache. Its API has no cancellation-token overload,
            // so keep the scope alive until the read finishes and check shutdown before publishing.
            var stores = await scope.ServiceProvider.GetRequiredService<IStoreService>().GetAllStoresAsync();
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = BuildSnapshot(stores);
            Volatile.Write(ref _lastAttempt, snapshot);
            if (snapshot.Hosts.Count == 0)
                throw new InvalidOperationException("No usable store authorities.");

            Volatile.Write(ref _snapshot, snapshot);
            Volatile.Write(ref _lastFailure, null);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Only the exception type is recorded: SQL exceptions can contain connection or record details.
            Volatile.Write(ref _lastFailure, exception.GetType().Name);
            _refreshFailures.Add(1, new KeyValuePair<string, object>("worker", _worker));
            return false;
        }
    }

    private static Snapshot BuildSnapshot(IEnumerable<Store> stores)
    {
        var hosts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var audit = new HashSet<string>();
        var invalid = 0;
        var discrepancies = 0;
        var duplicates = 0;

        foreach (var store in stores.Where(store => !store.Deleted))
        {
            // Hosts is audited for nopCommerce routing consistency, never used to expand admission.
            var configuredHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in (store.Hosts ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var configuredHost = entry.Trim();
                if (IsValidAuthority(configuredHost))
                    configuredHosts.Add(configuredHost);
                else
                {
                    invalid++;
                    AddAudit($"invalid-host:store-{store.Id}");
                }
            }

            var authority = GetUrlAuthority(store.Url);
            if (authority == null)
            {
                invalid++;
                AddAudit($"invalid-url:store-{store.Id}");
            }
            else
            {
                if (configuredHosts.Count != 1 || !configuredHosts.Contains(authority))
                {
                    discrepancies++;
                    AddAudit($"url-hosts-mismatch:store-{store.Id}");
                }
                if (hosts.TryGetValue(authority, out var owner) && owner != store.Id)
                {
                    duplicates++;
                    AddAudit($"duplicate:stores-{owner}-{store.Id}");
                }
                else
                    hosts[authority] = store.Id;
            }
        }

        return new Snapshot(hosts.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase), DateTimeOffset.UtcNow,
            invalid, discrepancies, duplicates, string.Join(", ", audit.OrderBy(value => value, StringComparer.Ordinal)));

        void AddAudit(string message)
        {
            if (audit.Count < MaximumExamples)
                audit.Add(message);
        }
    }

    private static string GetUrlAuthority(string url)
    {
        if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return null;

        // Read the original authority so Uri normalization cannot discard an explicit default port or trailing dot.
        var start = url.IndexOf("://", StringComparison.Ordinal);
        if (start < 0)
            return null;
        start += 3;
        var end = url.IndexOfAny(['/', '?', '#'], start);
        var authority = end < 0 ? url[start..] : url[start..end];
        return IsValidAuthority(authority) ? authority : null;
    }

    // Accepts a literal DNS/IP authority with an optional numeric port, never a URL or wildcard.
    // Validation intentionally does not canonicalize: normalizing here could admit a host that
    // nopCommerce's exact Hosts matching would send to its first-store fallback.
    private static bool IsValidAuthority(string authority)
    {
        if (string.IsNullOrEmpty(authority) || authority.Length > 320 ||
            authority.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)) ||
            authority.IndexOfAny(['/', '\\', '?', '#', '@', '%', '*']) >= 0)
            return false;

        string port = null;
        if (authority.StartsWith('['))
        {
            var end = authority.IndexOf(']');
            if (end < 0 || !IPAddress.TryParse(authority[1..end], out var address) ||
                address.AddressFamily != AddressFamily.InterNetworkV6)
                return false;
            if (end + 1 < authority.Length)
            {
                if (authority[end + 1] != ':')
                    return false;
                port = authority[(end + 2)..];
            }
        }
        else
        {
            var separator = authority.IndexOf(':');
            var host = separator < 0 ? authority : authority[..separator];
            if (separator >= 0)
                port = authority[(separator + 1)..];
            if (host.Length > 253 || Uri.CheckHostName(host) == UriHostNameType.Unknown)
                return false;
        }

        return port == null || (port.Length is > 0 and <= 5 && port.All(char.IsAsciiDigit) &&
            int.TryParse(port, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number is > 0 and <= 65535);
    }

    private static string SanitizeExample(string authority)
    {
        return string.Concat(authority.Select(character =>
            char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_' or ':' or '[' or ']'
                ? character : '_'));
    }

    private async Task ReportLoopAsync(CancellationToken cancellationToken)
    {
        // Observe reports traffic at most once a minute. Enforce reports only diagnostic state changes;
        // rejection volume and snapshot age remain available as metrics in both modes.
        (string State, string Failure, int Invalid, int Mismatches, int Duplicates, string Audit)? previous = null;
        using var timer = new PeriodicTimer(RefreshInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            long count;
            string examples;
            lock (_observationLock)
            {
                count = _observationCount;
                examples = string.Join(", ", _examples);
                _observationCount = 0;
                _examples.Clear();
            }

            var snapshot = Volatile.Read(ref _snapshot);
            var audit = Volatile.Read(ref _lastAttempt);
            var failure = Volatile.Read(ref _lastFailure);
            var state = GetSnapshotState(snapshot, failure);
            var diagnostics = (state, failure, audit?.InvalidEntries ?? 0, audit?.UrlAliasMismatches ?? 0,
                audit?.DuplicateAssignments ?? 0, audit?.Audit ?? string.Empty);
            if (count == 0 && previous == diagnostics)
                continue;
            previous = diagnostics;

            var age = snapshot == null ? -1 : Math.Max(0, (DateTimeOffset.UtcNow - snapshot.LoadedUtc).TotalSeconds);
            _logger.Log(state is "unavailable" or "stale" ? LogLevel.Warning : LogLevel.Information,
                "Store domain protection {Mode} on {Worker}: state {State}, snapshot age {AgeSeconds}s, " +
                "observed misses {Misses}, examples [{Examples}], invalid entries {Invalid}, URL/Hosts mismatches {Mismatches}, " +
                "duplicate assignments {Duplicates}, audit [{Audit}], refresh failure {Failure}",
                _configuration.StoreHostGuardMode, _worker, state, age, count, examples,
                audit?.InvalidEntries ?? 0, audit?.UrlAliasMismatches ?? 0,
                audit?.DuplicateAssignments ?? 0, audit?.Audit ?? string.Empty, failure ?? "none");
        }
    }

    private string GetSnapshotState(Snapshot snapshot, string failure)
    {
        if (failure != null)
            return snapshot == null ? "unavailable" : "stale";
        if (Volatile.Read(ref _databaseInstalled) == 0)
            return "uninstalled";
        return snapshot == null ? "unavailable" : "ready";
    }

    public override void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        base.Dispose();
        _refreshSignal.Dispose();
        _meter.Dispose();
    }

    private sealed record Snapshot(FrozenSet<string> Hosts, DateTimeOffset LoadedUtc, int InvalidEntries,
        int UrlAliasMismatches, int DuplicateAssignments, string Audit);
}
