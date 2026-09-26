using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Db;

public static class DatabaseMigrationRunner
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

        await EnsureQueueSourceMetadataColumnsAsync(dbContext);
    }

    private static Task EnsureQueueSourceMetadataColumnsAsync(BotDbContext dbContext)
    {
        return dbContext.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE guild_queue_item
                ADD COLUMN IF NOT EXISTS source_query text;

            ALTER TABLE guild_queue_item
                ADD COLUMN IF NOT EXISTS source_search_mode character varying(64);
            """);
    }
}
