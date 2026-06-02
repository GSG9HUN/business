using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Wrapper;
using DC_bot_tests.IntegrationTests.Persistence;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Tracks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Music;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class TrackEndedHandlerServicePostgreSqlIntegrationTests
{
    [Fact]
    public async Task HandleTrackEndedAsync_WithRealPersistence_MarksCurrentItemPlayedAndStartsNextQueuedTrack()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();

        const ulong guildId = 9301UL;
        var queueRepository = CreateQueueRepository(services);
        var repeatListRepository = CreateRepeatListRepository(services);
        var playbackStateRepository = CreatePlaybackStateRepository(services);
        var repeatService = new RepeatService(playbackStateRepository, repeatListRepository);
        var currentTrackService = new CurrentTrackService(playbackStateRepository);
        var musicQueueService = new MusicQueueService(queueRepository, repeatListRepository);
        var trackPlaybackService = new Mock<ITrackPlaybackService>();
        var trackNotificationService = new Mock<ITrackNotificationService>();
        var service = CreateService(
            repeatService,
            currentTrackService,
            musicQueueService,
            trackPlaybackService.Object,
            trackNotificationService.Object,
            queueRepository);

        var currentTrack = CreateTrack("ended-current", "Ended Current");
        await queueRepository.EnqueueAsync(guildId, currentTrack.ToString());
        var claimedCurrentItem = await queueRepository.ClaimNextQueuedItemAsync(guildId);
        Assert.NotNull(claimedCurrentItem);
        await currentTrackService.SetCurrentTrackAsync(
            guildId,
            CreateTrack("ended-current", "Ended Current", claimedCurrentItem.Id));
        await musicQueueService.Enqueue(guildId, CreateTrack("ended-next", "Ended Next"));

        var player = CreatePlayer(guildId);
        var channel = CreateTextChannel(guildId);

        await service.HandleTrackEndedAsync(
            player.Object,
            new TrackEndedEventArgs(player.Object, currentTrack.ToLavalinkTrack(), TrackEndReason.Finished),
            channel.Object);

        var previousItem = await queueRepository.GetPreviousItemAsync(guildId);
        Assert.NotNull(previousItem);
        Assert.Equal(claimedCurrentItem.Id, previousItem.Id);
        Assert.Equal(2, previousItem.State);
        trackPlaybackService.Verify(playback => playback.PlayTrackFromQueueAsync(player.Object, channel.Object), Times.Once);
        trackNotificationService.Verify(notification => notification.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task HandleTrackEndedAsync_WithRepeatListSnapshot_RequeuesSnapshotAndStartsPlayback()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();

        const ulong guildId = 9302UL;
        var queueRepository = CreateQueueRepository(services);
        var repeatListRepository = CreateRepeatListRepository(services);
        var playbackStateRepository = CreatePlaybackStateRepository(services);
        var repeatService = new RepeatService(playbackStateRepository, repeatListRepository);
        var currentTrackService = new CurrentTrackService(playbackStateRepository);
        var musicQueueService = new MusicQueueService(queueRepository, repeatListRepository);
        var trackPlaybackService = new Mock<ITrackPlaybackService>();
        var trackNotificationService = new Mock<ITrackNotificationService>();
        var service = CreateService(
            repeatService,
            currentTrackService,
            musicQueueService,
            trackPlaybackService.Object,
            trackNotificationService.Object,
            queueRepository);

        var first = CreateTrack("repeat-list-first", "Repeat List First");
        var second = CreateTrack("repeat-list-second", "Repeat List Second");
        await repeatService.SetRepeatingListAsync(guildId, true);
        await repeatService.SaveRepeatListSnapshotAsync(guildId, null, [first, second]);

        var player = CreatePlayer(guildId);
        var channel = CreateTextChannel(guildId);

        await service.HandleTrackEndedAsync(
            player.Object,
            new TrackEndedEventArgs(player.Object, first.ToLavalinkTrack(), TrackEndReason.Finished),
            channel.Object);

        var queuedItems = await queueRepository.GetQueuedItemsAsync(guildId);
        Assert.Equal([first.ToString(), second.ToString()], queuedItems.Select(item => item.TrackIdentifier));
        trackPlaybackService.Verify(playback => playback.PlayTrackFromQueueAsync(player.Object, channel.Object), Times.Once);
        trackNotificationService.Verify(notification => notification.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    private static TrackEndedHandlerService CreateService(
        IRepeatService repeatService,
        ICurrentTrackService currentTrackService,
        IMusicQueueService musicQueueService,
        ITrackPlaybackService trackPlaybackService,
        ITrackNotificationService trackNotificationService,
        QueueRepository queueRepository)
    {
        return new TrackEndedHandlerService(
            repeatService,
            currentTrackService,
            musicQueueService,
            trackPlaybackService,
            trackNotificationService,
            queueRepository,
            Mock.Of<ILogger<TrackEndedHandlerService>>());
    }

    private static QueueRepository CreateQueueRepository(ServiceProvider services)
    {
        return new QueueRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static PlaybackStateRepository CreatePlaybackStateRepository(ServiceProvider services)
    {
        return new PlaybackStateRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static RepeatListRepository CreateRepeatListRepository(ServiceProvider services)
    {
        return new RepeatListRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static Mock<ILavalinkPlayer> CreatePlayer(ulong guildId)
    {
        var player = new Mock<ILavalinkPlayer>();
        player.SetupGet(x => x.GuildId).Returns(guildId);
        return player;
    }

    private static Mock<IDiscordChannel> CreateTextChannel(ulong guildId)
    {
        var guild = new Mock<IDiscordGuild>();
        guild.SetupGet(x => x.Id).Returns(guildId);
        guild.SetupGet(x => x.Name).Returns("IntegrationGuild");

        var channel = new Mock<IDiscordChannel>();
        channel.SetupGet(x => x.Id).Returns(999UL);
        channel.SetupGet(x => x.Name).Returns("integration-text");
        channel.SetupGet(x => x.Guild).Returns(guild.Object);
        return channel;
    }

    private static LavaLinkTrackWrapper CreateTrack(string identifier, string title, long? queueItemId = null)
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
        })
        {
            QueueItemId = queueItemId
        };
    }
}
