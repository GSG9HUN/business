using DC_bot.Commands.Music;
using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Music;

public class JoinCommandTests
{
    private const string JoinCommandName = "join";
    private const string JoinCommandDescriptionValue = "The bot join to your voice channel and start playing queue music if its exists.";

    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly JoinCommand _joinCommand;
    private readonly Mock<ICommandHelper> _commandHelperMock;

    public JoinCommandTests()
    {
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.JoinCommandDescription))
            .Returns(JoinCommandDescriptionValue);

        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var joinCommandLoggerMock = new Mock<ILogger<JoinCommand>>();
       
        _commandHelperMock = new Mock<ICommandHelper>();
        var responseBuilderMock = new Mock<IResponseBuilder>();
        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _joinCommand = new JoinCommand(_lavaLinkServiceMock.Object, userValidationService,
            joinCommandLoggerMock.Object, responseBuilderMock.Object, localizationServiceMock.Object, _commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        // Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(true);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((UserValidationResult?)null);

        // Act
        await _joinCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(
            l => l.StartPlayingQueue(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordChannel>(), It.IsAny<IDiscordMember>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotIn_VoiceChannel()
    {
        // Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState?)null);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((UserValidationResult?)null);

        // Act
        await _joinCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(
            l => l.StartPlayingQueue(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordChannel>(), It.IsAny<IDiscordMember>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinCommand_Should_Execute_As_Expected()
    {
        // Arrange
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(_channelMock.Object);

        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        // Act
        await _joinCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(
            l => l.StartPlayingQueue(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordChannel>(), It.IsAny<IDiscordMember>()),
            Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(JoinCommandName, _joinCommand.Name);
        Assert.Equal(JoinCommandDescriptionValue, _joinCommand.Description);
    }
}