using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class SavePlaylistCommandTests : PlaylistCommandTestBase
{
    [Theory]
    [InlineData(SavePlaylistResult.Saved, "success", LocalizationKeys.SavePlaylistCommandSaved)]
    [InlineData(SavePlaylistResult.AlreadyExists, "warning", LocalizationKeys.SavePlaylistCommandAlreadyExists)]
    [InlineData(SavePlaylistResult.NoTracksFound, "warning", LocalizationKeys.SavePlaylistCommandNoTracksFound)]
    [InlineData(SavePlaylistResult.UnknownError, "error", LocalizationKeys.SavePlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedResponse(
        SavePlaylistResult result,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.SavePlaylistAsync(GuildId, PlaylistName, PlaylistUrl))
            .ReturnsAsync(result);
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("savePlaylist", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private SavePlaylistCommand CreateCommand()
    {
        return new SavePlaylistCommand(
            Mock.Of<ILogger<SavePlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
