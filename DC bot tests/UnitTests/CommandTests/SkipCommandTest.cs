using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class SkipCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly SkipCommand _skipCommand;

    public SkipCommandTest()
    {
        Mock<ILogger<SkipCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();
        
        localizationServiceMock.Setup(g => g.Get("user_not_in_a_voice_channel"))
            .Returns("You must be in a voice channel!");
        
        localizationServiceMock.Setup(g => g.Get("skip_command_description"))
            .Returns("Skip the current track.");
        
        localizationServiceMock.Setup(g => g.Get("skip_command_error"))
            .Returns("No track is currently playing.");
        
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        
        var userValidationService = new ValidationService(localizationServiceMock.Object,validationLoggerMock.Object);
        _skipCommand = new SkipCommand(_lavaLinkServiceMock.Object,userValidationService, loggerMock.Object,localizationServiceMock.Object);
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

        //Act
        await _skipCommand.ExecuteAsync(_messageMock.Object);

        //Assert

        _lavaLinkServiceMock.Verify(l => l.SkipAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotIn_VoiceChannel()
    {
        //Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState?)null);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _skipCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync("You must be in a voice channel!"), Times.Once);
        _lavaLinkServiceMock.Verify(l => l.SkipAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserIn_VoiceChannel()
    {
        //Arrange
        var mockDiscordVoiceState = new Mock<IDiscordVoiceState>();
        mockDiscordVoiceState.Setup(vs => vs.Channel).Returns(_channelMock.Object);
        
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(mockDiscordVoiceState.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _skipCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(l => l.SkipAsync(It.IsAny<IDiscordChannel>()), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("skip", _skipCommand.Name);
        Assert.Equal("Skip the current track.", _skipCommand.Description);
    }
}