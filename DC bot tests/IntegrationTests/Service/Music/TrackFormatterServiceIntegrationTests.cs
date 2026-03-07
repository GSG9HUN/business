using DC_bot.Interface;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Tracks;

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

        var current = CreateTrack("CurrentAuthor", "CurrentTitle", "track-current");

        currentTrackService.SetCurrentTrack(guildId, current);
        queueService.Enqueue(guildId, CreateQueueTrack("QueueAuthorA", "QueueTitleA", "track-a"));
        queueService.Enqueue(guildId, CreateQueueTrack("QueueAuthorB", "QueueTitleB", "track-b"));

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

        currentTrackService.SetCurrentTrack(guildA, CreateTrack("A-CurrentAuthor", "A-CurrentTitle", "a-current"));
        currentTrackService.SetCurrentTrack(guildB, CreateTrack("B-CurrentAuthor", "B-CurrentTitle", "b-current"));

        queueService.Enqueue(guildA, CreateQueueTrack("A-QueueAuthor", "A-QueueTitle", "a-queue"));
        queueService.Enqueue(guildB, CreateQueueTrack("B-QueueAuthor", "B-QueueTitle", "b-queue"));

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

        var current = CreateTrack("CurrentAuthor", "CurrentTitle", "repeat-current");

        currentTrackService.SetCurrentTrack(guildId, current);
        queueService.Enqueue(guildId, CreateQueueTrack("QueueAuthorA", "QueueTitleA", "repeat-a"));
        queueService.Enqueue(guildId, CreateQueueTrack("QueueAuthorB", "QueueTitleB", "repeat-b"));
        
        queueService.Clone(guildId, current);
        
        queueService.Dequeue(guildId);
        queueService.Dequeue(guildId);
        Assert.False(queueService.HasTracks(guildId));

        foreach (var t in queueService.GetRepeatableQueue(guildId))
        {
            // Avoid persisting raw LavalinkTrackWrapper instances; SaveQueue serializes via ToString().
            var track = t.ToLavalinkTrack();
            queueService.Enqueue(guildId, CreateQueueTrack(track.Author, track.Title, track.Identifier));
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

    private static LavalinkTrack CreateTrack(string author, string title, string id)
    {
        return new LavalinkTrack
        {
            Author = author,
            Title = title,
            Identifier = id,
            Duration = TimeSpan.FromSeconds(120)
        };
    }

    private static ILavaLinkTrack CreateQueueTrack(string author, string title, string id)
    {
        return new FakeQueueTrack(author, title, id);
    }

    private sealed class FakeQueueTrack : ILavaLinkTrack
    {
        private readonly string _author;
        private readonly string _title;
        private readonly string _id;

        public FakeQueueTrack(string author, string title, string id)
        {
            _author = author;
            _title = title;
            _id = id;
        }

        public string Title => _title;
        public string Author => _author;

        public LavalinkTrack ToLavalinkTrack()
        {
            return new LavalinkTrack
            {
                Author = _author,
                Title = _title,
                Identifier = _id,
                Duration = TimeSpan.FromSeconds(120)
            };
        }

        public override string ToString()
        { 
            return _id;
        }
    }
}