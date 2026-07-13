using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class AddSongToPlaylistCommandTests : PlaylistCommandTestBase
{
    [Theory]
    [InlineData(AddSongResult.Added, "success", LocalizationKeys.AddSongToPlaylistCommandAdded)]
    [InlineData(AddSongResult.PlaylistDoesNotExist, "warning", LocalizationKeys.AddSongToPlaylistCommandPlaylistDoesNotExist)]
    [InlineData(AddSongResult.NoTracksFound, "warning", LocalizationKeys.AddSongToPlaylistCommandNoTracksFound)]
    [InlineData(AddSongResult.InvalidSongUrl, "warning", LocalizationKeys.AddSongToPlaylistCommandInvalidSongUrl)]
    [InlineData(AddSongResult.UnknownError, "error", LocalizationKeys.AddSongToPlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedResponse(
        AddSongResult result,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.AddSongToPlaylistAsync(GuildId, PlaylistName, SongUrl))
            .ReturnsAsync(result);
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("addSong", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private AddSongToPlaylistCommand CreateCommand()
    {
        return new AddSongToPlaylistCommand(
            Mock.Of<ILogger<AddSongToPlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
