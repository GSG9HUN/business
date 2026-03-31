using DC_bot_tests.TestHelperFiles;
using DC_bot.Service.Music.MusicServices;

namespace DC_bot_tests.IntegrationTests.Service.Music;

public class TrackFormatterServiceIntegrationTests
{
    [Fact]
    public void FormatCurrentTrackList_WhenQueueChanges_ReflectsLatestState()
    {
        // Arrange
        const ulong guildId = 1001;
        var currentTrackService = new CurrentTrackService();
        currentTrackService.Init(guildId);

        var queueService = new MusicQueueService();
        queueService.Init(guildId);

        var formatter = new TrackFormatterService(currentTrackService, queueService);

        var current = TrackTestHelper.CreateTrackWrapper(
            "Noize Generation",
            "Sugababes - Round Round (Noize Generation Remix)",
            "4g7MsUvOZN4",
            100D
        );
        
        currentTrackService.SetCurrentTrack(guildId, current);

        queueService.Enqueue(guildId,
            TrackTestHelper.CreateTrackWrapper(
                "Noize Generation",
                "Sugababes - Round Round (Noize Generation Remix)",
                "4g7MsUvOZN4"
                )
            );

        queueService.Enqueue(guildId,
            TrackTestHelper.CreateTrackWrapper(
                "Noize Generation",
                "Sugababes - Round Round (Noize Generation Remix)",
                "4g7MsUvOZN4"
            )
        );

        // Act
        var beforeDequeue = formatter.FormatCurrentTrackList(guildId);
        var dequeued = queueService.Dequeue(guildId);
        var afterDequeue = formatter.FormatCurrentTrackList(guildId);

        // Assert
        Assert.NotNull(dequeued);
        Assert.Equal("QueueTitleA", dequeued.Title);

        Assert.Equal(
            "CurrentAuthor CurrentTitle\nQueueAuthorA QueueTitleA\nQueueAuthorB QueueTitleB\n",
            beforeDequeue);

        Assert.Equal(
            "CurrentAuthor CurrentTitle\nQueueAuthorB QueueTitleB\n",
            afterDequeue);
    }

    [Fact]
    public void FormatCurrentTrackList_MultiGuildState_DoesNotLeakBetweenGuilds()
    {
        // Arrange
        const ulong guildA = 2001;
        const ulong guildB = 2002;

        var currentTrackService = new CurrentTrackService();
        currentTrackService.Init(guildA);
        currentTrackService.Init(guildB);

        var queueService = new MusicQueueService();
        queueService.Init(guildA);
        queueService.Init(guildB);

        var formatter = new TrackFormatterService(currentTrackService, queueService);

        currentTrackService.SetCurrentTrack(guildA,
            TrackTestHelper.CreateTrackWrapper("A-CurrentAuthor", "A-CurrentTitle", "a-current")
        );
        currentTrackService.SetCurrentTrack(guildB,
            TrackTestHelper.CreateTrackWrapper("B-CurrentAuthor", "B-CurrentTitle", "b-current")
        );

        queueService.Enqueue(guildA,
            TrackTestHelper.CreateTrackWrapper("A-CurrentAuthor", "A-CurrentTitle", "a-current")
        );
        queueService.Enqueue(guildB, TrackTestHelper.CreateTrackWrapper("B-CurrentAuthor", "B-CurrentTitle", "b-current")
        );

        // Act
        var resultA = formatter.FormatCurrentTrackList(guildA);
        var resultB = formatter.FormatCurrentTrackList(guildB);

        // Assert
        Assert.Contains("A-CurrentAuthor A-CurrentTitle", resultA);
        Assert.Contains("A-QueueAuthor A-QueueTitle", resultA);
        Assert.DoesNotContain("B-CurrentAuthor", resultA);
        Assert.DoesNotContain("B-QueueAuthor", resultA);

        Assert.Contains("B-CurrentAuthor B-CurrentTitle", resultB);
        Assert.Contains("B-QueueAuthor B-QueueTitle", resultB);
        Assert.DoesNotContain("A-CurrentAuthor", resultB);
        Assert.DoesNotContain("A-QueueAuthor", resultB);
    }

    [Fact]
    public void CloneRepeatableQueue_PreservesOrder_ForRepeatListFlow()
    {
        // Arrange
        const ulong guildId = 3001;

        var currentTrackService = new CurrentTrackService();
        currentTrackService.Init(guildId);

        var queueService = new MusicQueueService();
        queueService.Init(guildId);

        var formatter = new TrackFormatterService(currentTrackService, queueService);

        var current = TrackTestHelper.CreateTrackWrapper("CurrentAuthor", "CurrentTitle", "repeat-current");

        currentTrackService.SetCurrentTrack(guildId, current);
        queueService.Enqueue(guildId, TrackTestHelper.CreateTrackWrapper("QueueAuthorA", "QueueTitleA", "track-a"));
        queueService.Enqueue(guildId, TrackTestHelper.CreateTrackWrapper("QueueAuthorB", "QueueTitleB", "track-b"));

        queueService.Clone(guildId, current);

        queueService.Dequeue(guildId);
        queueService.Dequeue(guildId);
        Assert.False(queueService.HasTracks(guildId));

        foreach (var t in queueService.GetRepeatableQueue(guildId))
        {
            var track = t.ToLavalinkTrack();
            queueService.Enqueue(guildId, TrackTestHelper.CreateTrackWrapper(track.Author, track.Title, track.Identifier));
        }

        var firstRepeated = queueService.Dequeue(guildId);
        currentTrackService.SetCurrentTrack(guildId, firstRepeated);

        // Act
        var formatted = formatter.FormatCurrentTrackList(guildId);

        // Assert
        Assert.Equal("CurrentTitle", firstRepeated!.Title);
        Assert.Equal(
            "CurrentAuthor CurrentTitle\nQueueAuthorA QueueTitleA\nQueueAuthorB QueueTitleB\n",
            formatted);
    }
}