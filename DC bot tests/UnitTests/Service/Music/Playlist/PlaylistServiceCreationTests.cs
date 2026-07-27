using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models;
using Lavalink4NET.Rest.Entities.Tracks;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.Playlist;

[Trait("Category", "Unit")]
public class PlaylistServiceCreationTests : PlaylistServiceTestBase
{
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
    public async Task CreatePlaylistAsync_WhenPlaylistLimitReached_ReturnsPlaylistLimitReached()
    {
        var context = CreateContext(new() { MaxPlaylistsPerGuild = 1 });
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAsync(GuildId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PlaylistSummaryRecord(PlaylistId, GuildId, "existing", 0)]);

        var result = await context.Service.CreatePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(CreatePlaylistResult.PlaylistLimitReached, result);
        context.PlaylistRepository.Verify(repository => repository.CreatePlaylistAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
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
    public async Task SavePlaylistAsync_WhenNameIsInvalid_ReturnsInvalidPlaylistName()
    {
        var context = CreateContext();

        var result = await context.Service.SavePlaylistAsync(GuildId, string.Empty, "https://example.com/playlist");

        Assert.Equal(SavePlaylistResult.InvalidPlaylistName, result);
        context.PlaylistRepository.Verify(repository => repository.ExistsAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SavePlaylistAsync_WhenPlaylistLimitReached_ReturnsPlaylistLimitReached()
    {
        var context = CreateContext(new() { MaxPlaylistsPerGuild = 1 });
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAsync(GuildId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PlaylistSummaryRecord(PlaylistId, GuildId, "existing", 0)]);

        var result = await context.Service.SavePlaylistAsync(GuildId, PlaylistName, "https://example.com/playlist");

        Assert.Equal(SavePlaylistResult.PlaylistLimitReached, result);
        context.TrackSearchResolver.Verify(resolver => resolver.ResolveSearchMode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SavePlaylistAsync_WhenTrackLimitExceeded_ReturnsTrackLimitExceeded()
    {
        var context = CreateContext(new() { MaxImportedTracks = 1, MaxTracksPerPlaylist = 10 });
        var first = CreateLavalinkTrack("a", "one");
        var second = CreateLavalinkTrack("b", "two");
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.AudioService
            .Setup(service => service.Tracks.LoadTracksAsync(
                "https://example.com/playlist",
                TrackSearchMode.YouTube,
                default,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(new[] { first, second }, null)));

        var result = await context.Service.SavePlaylistAsync(GuildId, PlaylistName, "https://example.com/playlist");

        Assert.Equal(SavePlaylistResult.TrackLimitExceeded, result);
        context.PlaylistRepository.Verify(repository => repository.CreatePlaylistAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SavePlaylistAsync_WhenTrackInsertFails_DeletesCreatedPlaylist()
    {
        var context = CreateContext();
        var track = CreateLavalinkTrack();
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.PlaylistRepository.Setup(repository => repository.CreatePlaylistAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlaylistId);
        context.AudioService
            .Setup(service => service.Tracks.LoadTracksAsync(
                "https://example.com/playlist",
                TrackSearchMode.YouTube,
                default,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(track, null)));
        context.PlaylistTrackRepository.Setup(repository => repository.AddRangeAsync(
                PlaylistId,
                It.IsAny<IReadOnlyCollection<PlaylistTrackCreateRecord>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("insert failed"));

        var result = await context.Service.SavePlaylistAsync(GuildId, PlaylistName, "https://example.com/playlist");

        Assert.Equal(SavePlaylistResult.UnknownError, result);
        context.PlaylistRepository.Verify(repository => repository.DeleteByGuildAndNameAsync(
            GuildId,
            PlaylistName,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
