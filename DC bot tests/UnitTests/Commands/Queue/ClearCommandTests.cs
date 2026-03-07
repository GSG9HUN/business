using DC_bot.Commands.Queue;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Queue;

public class ClearCommandTests
{
    private const string ClearCommandName = "clear";
    private const string ClearCommandDescriptionValue = "Clear the music queue.";
    
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock;
    private readonly ClearCommand _clearCommand;
    private readonly Mock<ICommandHelper> _commandHelperMock;

    public ClearCommandTests()
    {
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILogger<ClearCommand>> loggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.ClearCommandDescription))
            .Returns(ClearCommandDescriptionValue);

        _messageMock = new Mock<IDiscordMessage>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _musicQueueServiceMock = new Mock<IMusicQueueService>();
        _commandHelperMock = new Mock<ICommandHelper>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _clearCommand = new ClearCommand(userValidationService,_musicQueueServiceMock.Object,
            loggerMock.Object, _responseBuilderMock.Object, localizationServiceMock.Object, _commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        // Arrange
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((DC_bot.Helper.Validation.UserValidationResult?)null);

        // Act
        await _clearCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFails_ShouldSendError()
    {
        // Arrange
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((DC_bot.Helper.Validation.UserValidationResult?)null);

        // Act
        await _clearCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUser_ShouldClearQueue()
    {
        // Arrange
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new DC_bot.Helper.Validation.UserValidationResult(true, string.Empty, new Mock<IDiscordMember>().Object));

        _guildMock.SetupGet(g => g.Id).Returns(987654321L);

        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);


        // Act
        await _clearCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(m => m.SetQueue(987654321L, It.Is<Queue<ILavaLinkTrack>>(q => q.Count == 0)), Times.Once);
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue()
    {
        // Assert
        Assert.Equal(ClearCommandName, _clearCommand.Name);
        Assert.Equal(ClearCommandDescriptionValue, _clearCommand.Description);
    }
}
