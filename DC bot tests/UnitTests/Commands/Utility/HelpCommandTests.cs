using DC_bot.Commands.Utility;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Utility;

public class HelpCommandTests
{
    private const string CommandNamePing = "ping";
    private const string CommandNamePlay = "play";
    private const string CommandDescriptionPing = "Replies with Pong!";
    private const string CommandDescriptionPlay = "Plays a song";
    private const string HelpCommandName = "help";
    private const string HelpCommandDescriptionValue = "Lists all available commands.";
    private const string HelpCommandResponseValue = "Available commands:";
    private const string ExpectedCommandsHeader = "Available commands:\n";
    private const string ExpectedCommandsList = "Available commands:\nping : Replies with Pong!\nplay : Plays a song\n";
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly HelpCommand _helpCommand;

    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;

    public HelpCommandTests()
    {
        Mock<ILogger<HelpCommand>> mockLogger = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.HelpCommandDescription))
            .Returns(HelpCommandDescriptionValue);

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.HelpCommandResponse))
            .Returns(HelpCommandResponseValue);

        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);

        var services = new ServiceCollection();
        var mockCommand1 = new Mock<ICommand>();
        mockCommand1.Setup(c => c.Name).Returns(CommandNamePing);
        mockCommand1.Setup(c => c.Description).Returns(CommandDescriptionPing);

        var mockCommand2 = new Mock<ICommand>();
        mockCommand2.Setup(c => c.Name).Returns(CommandNamePlay);
        mockCommand2.Setup(c => c.Description).Returns(CommandDescriptionPlay);

        services.AddSingleton(mockCommand1.Object);
        services.AddSingleton(mockCommand2.Object);

        var serviceProvider = services.BuildServiceProvider();

        _helpCommand = new HelpCommand(userValidationService, mockLogger.Object, _responseBuilderMock.Object,
            localizationServiceMock.Object, serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldListCommands_WhenCalled()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);

        _discordMemberMock.SetupGet(dm => dm.IsBot).Returns(false);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        // Act
        await _helpCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object, ExpectedCommandsList), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleNoCommands()
    {
        // Arrange
        Mock<ILogger<HelpCommand>> mockLogger = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.HelpCommandDescription))
            .Returns(HelpCommandDescriptionValue);

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.HelpCommandResponse))
            .Returns(HelpCommandResponseValue);

        var userValidationService = new ValidationService(validationLoggerMock.Object);

        var services = new ServiceCollection();
        var emptyServiceProvider = services.BuildServiceProvider();

        var helpCommandWithNoCommands = new HelpCommand(userValidationService, mockLogger.Object,
            _responseBuilderMock.Object, localizationServiceMock.Object, emptyServiceProvider);

        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);

        _discordMemberMock.SetupGet(dm => dm.IsBot).Returns(false);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        // Act
        await helpCommandWithNoCommands.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object, ExpectedCommandsHeader), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _helpCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(HelpCommandName, _helpCommand.Name);
        Assert.Equal(HelpCommandDescriptionValue, _helpCommand.Description);
    }
}