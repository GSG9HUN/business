using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.Playlist;

[Trait("Category", "Unit")]
public class PlaylistServiceTrackMutationTests : PlaylistServiceTestBase
{
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
}
