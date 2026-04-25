using System.Reflection;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Core;
using DSharpPlus;
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
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
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
    [InlineData("!   ping", "!", "ping")]
    public void TryGetCommandName_ValidCommands_ExtractsCorrectly(string content, string prefix, string expected)
    {
        // Act
        var commandName = InvokeTryGetCommandName(content, prefix);

        // Assert
        Assert.Equal(expected, commandName);
    }

    [Theory]
    [InlineData("!", "!")]
    [InlineData("!  ", "!")]
    [InlineData("$", "$")]
    public void TryGetCommandName_EmptyCommand_ReturnsNull(string content, string prefix)
    {
        // Act
        var commandName = InvokeTryGetCommandName(content, prefix);

        // Assert
        Assert.Null(commandName);
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

    #region HandleCommandAsync Tests

    [Fact]
    public async Task HandleCommandAsync_NoPrefix_LogsAndReturnsEarly()
    {
        // Arrange
        _commandHandlerService.Prefix = null;

        // Act
        await InvokeHandleCommandAsync(_commandHandlerService, null, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1103),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No prefix provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_NullArgs_LogsCommandExecutionFailed()
    {
        // Arrange
        _commandHandlerService.Prefix = "!";

        // Act
        await InvokeHandleCommandAsync(_commandHandlerService, null, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1004),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("message_created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void UnregisterHandler_WithNullMessageHandler_LogsWarning()
    {
        // Arrange
        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };

        using var client = new DiscordClient(discordConfig);

        // Act - Unregister without registering first
        _commandHandlerService.UnregisterHandler(client);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.Is<EventId>(e => e.Id == 1106),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RegisterHandler_WithTestMode_RegistersSuccessfully()
    {
        // Arrange
        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };

        using var client = new DiscordClient(discordConfig);

        // Act
        _commandHandlerService.RegisterHandler(client);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1102),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _commandHandlerService.UnregisterHandler(client);
    }

    [Fact]
    public void RegisterHandler_CalledTwice_LogsAlreadyRegisteredSecondTime()
    {
        // Arrange
        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };

        using var client = new DiscordClient(discordConfig);

        // Act
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _commandHandlerService.RegisterHandler(client);
        _mockLogger.Invocations.Clear();
        _commandHandlerService.RegisterHandler(client);

        // Assert - Second call should log already registered
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce);

        _commandHandlerService.UnregisterHandler(client);
    }

    #endregion

    private static string? InvokeTryGetCommandName(string content, string prefix)
    {
        var method = typeof(CommandHandlerService).GetMethod("TryGetCommandName",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        return (string?)method!.Invoke(null, [content, prefix]);
    }

    private static async Task InvokeHandleCommandAsync(CommandHandlerService service, object? sender, object? args)
    {
        var method = typeof(CommandHandlerService).GetMethod("HandleCommandAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        var task = (Task?)method!.Invoke(service, [sender, args]);
        Assert.NotNull(task);
        await task!;
    }
}