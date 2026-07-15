using DC_bot.Interface;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Service.Music.PlaylistService;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class PlaylistServiceTests
{
    private const ulong GuildId = 42ul;
    private const long PlaylistId = 1001L;
    private const string PlaylistName = "mix";
    private const string NewPlaylistName = "renamed";

    [Fact]
    public async Task CreatePlaylistAsync_WhenNameIsInvalid_ReturnsInvalidPlaylistName()
    {
        var context = CreateContext();

        var result = await context.Service.CreatePlaylistAsync(GuildId, string.Empty);

        Assert.Equal(CreatePlaylistResult.InvalidPlaylistName, result);
        context.PlaylistRepository.Verify(repository => repository.CreatePlaylistAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePlaylistAsync_WhenPlaylistAlreadyExists_ReturnsPlaylistAlreadyExists()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.CreatePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(CreatePlaylistResult.PlaylistAlreadyExists, result);
    }

    [Fact]
    public async Task CreatePlaylistAsync_WhenPlaylistDoesNotExist_CreatesPlaylist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await context.Service.CreatePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(CreatePlaylistResult.Created, result);
        context.PlaylistRepository.Verify(repository => repository.CreatePlaylistAsync(
            GuildId,
            PlaylistName,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SavePlaylistAsync_WhenPlaylistAlreadyExists_ReturnsAlreadyExistsWithoutLoadingTracks()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.SavePlaylistAsync(GuildId, PlaylistName, "https://example.com/playlist");

        Assert.Equal(SavePlaylistResult.AlreadyExists, result);
        context.TrackSearchResolver.Verify(resolver => resolver.ResolveSearchMode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ListPlaylistsAsync_WhenRepositoryReturnsNoPlaylists_ReturnsNoPlaylists()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAsync(GuildId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await context.Service.ListPlaylistsAsync(GuildId);

        Assert.Equal(ListPlaylistsStatus.NoPlaylists, result.Status);
        Assert.Empty(result.Playlists);
    }

    [Fact]
    public async Task ListPlaylistsAsync_WhenRepositoryReturnsSummaries_MapsTrackCounts()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAsync(GuildId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PlaylistSummaryRecord(1, GuildId, "alpha", 2),
                new PlaylistSummaryRecord(2, GuildId, "beta", 5)
            ]);

        var result = await context.Service.ListPlaylistsAsync(GuildId);

        Assert.Equal(ListPlaylistsStatus.Listed, result.Status);
        Assert.Equal(["alpha", "beta"], result.Playlists.Select(playlist => playlist.Name));
        Assert.Equal([2, 5], result.Playlists.Select(playlist => playlist.TrackCount));
    }

    [Fact]
    public async Task ViewPlaylistAsync_WhenPlaylistDoesNotExist_ReturnsPlaylistDoesNotExist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistRecord?)null);

        var result = await context.Service.ViewPlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(ViewPlaylistStatus.PlaylistDoesNotExist, result.Status);
        Assert.Empty(result.Tracks);
    }

    [Fact]
    public async Task ViewPlaylistAsync_WhenPlaylistHasNoTracks_ReturnsEmptyPlaylist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistTrackRepository.Setup(repository => repository.GetByPlaylistIdOrderedAsync(
                PlaylistId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await context.Service.ViewPlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(ViewPlaylistStatus.EmptyPlaylist, result.Status);
        Assert.Empty(result.Tracks);
    }

    [Fact]
    public async Task ViewPlaylistAsync_WhenPlaylistHasTracks_ReturnsDisplayableTrackData()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistTrackRepository.Setup(repository => repository.GetByPlaylistIdOrderedAsync(
                PlaylistId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PlaylistTrackRecord(10, PlaylistId, 1, "YouTube", "serialized-a", "https://example.com/a"),
                new PlaylistTrackRecord(11, PlaylistId, 2, "YouTube", "serialized-b", "https://example.com/b")
            ]);
        context.TrackSerializer.Setup(serializer => serializer.Deserialize("serialized-a", null))
            .Returns(CreateTrack("Author A", "Title A", TimeSpan.FromSeconds(65)));
        context.TrackSerializer.Setup(serializer => serializer.Deserialize("serialized-b", null))
            .Returns(CreateTrack("Author B", "Title B", TimeSpan.FromSeconds(125)));

        var result = await context.Service.ViewPlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(ViewPlaylistStatus.Viewed, result.Status);
        Assert.Equal(PlaylistName, result.PlaylistName);
        Assert.Equal([1, 2], result.Tracks.Select(track => track.OrderNumber));
        Assert.Equal(["Title A", "Title B"], result.Tracks.Select(track => track.Title));
        Assert.Equal(["Author A", "Author B"], result.Tracks.Select(track => track.Author));
        Assert.Equal(["https://example.com/a", "https://example.com/b"], result.Tracks.Select(track => track.TrackUri));
    }

    [Fact]
    public async Task ViewPlaylistAsync_WhenAllTracksAreUnparsable_ReturnsUnknownError()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistTrackRepository.Setup(repository => repository.GetByPlaylistIdOrderedAsync(
                PlaylistId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PlaylistTrackRecord(10, PlaylistId, 1, "YouTube", "bad", "https://example.com/a")]);
        context.TrackSerializer.Setup(serializer => serializer.Deserialize("bad", null))
            .Throws(new FormatException("bad track"));

        var result = await context.Service.ViewPlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(ViewPlaylistStatus.UnknownError, result.Status);
        Assert.Empty(result.Tracks);
    }

    [Fact]
    public async Task DeletePlaylistAsync_WhenPlaylistDoesNotExist_ReturnsDoesNotExist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistRecord?)null);

        var result = await context.Service.DeletePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(DeletePlaylistResult.DoesNotExist, result);
    }

    [Fact]
    public async Task DeletePlaylistAsync_WhenRepositoryDeletes_ReturnsDeleted()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistRepository.Setup(repository => repository.DeleteByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.DeletePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(DeletePlaylistResult.Deleted, result);
    }

    [Fact]
    public async Task AddSongToPlaylistAsync_WhenPlaylistDoesNotExist_ReturnsPlaylistDoesNotExist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistRecord?)null);

        var result = await context.Service.AddSongToPlaylistAsync(GuildId, PlaylistName, "https://example.com/song");

        Assert.Equal(AddSongResult.PlaylistDoesNotExist, result);
    }

    [Fact]
    public async Task RemoveSongFromPlaylistAsync_WhenPlaylistNameIsInvalid_ReturnsInvalidPlaylistName()
    {
        var context = CreateContext();

        var result = await context.Service.RemoveSongFromPlaylistAsync(GuildId, string.Empty, 1);

        Assert.Equal(RemoveSongResult.InvalidPlaylistName, result);
        context.PlaylistRepository.Verify(repository => repository.GetByGuildAndNameAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RemoveSongFromPlaylistAsync_WhenTrackNumberIsInvalid_ReturnsInvalidTrackNumber(int trackNumber)
    {
        var context = CreateContext();

        var result = await context.Service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, trackNumber);

        Assert.Equal(RemoveSongResult.InvalidTrackNumber, result);
        context.PlaylistRepository.Verify(repository => repository.GetByGuildAndNameAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveSongFromPlaylistAsync_WhenPlaylistDoesNotExist_ReturnsPlaylistDoesNotExist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistRecord?)null);

        var result = await context.Service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, 1);

        Assert.Equal(RemoveSongResult.PlaylistDoesNotExist, result);
    }

    [Fact]
    public async Task RemoveSongFromPlaylistAsync_WhenTrackNumberDoesNotExist_ReturnsSongNotFound()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistTrackRepository.Setup(repository => repository.GetByPlaylistIdOrderedAsync(
                PlaylistId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PlaylistTrackRecord(10, PlaylistId, 1, "YouTube", "track-a", "https://example.com/a")]);

        var result = await context.Service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, 2);

        Assert.Equal(RemoveSongResult.SongNotFound, result);
        context.PlaylistTrackRepository.Verify(repository => repository.RemoveTrackAsync(
            It.IsAny<long>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveSongFromPlaylistAsync_WhenTrackNumberExists_RemovesTrack()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistTrackRepository.Setup(repository => repository.GetByPlaylistIdOrderedAsync(
                PlaylistId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PlaylistTrackRecord(10, PlaylistId, 1, "YouTube", "track-a", "https://example.com/a")]);
        context.PlaylistTrackRepository.Setup(repository => repository.RemoveTrackAsync(
                PlaylistId,
                1,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await context.Service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, 1);

        Assert.Equal(RemoveSongResult.Removed, result);
        context.PlaylistTrackRepository.Verify(repository => repository.RemoveTrackAsync(
            PlaylistId,
            1,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveSongFromPlaylistAsync_WhenRepositoryThrows_ReturnsUnknownError()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("database failed"));

        var result = await context.Service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, 1);

        Assert.Equal(RemoveSongResult.UnknownError, result);
    }

    [Fact]
    public async Task RenamePlaylistAsync_WhenNewNameExists_ReturnsPlaylistAlreadyExists()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(
                GuildId,
                NewPlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.RenamePlaylistAsync(GuildId, PlaylistName, NewPlaylistName);

        Assert.Equal(RenamePlaylistResult.PlaylistAlreadyExists, result);
    }

    [Fact]
    public async Task RenamePlaylistAsync_WhenRepositoryRenames_ReturnsRenamed()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(
                GuildId,
                NewPlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.PlaylistRepository.Setup(repository => repository.RenameAsync(
                GuildId,
                PlaylistName,
                NewPlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.RenamePlaylistAsync(GuildId, PlaylistName, NewPlaylistName);

        Assert.Equal(RenamePlaylistResult.Renamed, result);
    }

    private static TestContext CreateContext()
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

    private static ILavaLinkTrack CreateTrack(string author, string title, TimeSpan duration)
    {
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(item => item.Author).Returns(author);
        track.SetupGet(item => item.Title).Returns(title);
        track.SetupGet(item => item.Duration).Returns(duration);
        return track.Object;
    }

    private sealed record TestContext(
        PlaylistService Service,
        Mock<IPlaylistRepository> PlaylistRepository,
        Mock<IPlaylistTrackRepository> PlaylistTrackRepository,
        Mock<ITrackSearchResolverService> TrackSearchResolver,
        Mock<ITrackSerializer> TrackSerializer);
}
