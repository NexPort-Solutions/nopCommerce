using System.Data.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

/// <summary>
/// Checks whether the primary database connection can be opened.
/// </summary>
public sealed class NexportDatabaseHealthCheck : IHealthCheck
{
    private readonly Func<DbConnection> _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NexportDatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="connectionFactory">Factory for the primary database connection.</param>
    public NexportDatabaseHealthCheck(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Attempts to acquire and open the primary database connection.
    /// </summary>
    /// <param name="context">Context for the health check.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        DbConnection connection = null;
        try
        {
            connection = _connectionFactory();
            if (connection is null)
                return new HealthCheckResult(context.Registration.FailureStatus);

            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus);
        }
        finally
        {
            try
            {
                connection?.Dispose();
            }
            catch (Exception)
            {
                // A failed health check must not expose disposal details to the caller.
            }
        }
    }
}
