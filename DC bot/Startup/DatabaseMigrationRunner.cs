using DC_bot.Persistence.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

internal static class DatabaseMigrationRunner
{
    public static async Task ApplyMigrationsIfNeededAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BotDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
}
