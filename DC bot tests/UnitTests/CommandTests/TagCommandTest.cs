using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class TagCommandTest
{
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly TagCommand _tagCommand;

    public TagCommandTest()
    {
        Mock<ILogger<UserValidationService>> userLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();
        
        localizationServiceMock.Setup(g => g.Get("tag_command_description"))
            .Returns("You can tag someone.");
        
        localizationServiceMock.Setup(g => g.Get("tag_command_usage"))
            .Returns("Username provided.");
        
        localizationServiceMock.Setup(g => g.Get("tag_command_response", "TestUser"))
            .Returns("TestUser Wake Up!");
        
        localizationServiceMock.Setup(g => g.Get("tag_command_user_not_exist_error", "test"))
            .Returns("User test does not exist.");

        var logger = new Mock<ILogger<TagCommand>>();

        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _discordUserMock = new Mock<IDiscordUser>();

        var mockUserValidation = new UserValidationService(userLoggerMock.Object, localizationServiceMock.Object);
        _tagCommand = new TagCommand(mockUserValidation, logger.Object, localizationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_User_Not_Provided_Username()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag");

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.Channel.SendMessageAsync("Username provided."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username()
    {
        //Arrange
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns("TestUser");
        discordMemberMock.SetupGet(dm => dm.Mention).Returns("TestUser");

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.Channel.SendMessageAsync($"{discordMemberMock.Object.Mention} Wake Up!"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Wrong_Username()
    {
        //Arrange
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns("TestUser");
        discordMemberMock.SetupGet(dm => dm.Mention).Returns("TestUser");

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag test");

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.Channel.SendMessageAsync("User test does not exist."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.Channel.SendMessageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("tag", _tagCommand.Name);
        Assert.Equal("You can tag someone.", _tagCommand.Description);
    }
}