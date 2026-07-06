using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Persistence;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class QueueRepositoryPostgreSqlIntegrationTests
{
    [Fact]
    public async Task ClaimNextQueuedItemAsync_ClaimsLowestQueuedItemAndMarksItPlaying()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = new QueueRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());

        var first = await repository.EnqueueAsync(42ul, "track-a");
        await repository.EnqueueAsync(42ul, "track-b");

        var claimed = await repository.ClaimNextQueuedItemAsync(42ul);

        Assert.NotNull(claimed);
        Assert.Equal(first.Id, claimed.Id);
        Assert.Equal(QueueItemState.Playing, claimed.State);

        var remaining = await repository.GetQueuedItemsAsync(42ul);
        Assert.Single(remaining);
        Assert.Equal("track-b", remaining[0].TrackIdentifier);
    }

    [Fact]
    public async Task ClaimNextQueuedItemAsync_WhenCalledConcurrently_DoesNotClaimSameQueuedItemTwice()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = new QueueRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());

        var expectedIds = new List<long>();
        for (var index = 0; index < 10; index++)
        {
            var item = await repository.EnqueueAsync(84ul, $"track-{index}");
            expectedIds.Add(item.Id);
        }

        var claims = await Task.WhenAll(Enumerable.Range(0, expectedIds.Count)
            .Select(_ => repository.ClaimNextQueuedItemAsync(84ul)));

        var claimedIds = claims
            .Where(item => item is not null)
            .Select(item => item!.Id)
            .OrderBy(id => id)
            .ToArray();

        Assert.Equal(expectedIds.OrderBy(id => id), claimedIds);
        Assert.Equal(claimedIds.Length, claimedIds.Distinct().Count());
        Assert.Empty(await repository.GetQueuedItemsAsync(84ul));
    }

    [Fact]
    public async Task ClaimNextQueuedItemAsync_WithMoreParallelConsumersThanItems_ClaimsEachItemOnce()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var seedRepository = new QueueRepository(factory);
        const ulong guildId = 168ul;
        const int queuedItemCount = 5;
        const int consumerCount = 12;
        var expectedTrackIdentifiers = new List<string>();

        for (var index = 0; index < queuedItemCount; index++)
        {
            var trackIdentifier = $"track-{index}";
            await seedRepository.EnqueueAsync(guildId, trackIdentifier);
            expectedTrackIdentifiers.Add(trackIdentifier);
        }

        var claims = await Task.WhenAll(Enumerable.Range(0, consumerCount)
            .Select(_ => new QueueRepository(factory).ClaimNextQueuedItemAsync(guildId)));
        var claimed = claims.Where(item => item is not null).Select(item => item!).ToArray();

        Assert.Equal(queuedItemCount, claimed.Length);
        Assert.Equal(consumerCount - queuedItemCount, claims.Count(item => item is null));
        Assert.Equal(claimed.Length, claimed.Select(item => item.Id).Distinct().Count());
        Assert.Equal(
            expectedTrackIdentifiers.OrderBy(identifier => identifier),
            claimed.Select(item => item.TrackIdentifier).OrderBy(identifier => identifier));
        Assert.Empty(await seedRepository.GetQueuedItemsAsync(guildId));
    }

    [Fact]
    public async Task ReorderQueuedItemsAsync_WithPostgreSqlUniquePositionIndex_ReordersWithoutConstraintCollision()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = new QueueRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());

        await repository.EnqueueAsync(126ul, "track-a");
        await repository.EnqueueAsync(126ul, "track-b");
        await repository.EnqueueAsync(126ul, "track-c");

        await repository.ReorderQueuedItemsAsync(126ul, ["track-c", "track-a", "track-b"]);

        var reordered = await repository.GetQueuedItemsAsync(126ul);
        Assert.Equal(["track-c", "track-a", "track-b"], reordered.Select(item => item.TrackIdentifier));
        Assert.Equal([0, 1, 2], reordered.Select(item => item.Position));
    }
}
