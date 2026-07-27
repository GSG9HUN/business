using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
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
    public async Task SavePlaylistAsync_WhenPlaylistAlreadyExists_ReturnsAlreadyExistsWithoutLoadingTracks()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(GuildId, PlaylistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.SavePlaylistAsync(GuildId, PlaylistName, "https://example.com/playlist");

        Assert.Equal(SavePlaylistResult.AlreadyExists, result);
        context.TrackSearchResolver.Verify(resolver => resolver.ResolveSearchMode(It.IsAny<string>()), Times.Never);
    }
}
