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

public class ResumeCommandTests
{
    private const string ResumeCommandName = "resume";
    private const string ResumeCommandDescriptionValue = "Resume the paused song.";
    private const string ResumeCommandResponseValue = "Music resumed:";

    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly ResumeCommand _resumeCommand;
    private readonly Mock<ICommandHelper> _commandHelperMock;

    public ResumeCommandTests()
    {
        Mock<ILogger<ResumeCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.ResumeCommandDescription))
            .Returns(ResumeCommandDescriptionValue);

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.ResumeCommandResponse))
            .Returns(ResumeCommandResponseValue);

        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _commandHelperMock = new Mock<ICommandHelper>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _resumeCommand = new ResumeCommand(_lavaLinkServiceMock.Object, 
            userValidationService, 
            loggerMock.Object, 
            _responseBuilderMock.Object, localizationServiceMock.Object, _commandHelperMock.Object);
    }
    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(true);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(It.IsAny<IUserValidationService>(), It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((DC_bot.Helper.Validation.UserValidationResult?)null);

        //Act
        await _resumeCommand.ExecuteAsync(_messageMock.Object);

        //Assert

        _lavaLinkServiceMock.Verify(l => l.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotIn_VoiceChannel()
    {
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState?)null);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(It.IsAny<IUserValidationService>(), It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((DC_bot.Helper.Validation.UserValidationResult?)null);

        //Act
        await _resumeCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel), Times.Never);
        _lavaLinkServiceMock.Verify(l => l.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserIn_VoiceChannel()
    {
        //Arrange
        var mockDiscordVoiceState = new Mock<IDiscordVoiceState>();
        mockDiscordVoiceState.Setup(vs => vs.Channel).Returns(_channelMock.Object);

        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(d => d.VoiceState).Returns(mockDiscordVoiceState.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(It.IsAny<IUserValidationService>(), It.IsAny<IResponseBuilder>(), It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new DC_bot.Helper.Validation.UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        //Act
        await _resumeCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(l => l.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember>()), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(ResumeCommandName, _resumeCommand.Name);
        Assert.Equal(ResumeCommandDescriptionValue, _resumeCommand.Description);
    }
}