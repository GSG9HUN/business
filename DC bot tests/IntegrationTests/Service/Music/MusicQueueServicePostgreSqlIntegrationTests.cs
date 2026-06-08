using DC_bot.Interface;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Wrapper;
using DC_bot_tests.IntegrationTests.Persistence;
using Lavalink4NET.Tracks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Service.Music;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class MusicQueueServicePostgreSqlIntegrationTests
{
    [Fact]
    public async Task QueueFlow_WithRealRepositories_EnqueuesReordersDequeuesAndClears()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var queueRepository = CreateQueueRepository(services);
        var service = new MusicQueueService(queueRepository, CreateRepeatListRepository(services));

        const ulong guildId = 9101UL;
        var first = CreateTrack("track-1", "First");
        var second = CreateTrack("track-2", "Second");
        var third = CreateTrack("track-3", "Third");

        await service.Enqueue(guildId, first);
        await service.EnqueueMany(guildId, [second, third]);

        Assert.True(await service.HasTracks(guildId));
        Assert.Equal(["First", "Second", "Third"], (await service.ViewQueue(guildId)).Select(track => track.Title));

        await service.SetQueue(guildId, new Queue<ILavaLinkTrack>([third, first, second]));
        Assert.Equal(["Third", "First", "Second"], (await service.ViewQueue(guildId)).Select(track => track.Title));

        var dequeued = await service.Dequeue(guildId);
        Assert.NotNull(dequeued);
        Assert.Equal("Third", dequeued.Title);
        Assert.NotNull(((LavaLinkTrackWrapper)dequeued).QueueItemId);

        await service.ClearQueue(guildId);
        Assert.False(await service.HasTracks(guildId));
        Assert.Empty(await queueRepository.GetQueuedItemsAsync(guildId));
    }

    [Fact]
    public async Task GetRepeatableQueue_WithRealRepeatListRepository_ReturnsPersistedSnapshot()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repeatListRepository = CreateRepeatListRepository(services);
        var service = new MusicQueueService(CreateQueueRepository(services), repeatListRepository);

        const ulong guildId = 9102UL;
        var first = CreateTrack("repeat-1", "Repeat First");
        var second = CreateTrack("repeat-2", "Repeat Second");

        await repeatListRepository.ReplaceAsync(guildId, [first.ToString(), second.ToString()]);

        var repeatableQueue = await service.GetRepeatableQueue(guildId);

        Assert.Equal(["Repeat First", "Repeat Second"], repeatableQueue.Select(track => track.Title));
    }

    private static QueueRepository CreateQueueRepository(ServiceProvider services)
    {
        return new QueueRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static RepeatListRepository CreateRepeatListRepository(ServiceProvider services)
    {
        return new RepeatListRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static LavaLinkTrackWrapper CreateTrack(string identifier, string title)
    {
        return new LavaLinkTrackWrapper(new LavalinkTrack
        {
            Author = $"Author {identifier}",
            Title = title,
            Duration = TimeSpan.FromMinutes(3),
            Identifier = identifier,
            IsLiveStream = false,
            IsSeekable = true,
            SourceName = "youtube",
            Uri = new Uri($"https://example.com/{identifier}")
        });
    }
}
