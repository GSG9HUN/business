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
public class RepeatAndCurrentTrackServicePostgreSqlIntegrationTests
{
    [Fact]
    public async Task RepeatAndCurrentTrackServices_WithRealRepositories_PersistSharedPlaybackState()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();

        var playbackStateRepository = CreatePlaybackStateRepository(services);
        var repeatListRepository = CreateRepeatListRepository(services);
        var repeatService = new RepeatService(playbackStateRepository, repeatListRepository);
        var currentTrackService = new CurrentTrackService(playbackStateRepository);

        const ulong guildId = 9201UL;
        var currentTrack = CreateTrack("current-track", "Current Track", queueItemId: 77L);
        var queuedTrack = CreateTrack("queued-track", "Queued Track");

        await repeatService.InitAsync(guildId);
        await currentTrackService.SetCurrentTrackAsync(guildId, currentTrack);
        await repeatService.SetRepeatingAsync(guildId, true);
        await repeatService.SetRepeatingListAsync(guildId, true);
        await repeatService.SaveRepeatListSnapshotAsync(guildId, currentTrack, [queuedTrack]);

        var restoredCurrentTrack = await currentTrackService.GetCurrentTrackAsync(guildId);
        Assert.NotNull(restoredCurrentTrack);
        Assert.Equal("Current Track", restoredCurrentTrack.Title);
        Assert.Equal(77L, ((LavaLinkTrackWrapper)restoredCurrentTrack).QueueItemId);
        Assert.True(await repeatService.IsRepeatingAsync(guildId));
        Assert.True(await repeatService.IsRepeatingListAsync(guildId));
        Assert.Equal(
            [currentTrack.ToString(), queuedTrack.ToString()],
            await repeatListRepository.GetTrackIdentifiersAsync(guildId));

        await repeatService.SetRepeatingListAsync(guildId, false);

        Assert.True(await repeatService.IsRepeatingAsync(guildId));
        Assert.False(await repeatService.IsRepeatingListAsync(guildId));
        Assert.Empty(await repeatListRepository.GetTrackIdentifiersAsync(guildId));
    }

    private static PlaybackStateRepository CreatePlaybackStateRepository(ServiceProvider services)
    {
        return new PlaybackStateRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
    }

    private static RepeatListRepository CreateRepeatListRepository(ServiceProvider services)
    {
        return new RepeatListRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
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
