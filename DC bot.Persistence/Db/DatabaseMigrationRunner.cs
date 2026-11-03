using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Db;

/// <summary>
/// Applies pending EF migrations before application startup without taking ownership of the application&apos;s service provider.
/// </summary>
public static class DatabaseMigrationRunner
{

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
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
