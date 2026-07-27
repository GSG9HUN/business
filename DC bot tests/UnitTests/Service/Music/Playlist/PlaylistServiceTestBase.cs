using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Service.Music.PlaylistService;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.Playlist;

public abstract class PlaylistServiceTestBase
{
    protected const ulong GuildId = 42ul;
    protected const long PlaylistId = 1001L;
    protected const string PlaylistName = "mix";
    protected const string NewPlaylistName = "renamed";

    protected static TestContext CreateContext(PlaylistOptions? playlistOptions = null)
    {
        var audioService = new Mock<IAudioService>();
        var playlistRepository = new Mock<IPlaylistRepository>();
        var playlistTrackRepository = new Mock<IPlaylistTrackRepository>();
        var trackSearchResolver = new Mock<ITrackSearchResolverService>();
        var trackSerializer = new Mock<ITrackSerializer>();

        playlistRepository
            .Setup(repository => repository.GetByGuildAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        trackSearchResolver
            .Setup(resolver => resolver.ResolveSearchMode(It.IsAny<string>()))
            .Returns(TrackSearchMode.YouTube);
        trackSerializer
            .Setup(serializer => serializer.Serialize(It.IsAny<ILavaLinkTrack>()))
            .Returns("serialized-track");

        return new TestContext(
            new PlaylistService(
                audioService.Object,
                playlistRepository.Object,
                playlistTrackRepository.Object,
                trackSearchResolver.Object,
                trackSerializer.Object,
                Options.Create(playlistOptions ?? new PlaylistOptions()),
                Mock.Of<ILogger<PlaylistService>>()),
            audioService,
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

    protected static LavalinkTrack CreateLavalinkTrack(string author = "Author", string title = "Title")
    {
        return new LavalinkTrack
        {
            Author = author,
            Title = title,
            Identifier = $"{author}-{title}",
            Duration = TimeSpan.FromMinutes(3)
        };
    }

    protected sealed record TestContext(
        PlaylistService Service,
        Mock<IAudioService> AudioService,
        Mock<IPlaylistRepository> PlaylistRepository,
        Mock<IPlaylistTrackRepository> PlaylistTrackRepository,
        Mock<ITrackSearchResolverService> TrackSearchResolver,
        Mock<ITrackSerializer> TrackSerializer);
}
