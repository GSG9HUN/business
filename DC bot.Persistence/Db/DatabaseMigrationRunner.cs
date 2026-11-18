using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DC_bot.Db;

/// <summary>
/// Applies pending EF migrations before application startup without taking ownership of the application&apos;s service provider.
/// </summary>
public static class DatabaseMigrationRunner
{
    private const int MaxConnectionAttempts = 8;

    /// <summary>
    /// Applies pending EF migrations in an asynchronous scope, forwarding application cancellation to database operations.
    /// </summary>
    /// <param name = "services">The service collection or provider used at the persistence composition boundary.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    public static async Task ApplyMigrationsIfNeededAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var scope = services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BotDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(DatabaseMigrationRunner));
        await WaitForDatabaseAsync(dbContext, logger, cancellationToken);
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    /// <summary>Retries transient connection failures before migration work, preserving cancellation and terminal failures.</summary>
    internal static async Task WaitForDatabaseAsync(
        BotDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxConnectionAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await dbContext.Database.OpenConnectionAsync(cancellationToken);
                await dbContext.Database.CloseConnectionAsync();
                return;
            }
            catch (Exception exception) when (exception is NpgsqlException { IsTransient: true }
                or InvalidOperationException { InnerException: NpgsqlException { IsTransient: true } }
                && attempt < MaxConnectionAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(1 << (attempt - 1), 30));
                logger?.LogWarning(exception,
                    "Database is not ready. Connection attempt {Attempt}/{MaxAttempts}; retrying after {Delay}.",
                    attempt, MaxConnectionAttempts, delay);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
