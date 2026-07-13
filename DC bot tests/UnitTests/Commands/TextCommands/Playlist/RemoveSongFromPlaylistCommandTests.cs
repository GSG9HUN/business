using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class RemoveSongFromPlaylistCommandTests : PlaylistCommandTestBase
{
    [Theory]
    [InlineData(RemoveSongResult.Removed, "success", LocalizationKeys.RemoveSongFromPlaylistCommandRemoved)]
    [InlineData(RemoveSongResult.PlaylistDoesNotExist, "warning", LocalizationKeys.RemoveSongFromPlaylistCommandPlaylistDoesNotExist)]
    [InlineData(RemoveSongResult.SongNotFound, "warning", LocalizationKeys.RemoveSongFromPlaylistCommandSongNotFound)]
    [InlineData(RemoveSongResult.InvalidPlaylistName, "warning", LocalizationKeys.RemoveSongFromPlaylistCommandInvalidPlaylistName)]
    [InlineData(RemoveSongResult.InvalidTrackNumber, "warning", LocalizationKeys.RemoveSongFromPlaylistCommandInvalidTrackNumber)]
    [InlineData(RemoveSongResult.UnknownError, "error", LocalizationKeys.RemoveSongFromPlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedResponse(
        RemoveSongResult result,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, TrackNumber))
            .ReturnsAsync(result);
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public async Task ExecuteAsync_WhenTrackNumberIsInvalid_SendsWarningWithoutCallingService(string trackNumber)
    {
        CommandHelperMock
            .Setup(helper => helper.TryParseSavePlaylistArguments(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.Is<string>(name => name == "removeSong")))
            .ReturnsAsync((PlaylistName, trackNumber));
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse("warning", LocalizationKeys.RemoveSongFromPlaylistCommandInvalidTrackNumber);
        PlaylistServiceMock.Verify(service => service.RemoveSongFromPlaylistAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("removeSong", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private RemoveSongFromPlaylistCommand CreateCommand()
    {
        return new RemoveSongFromPlaylistCommand(
            Mock.Of<ILogger<RemoveSongFromPlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
