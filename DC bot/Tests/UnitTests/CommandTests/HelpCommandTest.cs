using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.UnitTests.CommandTests;

public class HelpCommandTests
{
    private readonly Mock<IDiscordMessage> _mockMessage;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly HelpCommand _helpCommand;

    public HelpCommandTests()
    {
        Mock<ILogger<HelpCommand>> mockLogger = new();
        Mock<ILogger<UserValidationService>> userLogger = new();
        
        _mockMessage = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        var userValidationService = new UserValidationService(userLogger.Object);

        var services = new ServiceCollection();
        var mockCommand1 = new Mock<ICommand>();
        mockCommand1.Setup(c => c.Name).Returns("ping");
        mockCommand1.Setup(c => c.Description).Returns("Replies with Pong!");

        var mockCommand2 = new Mock<ICommand>();
        mockCommand2.Setup(c => c.Name).Returns("play");
        mockCommand2.Setup(c => c.Description).Returns("Plays a song");

        services.AddSingleton(mockCommand1.Object);
        services.AddSingleton(mockCommand2.Object);

        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.SetServiceProvider(serviceProvider);

        _helpCommand = new HelpCommand(userValidationService, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldListCommands_WhenCalled()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);

        _discordMemberMock.SetupGet(dm => dm.IsBot).Returns(false);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _mockMessage.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _mockMessage.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        // Act
        await _helpCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMessage.Verify(m => m.RespondAsync("Available commands:\n" +
                                                "ping : Replies with Pong!\n" +
                                                "play : Plays a song\n"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleNoCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);

        _discordMemberMock.SetupGet(dm => dm.IsBot).Returns(false);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _mockMessage.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _mockMessage.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        ServiceLocator.SetServiceProvider(serviceProvider);

        // Act
        await _helpCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMessage.Verify(m => m.RespondAsync("Available commands:\n"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
            
        _mockMessage.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _mockMessage.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        
        //Act
        await _helpCommand.ExecuteAsync(_mockMessage.Object);

        //Assert
        _mockMessage.Verify(m => m.RespondAsync(It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("help", _helpCommand.Name);
        Assert.Equal("Lists all available commands.", _helpCommand.Description);
    }
}