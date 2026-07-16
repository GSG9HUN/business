using DC_bot.Interface;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Service.Music.PlaylistService;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.Playlist;

public abstract class PlaylistServiceTestBase
{
    protected const ulong GuildId = 42ul;
    protected const long PlaylistId = 1001L;
    protected const string PlaylistName = "mix";
    protected const string NewPlaylistName = "renamed";

    protected static TestContext CreateContext()
    {
        var audioService = new Mock<IAudioService>();
        var playlistRepository = new Mock<IPlaylistRepository>();
        var playlistTrackRepository = new Mock<IPlaylistTrackRepository>();
        var trackSearchResolver = new Mock<ITrackSearchResolverService>();
        var trackSerializer = new Mock<ITrackSerializer>();

        return new TestContext(
            new PlaylistService(
                audioService.Object,
                playlistRepository.Object,
                playlistTrackRepository.Object,
                trackSearchResolver.Object,
                trackSerializer.Object,
                Mock.Of<ILogger<PlaylistService>>()),
            playlistRepository,
            playlistTrackRepository,
            trackSearchResolver,
            trackSerializer);
    }

    protected static ILavaLinkTrack CreateTrack(string author, string title, TimeSpan duration)
    {
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(item => item.Author).Returns(author);
        track.SetupGet(item => item.Title).Returns(title);
        track.SetupGet(item => item.Duration).Returns(duration);
        return track.Object;
    }

    protected sealed record TestContext(
        PlaylistService Service,
        Mock<IPlaylistRepository> PlaylistRepository,
        Mock<IPlaylistTrackRepository> PlaylistTrackRepository,
        Mock<ITrackSearchResolverService> TrackSearchResolver,
        Mock<ITrackSerializer> TrackSerializer);
}
