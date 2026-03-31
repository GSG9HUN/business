using DC_bot.Wrapper;
using Lavalink4NET.Tracks;

namespace DC_bot_tests.TestHelperFiles;

public static class TrackTestHelper
{
    public static LavaLinkTrackWrapper CreateTrackWrapper(string author = "Song", string title = "Artist", string identifier = "id", double durationSeconds = 100)
    {
        return new LavaLinkTrackWrapper(new LavalinkTrack
        {
            Author = author,
            Title = title,
            Identifier = identifier,
            Duration = TimeSpan.FromSeconds(durationSeconds)
        });
    }
}

