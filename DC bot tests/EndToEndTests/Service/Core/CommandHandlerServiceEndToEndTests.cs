using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DC_bot.Service.Presentation;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.EndToEndTests.Service.Core;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class CommandHandlerServiceEndToEndTests : IAsyncLifetime
{
    private const string BotPrefix = "!";
    private readonly CommandHandlerService _commandHandlerService;
    private readonly Mock<ILogger<CommandHandlerService>> _commandServiceLoggerMock = new();
    private readonly DiscordClient _discordClient;
    private readonly bool _isConfigured;
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly ServiceProvider _serviceProvider;
    private readonly ulong _testChannelId;
    private bool _isDiscordAvailable;
    private readonly Mock<ILogger<ValidationService>> _validationLoggerMock = new();

    public CommandHandlerServiceEndToEndTests()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);
        var hasChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var testChannelId);
        _testChannelId = testChannelId;
        _isConfigured = hasToken && hasChannel;

        var botSettings = new BotSettings
        { Token = hasToken ? envToken : "fake-test-token", Prefix = BotPrefix };

        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.UnknownCommandError))
            .Returns("Unknown command. Use `!help` to see available commands.");
        _localizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.UnknownCommandError))
            .Returns("Unknown command. Use `!help` to see available commands.");
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.PingCommandDescription))
            .Returns("Replies with Pong.");
        _localizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.PingCommandDescription))
            .Returns("Replies with Pong.");
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.HelpCommandDescription))
            .Returns("Lists the available commands.");
        _localizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.HelpCommandDescription))
            .Returns("Lists the available commands.");
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.HelpCommandResponse))
            .Returns("Available commands:");
        _localizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.HelpCommandResponse))
            .Returns("Available commands:");
        _localizationServiceMock
            .Setup(ls => ls.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>(FormatLocalization);
        _localizationServiceMock
            .Setup(ls => ls.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));
        _localizationServiceMock
            .Setup(ls => ls.Get(
                It.IsAny<ulong>(),
                LocalizationKeys.HelpCommandResponse,
                It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));
        var userValidationService = new ValidationService(_validationLoggerMock.Object, true);

        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<IUserValidationService>(userValidationService)
            .BuildServiceProvider();

        _commandHandlerService = new CommandHandlerService(services.GetRequiredService<ICommandRegistry>(), _commandServiceLoggerMock.Object,
            _localizationServiceMock.Object, botSettings, true);

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton(guildDataRepositoryMock.Object)
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<DiscordClient>(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>()))
            .AddSingleton(_lavaLinkServiceMock.Object)
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton(_commandHandlerService)
            .BuildServiceProvider();

        _discordClient = _serviceProvider.GetRequiredService<DiscordClient>();
    }

    public async Task InitializeAsync()
    {
        if (!_isConfigured) return;
        _isDiscordAvailable = await EndToEndDiscordGuard.TryConnectAndWaitUntilReadyAsync(_discordClient);
    }

    public async Task DisposeAsync()
    {
        if (_isConfigured)
        {
            _commandHandlerService.UnregisterHandler(_discordClient);
            await EndToEndDiscordGuard.DisconnectIgnoringDisconnectedGatewayAsync(_discordClient);
        }

        await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(_serviceProvider);
        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(_discordClient);
    }

    private (Mock<ILogger<CommandHandlerService>> freshLoggerMock, CommandHandlerService freshCommandHandlerService,
        BotSettings botSettings) Init()
    {
        var freshLoggerMock = new Mock<ILogger<CommandHandlerService>>();

        freshLoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);
        var botSettings = new BotSettings
        { Token = hasToken ? envToken : "fake-test-token", Prefix = BotPrefix };

        var userValidationService = new ValidationService(_validationLoggerMock.Object, true);

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .BuildServiceProvider();

        var freshCommandHandlerService = new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            freshLoggerMock.Object,
            _localizationServiceMock.Object,
            botSettings,
            true);

        return (freshLoggerMock, freshCommandHandlerService, botSettings);
    }

    [Fact]
    public void RegisterHandler_ShouldRegisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.RegisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1102),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        freshCommandHandlerService.UnregisterHandler(mockClient);
        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public void UnregisterHandler_ShouldUnregisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.RegisterHandler(mockClient);
        freshCommandHandlerService.UnregisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1105),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unregistered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Test_Message()
    {
        if (!CanRun()) return;
        _commandHandlerService.RegisterHandler(_discordClient);

        var channel = await GetTestChannelAsync(_discordClient);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var testMessage = await channel.SendMessageAsync("!ping");

        Assert.NotNull(testMessage);

        await _commandHandlerService.HandleEventAsync(_discordClient,
            DiscordEventArgsFactory.CreateMessageCreated(testMessage, guild));
        var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
            channel,
            testMessage.Id,
            message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase),
            "Pong response",
            limit: 5);
        Assert.Contains("Pong", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnregisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Responds_To_Unknown_Command()
    {
        if (!CanRun()) return;
        _commandHandlerService.RegisterHandler(_discordClient);

        var channel = await GetTestChannelAsync(_discordClient);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var testMessage = await channel.SendMessageAsync("!unknowncommand");

        Assert.NotNull(testMessage);

        await _commandHandlerService.HandleEventAsync(_discordClient,
            DiscordEventArgsFactory.CreateMessageCreated(testMessage, guild));
        var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
            channel,
            testMessage.Id,
            message => message.Content.Contains("Unknown command.", StringComparison.OrdinalIgnoreCase),
            "unknown command response",
            limit: 5);
        Assert.Contains("Unknown command.", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnregisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Help_Message()
    {
        if (!CanRun()) return;
        _commandHandlerService.RegisterHandler(_discordClient);

        try
        {
            var channel = await GetTestChannelAsync(_discordClient);
            if (channel is null) return;
            var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
            var testMessage = await channel.SendMessageAsync("!help");

            await _commandHandlerService.HandleEventAsync(_discordClient,
                DiscordEventArgsFactory.CreateMessageCreated(testMessage, guild));
            var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
                channel,
                testMessage.Id,
                message => message.Content.Contains("Available commands:", StringComparison.OrdinalIgnoreCase),
                "help command response",
                limit: 5);
            Assert.Contains("ping", response.Content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("help", response.Content, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            _commandHandlerService.UnregisterHandler(_discordClient);
        }
    }

    [Fact]
    public async Task HandleCommandAsync_WhenMessageHasNoPrefix_DoesNotRespond()
    {
        if (!CanRun()) return;
        _commandHandlerService.RegisterHandler(_discordClient);

        try
        {
            var channel = await GetTestChannelAsync(_discordClient);
            if (channel is null) return;
            var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
            var marker = $"e2e-no-prefix-{Guid.NewGuid():N}";
            var markerMessage = await channel.SendMessageAsync(marker);

            await _commandHandlerService.HandleEventAsync(_discordClient,
                DiscordEventArgsFactory.CreateMessageCreated(markerMessage, guild));
            await DiscordMessageWaiter.AssertNoMessageAfterAsync(
                channel,
                markerMessage.Id,
                message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase) ||
                           message.Content.Contains("Unknown command", StringComparison.OrdinalIgnoreCase) ||
                           message.Content.Contains("Available commands", StringComparison.OrdinalIgnoreCase),
                "command response for non-prefixed message",
                quietPeriod: TimeSpan.FromMilliseconds(1200));
        }
        finally
        {
            _commandHandlerService.UnregisterHandler(_discordClient);
        }
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Log_No_Prefix_Provided()
    {
        if (!CanRun()) return;
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
        freshCommandHandlerService.Prefix = null;

        var mockClient = TestDiscordClientFactory.Create(
            botSettings.Token ?? "fake-test-token",
            DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);
        if (!await EndToEndDiscordGuard.TryConnectAndWaitUntilReadyAsync(mockClient))
        {
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
            return;
        }

        try
        {
            freshCommandHandlerService.RegisterHandler(mockClient);
            freshLoggerMock.Invocations.Clear();
            var channel = await GetTestChannelAsync(mockClient);
            if (channel is null) return;
            var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
            var message = await channel.SendMessageAsync("!noPrefix");

            await freshCommandHandlerService.HandleEventAsync(mockClient,
                DiscordEventArgsFactory.CreateMessageCreated(message, guild));

            freshLoggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.Is<EventId>(e => e.Id == 1103),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No prefix provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.AtLeastOnce
            );
        }
        finally
        {
            freshCommandHandlerService.UnregisterHandler(mockClient);
            await EndToEndDiscordGuard.DisconnectIgnoringDisconnectedGatewayAsync(mockClient);
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
        }
    }

    [Fact]
    public void UnregisterCommandHandler_Should_Log_Warning()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.UnregisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.Is<EventId>(e => e.Id == 1106),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Tried to unregister handler, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public void RegisterCommandAsync_Twice_Should_Log_Already_Registered()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var mockClient = TestDiscordClientFactory.Create(botSettings.Token ?? "fake-test-token");

        freshCommandHandlerService.RegisterHandler(mockClient);
        freshCommandHandlerService.RegisterHandler(mockClient);

        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1101),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CommandHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        freshCommandHandlerService.UnregisterHandler(mockClient);
        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
    }

    [Fact]
    public async Task HandleCommandAsync_WhenAuthorIsBot_AndIsTestModeFalse_IgnoresCommand()
    {
        if (!CanRun()) return;
        EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);

        var botSettings = new BotSettings { Token = envToken, Prefix = BotPrefix };
        var loggerMock = new Mock<ILogger<CommandHandlerService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var userValidationService = new ValidationService(_validationLoggerMock.Object);
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, PingCommand>()
            .BuildServiceProvider();

        var nonTestHandler = new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            loggerMock.Object,
            _localizationServiceMock.Object,
            botSettings);

        nonTestHandler.RegisterHandler(_discordClient);

        var channel = await GetTestChannelAsync(_discordClient);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var marker = $"e2e-ignore-bot-{Guid.NewGuid():N}";
        var markerMessage = await channel.SendMessageAsync($"!ping {marker}");

        await nonTestHandler.HandleEventAsync(_discordClient,
            DiscordEventArgsFactory.CreateMessageCreated(markerMessage, guild));
        await DiscordMessageWaiter.AssertNoMessageAfterAsync(
            channel,
            markerMessage.Id,
            message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase),
            "Pong response for bot-authored command",
            quietPeriod: TimeSpan.FromMilliseconds(1200));

        nonTestHandler.UnregisterHandler(_discordClient);
    }

    private async Task<DiscordChannel?> GetTestChannelAsync(DiscordClient client)
    {
        try
        {
            return await client.GetChannelAsync(_testChannelId);
        }
        catch (Exception exception) when (EndToEndDiscordGuard.IsDiscordEnvironmentUnavailable(exception))
        {
            return null;
        }
    }

    private bool CanRun()
    {
        return _isConfigured && _isDiscordAvailable;
    }

    private static string FormatLocalization(string key, object[] args)
    {
        return key switch
        {
            LocalizationKeys.UnknownCommandError => "Unknown command. Use `!help` to see available commands.",
            LocalizationKeys.PingCommandDescription => "Replies with Pong.",
            LocalizationKeys.PingCommandResponse => "Pong!",
            LocalizationKeys.HelpCommandDescription => "Lists the available commands.",
            LocalizationKeys.HelpCommandResponse => args.Length > 0
                ? $"Available commands:{Environment.NewLine}{args[0]}"
                : "Available commands:",
            _ => args.Length == 0 ? key : string.Format(key, args)
        };
    }
}

