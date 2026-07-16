using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.Playlist;

[Trait("Category", "Unit")]
public class PlaylistServiceQueryTests : PlaylistServiceTestBase
{
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
}
