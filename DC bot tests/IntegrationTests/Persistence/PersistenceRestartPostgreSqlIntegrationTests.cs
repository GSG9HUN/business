using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Persistence;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class PersistenceRestartPostgreSqlIntegrationTests
{
    [Fact]
    public async Task PlaybackQueueAndRepeatState_SurviveRepositoryRecreation()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();

        const ulong guildId = 20260522ul;
        long currentQueueItemId;

        await using (var firstProvider = database.CreateServiceProvider())
        {
            var factory = firstProvider.GetRequiredService<IDbContextFactory<BotDbContext>>();
            var queueRepository = new QueueRepository(factory);
            var playbackStateRepository = new PlaybackStateRepository(factory);
            var repeatListRepository = new RepeatListRepository(factory);

            var current = await queueRepository.EnqueueAsync(guildId, "current-track");
            await queueRepository.EnqueueAsync(guildId, "queued-track");
            await playbackStateRepository.SetCurrentTrackAsync(guildId, current.TrackIdentifier, current.Id);
            await playbackStateRepository.SetRepeatStateAsync(guildId, isRepeating: true, isRepeatingList: true);
            await repeatListRepository.ReplaceAsync(guildId, ["current-track", "queued-track"]);
            currentQueueItemId = current.Id;
        }

        await using var secondProvider = database.CreateServiceProvider();
        var recreatedFactory = secondProvider.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var recreatedQueueRepository = new QueueRepository(recreatedFactory);
        var recreatedPlaybackStateRepository = new PlaybackStateRepository(recreatedFactory);
        var recreatedRepeatListRepository = new RepeatListRepository(recreatedFactory);

        var playbackState = await recreatedPlaybackStateRepository.GetOrCreateAsync(guildId);
        var queuedItems = await recreatedQueueRepository.GetQueuedItemsAsync(guildId);
        var repeatList = await recreatedRepeatListRepository.GetTrackIdentifiersAsync(guildId);

        Assert.True(playbackState.IsRepeating);
        Assert.True(playbackState.IsRepeatingList);
        Assert.Equal("current-track", playbackState.CurrentTrackIdentifier);
        Assert.Equal(currentQueueItemId, playbackState.QueueItemId);
        Assert.Equal(["current-track", "queued-track"], repeatList);
        Assert.Equal(["current-track", "queued-track"], queuedItems.Select(item => item.TrackIdentifier));
    }
}
