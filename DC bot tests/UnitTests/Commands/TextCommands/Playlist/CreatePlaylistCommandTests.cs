using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class CreatePlaylistCommandTests : PlaylistCommandTestBase
{
    [Theory]
    [InlineData(CreatePlaylistResult.Created, "success", LocalizationKeys.CreatePlaylistCommandCreated)]
    [InlineData(CreatePlaylistResult.PlaylistAlreadyExists, "warning", LocalizationKeys.CreatePlaylistCommandAlreadyExists)]
    [InlineData(CreatePlaylistResult.InvalidPlaylistName, "warning", LocalizationKeys.CreatePlaylistCommandInvalidPlaylistName)]
    [InlineData(CreatePlaylistResult.UnknownError, "error", LocalizationKeys.CreatePlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedResponse(
        CreatePlaylistResult result,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.CreatePlaylistAsync(GuildId, PlaylistName)).ReturnsAsync(result);
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("createPlaylist", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private CreatePlaylistCommand CreateCommand()
    {
        return new CreatePlaylistCommand(
            Mock.Of<ILogger<CreatePlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
