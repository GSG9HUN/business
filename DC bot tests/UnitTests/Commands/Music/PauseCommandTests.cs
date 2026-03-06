using DC_bot.Commands.Music;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Music;

public class PauseCommandTests
{
    private const string PauseCommandName = "pause";
    private const string PauseCommandDescriptionValue = "Pause the current music.";
    
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly PauseCommand _pauseCommand;
    private readonly Mock<ICommandHelper> _commandHelperMock;

    public PauseCommandTests()
    {
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILogger<PauseCommand>> loggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.PauseCommandDescription))
            .Returns(PauseCommandDescriptionValue);

        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _commandHelperMock = new Mock<ICommandHelper>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _pauseCommand = new PauseCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object,
            _responseBuilderMock.Object, localizationServiceMock.Object,_commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        // Arrange
        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(It.IsAny<IUserValidationService>(), It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((DC_bot.Helper.Validation.UserValidationResult?)null);

        // Act
        await _pauseCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(l => l.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotInVoiceChannel_ShouldSendErrorMessage()
    {
        // Arrange
        IDiscordVoiceState? mockDiscordVoiceState = null;

        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);

        _discordMemberMock.Setup(dm => dm.VoiceState).Returns(mockDiscordVoiceState);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(It.IsAny<IUserValidationService>(), It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((DC_bot.Helper.Validation.UserValidationResult?)null);

        //Act
        await _pauseCommand.ExecuteAsync(_messageMock.Object);

        // Assert

        _responseBuilderMock.Verify(r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel), Times.Never);
        _lavaLinkServiceMock.Verify(l => l.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserInVoiceChannel_ShouldPauseMusic()
    {
        // Arrange
        var voiceChannel = new Mock<IDiscordVoiceState>();

        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);

        _discordMemberMock.SetupGet(dm => dm.IsBot).Returns(false);
        _discordMemberMock.Setup(dm => dm.VoiceState).Returns(voiceChannel.Object);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        voiceChannel.SetupGet(vc => vc.Channel).Returns(_channelMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(It.IsAny<IUserValidationService>(), It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new DC_bot.Helper.Validation.UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        // Act
        await _pauseCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(l => l.PauseAsync(_messageMock.Object, _discordMemberMock.Object), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(PauseCommandName, _pauseCommand.Name);
        Assert.Equal(PauseCommandDescriptionValue, _pauseCommand.Description);
    }
}