using DC_bot.Wrapper;
using Lavalink4NET.Tracks;

namespace DC_bot_tests.TestHelperFiles;

public static class TrackTestHelper
{
    public static LavaLinkTrackWrapper CreateTrackWrapper(string author = "Song", string title = "Artist", string identifier = "id", double durationSeconds = 100, long? queueItemId = null)
    {
        return new LavaLinkTrackWrapper(new LavalinkTrack
        {
            Author = author,
            Title = title,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            Identifier = identifier,
            IsLiveStream = false,
            IsSeekable = true,
            SourceName = "youtube",
            Uri = new Uri("https://example.com/track")
        })
        {
            QueueItemId = queueItemId
        };
    }
}

