using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Persistence;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class RepositoryPostgreSqlIntegrationTests
{
    [Fact]
    public async Task GuildDataRepository_UpsertPremium_PersistsPremiumState()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = CreateGuildDataRepository(services);

        const ulong guildId = 9001UL;

        await repository.EnsureGuildExistsAsync(guildId);
        Assert.False(await repository.IsPremiumAsync(guildId));

        await repository.UpsertPremiumAsync(guildId, true, DateTimeOffset.UtcNow.AddDays(1));
        Assert.True(await repository.IsPremiumAsync(guildId));

        await repository.UpsertPremiumAsync(guildId, true, DateTimeOffset.UtcNow.AddDays(-1));
        Assert.False(await repository.IsPremiumAsync(guildId));
    }

    [Fact]
    public async Task PlaybackStateRepository_SetRepeatAndCurrentTrack_PersistsState()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = CreatePlaybackStateRepository(services);

        const ulong guildId = 9002UL;

        var initial = await repository.GetOrCreateAsync(guildId);
        Assert.False(initial.IsRepeating);
        Assert.False(initial.IsRepeatingList);
        Assert.Null(initial.CurrentTrackIdentifier);
        Assert.Null(initial.QueueItemId);

        await repository.SetRepeatStateAsync(guildId, true, true);
        await repository.SetCurrentTrackAsync(guildId, "track-identifier", 42L);

        var updated = await repository.GetOrCreateAsync(guildId);
        Assert.True(updated.IsRepeating);
        Assert.True(updated.IsRepeatingList);
        Assert.Equal("track-identifier", updated.CurrentTrackIdentifier);
        Assert.Equal(42L, updated.QueueItemId);
    }

    [Fact]
    public async Task RepeatListRepository_ReplaceAndClear_PersistsOrderedSnapshot()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = CreateRepeatListRepository(services);

        const ulong guildId = 9003UL;

        await repository.ReplaceAsync(guildId, ["track-b", "track-a"]);
        Assert.Equal(["track-b", "track-a"], await repository.GetTrackIdentifiersAsync(guildId));

        await repository.ReplaceAsync(guildId, ["track-c"]);
        Assert.Equal(["track-c"], await repository.GetTrackIdentifiersAsync(guildId));

        await repository.ClearAsync(guildId);
        Assert.Empty(await repository.GetTrackIdentifiersAsync(guildId));
    }

    private static GuildDataRepository CreateGuildDataRepository(ServiceProvider services)
    {
        return new GuildDataRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static PlaybackStateRepository CreatePlaybackStateRepository(ServiceProvider services)
    {
        return new PlaybackStateRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static RepeatListRepository CreateRepeatListRepository(ServiceProvider services)
    {
        return new RepeatListRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }
}
