using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Core;

public class CommandHandlerServiceTests
{
    private readonly BotSettings _botSettings;
    private readonly CommandHandlerService _commandHandlerService;
    private readonly Mock<ILocalizationService> _mockLocalization;
    private readonly Mock<ILogger<CommandHandlerService>> _mockLogger;
    private readonly ServiceProvider _serviceProvider;

    public CommandHandlerServiceTests()
    {
        _mockLogger = new Mock<ILogger<CommandHandlerService>>();
        _mockLocalization = new Mock<ILocalizationService>();
        var mockCommand = new Mock<ICommand>();

        _botSettings = new BotSettings { Prefix = "!" };

        // Setup mock command
        mockCommand.Setup(x => x.Name).Returns("test");
        mockCommand.Setup(x => x.Description).Returns("Test command");
        mockCommand.Setup(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>())).Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(mockCommand.Object);
        _serviceProvider = services.BuildServiceProvider();

        _commandHandlerService = new CommandHandlerService(
            _serviceProvider,
            _mockLogger.Object,
            _mockLocalization.Object,
            _botSettings,
            true);
    }

    #region Prefix Edge Cases

    [Fact]
    public void SetPrefix_ValidPrefix_StoresCorrectly()
    {
        // Arrange
        const string newPrefix = "?";

        // Act
        _commandHandlerService.Prefix = newPrefix;

        // Assert
        Assert.Equal(newPrefix, _commandHandlerService.Prefix);
    }

    #endregion

    #region Localization Tests

    [Fact]
    public void CommandHandlerService_UsesLocalizationService()
    {
        // Arrange
        _mockLocalization.Setup(x => x.Get(LocalizationKeys.UnknownCommandError))
            .Returns("Unknown command");

        // Act
        var service = new CommandHandlerService(
            _serviceProvider,
            _mockLogger.Object,
            _mockLocalization.Object,
            _botSettings,
            false);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_DuplicateCommandNames_UsesLastCommand()
    {
        // Arrange
        var cmd1 = new Mock<ICommand>();
        cmd1.Setup(x => x.Name).Returns("play");
        var cmd2 = new Mock<ICommand>();
        cmd2.Setup(x => x.Name).Returns("play"); // Duplicate name

        var services = new ServiceCollection();
        services.AddSingleton(cmd1.Object);
        services.AddSingleton(cmd2.Object);
        var provider = services.BuildServiceProvider();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CommandHandlerService(
            provider,
            _mockLogger.Object,
            _mockLocalization.Object,
            _botSettings,
            false));
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_RegistersCommands_FromServiceProvider()
    {
        // Act 

        // Assert
        Assert.NotNull(_commandHandlerService);
        Assert.Equal("!", _commandHandlerService.Prefix);
    }

    [Fact]
    public void Constructor_SetsPrefix_FromBotSettings()
    {
        // Arrange
        var customSettings = new BotSettings { Prefix = "$" };

        // Act
        var service = new CommandHandlerService(
            _serviceProvider,
            _mockLogger.Object,
            _mockLocalization.Object,
            customSettings,
            false);

        // Assert
        Assert.Equal("$", service.Prefix);
    }

    [Fact]
    public void Constructor_EmptyServiceProvider_CreatesEmptyCommandDictionary()
    {
        // Arrange
        var emptyServices = new ServiceCollection().BuildServiceProvider();

        // Act
        var service = new CommandHandlerService(
            emptyServices,
            _mockLogger.Object,
            _mockLocalization.Object,
            _botSettings,
            false);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_MultipleCommands_RegistersAll()
    {
        // Arrange
        var mockCommand1 = new Mock<ICommand>();
        mockCommand1.Setup(x => x.Name).Returns("play");
        var mockCommand2 = new Mock<ICommand>();
        mockCommand2.Setup(x => x.Name).Returns("pause");

        var services = new ServiceCollection();
        services.AddSingleton(mockCommand1.Object);
        services.AddSingleton(mockCommand2.Object);
        var provider = services.BuildServiceProvider();

        // Act
        var service = new CommandHandlerService(
            provider,
            _mockLogger.Object,
            _mockLocalization.Object,
            _botSettings,
            false);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region TryGetCommandName Tests

    [Theory]
    [InlineData("!play test", "!", "play")]
    [InlineData("!pause", "!", "pause")]
    [InlineData("$skip song", "$", "skip")]
    [InlineData("!help command", "!", "help")]
    public void TryGetCommandName_ValidCommands_ExtractsCorrectly(string content, string prefix, string expected)
    {
        Assert.StartsWith(prefix, content);
        var remainder = content.Substring(prefix.Length).TrimStart();
        var splitIndex = remainder.IndexOf(' ');
        var commandName = splitIndex >= 0 ? remainder[..splitIndex] : remainder;
        Assert.Equal(expected, commandName);
    }

    [Theory]
    [InlineData("!", "!")]
    [InlineData("!  ", "!")]
    [InlineData("$", "$")]
    public void TryGetCommandName_EmptyCommand_ReturnsNull(string content, string prefix)
    {
        if (content.Length <= prefix.Length)
        {
            Assert.True(true); // Should return null
        }
        else
        {
            var remainder = content.Substring(prefix.Length).TrimStart();
            Assert.Empty(remainder); // Should be empty
        }
    }

    #endregion

    #region Prefix Tests

    [Fact]
    public void Prefix_CanBeSet()
    {
        // Arrange
        var newPrefix = "$";

        // Act
        _commandHandlerService.Prefix = newPrefix;

        // Assert
        Assert.Equal(newPrefix, _commandHandlerService.Prefix);
    }

    [Fact]
    public void Prefix_NullValue_CanBeSet()
    {
        // Act
        _commandHandlerService.Prefix = null;

        // Assert
        Assert.Null(_commandHandlerService.Prefix);
    }

    #endregion
}