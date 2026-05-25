using DC_bot.Commands.Utility;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Sdk;

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
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.PingCommandDescription))
            .Returns("Replies with Pong.");
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.HelpCommandDescription))
            .Returns("Lists the available commands.");
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.HelpCommandResponse))
            .Returns("Available commands:");
        var userValidationService = new ValidationService(_validationLoggerMock.Object, true);

        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ILocalizationService>(_localizationServiceMock.Object)
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<IUserValidationService>(userValidationService)
            .BuildServiceProvider();

        _commandHandlerService = new CommandHandlerService(services, _commandServiceLoggerMock.Object,
            _localizationServiceMock.Object, botSettings, true);

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IGuildDataRepository>(guildDataRepositoryMock.Object)
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<DiscordClient>(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>(),
                provider.GetRequiredService<DiscordClientEventHandler>()))
            .AddSingleton<ILavaLinkService>(_lavaLinkServiceMock.Object)
            .AddSingleton<IMusicQueueService>(_musicQueueServiceMock.Object)
            .AddSingleton<ILocalizationService>(_localizationServiceMock.Object)
            .AddSingleton<CommandHandlerService>(_commandHandlerService)
            .BuildServiceProvider();

        _discordClient = _serviceProvider.GetRequiredService<DiscordClient>();
    }

    public async Task InitializeAsync()
    {
        if (!_isConfigured) return;

        await _discordClient.ConnectAsync();
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        if (_isConfigured)
        {
            _commandHandlerService.UnregisterHandler(_discordClient);
            await _discordClient.DisconnectAsync();
        }

        await _serviceProvider.DisposeAsync();
        _discordClient.Dispose();
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
            .AddSingleton<ILocalizationService>(_localizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .BuildServiceProvider();

        var freshCommandHandlerService = new CommandHandlerService(
            services,
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

        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

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
        mockClient.Dispose();
    }

    [Fact]
    public void UnregisterHandler_ShouldUnregisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

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

        mockClient.Dispose();
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Test_Message()
    {
        EnsureConfigured();
        _commandHandlerService.RegisterHandler(_discordClient);

        var channel = await _discordClient.GetChannelAsync(_testChannelId);
        var testMessage = await channel.SendMessageAsync("!ping");

        Assert.NotNull(testMessage);

        await Task.Delay(1000);

        var messages = await channel.GetMessagesAsync(1);
        var response = messages.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Pong", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnregisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Responds_To_Unknown_Command()
    {
        EnsureConfigured();
        _commandHandlerService.RegisterHandler(_discordClient);

        var channel = await _discordClient.GetChannelAsync(_testChannelId);
        var testMessage = await channel.SendMessageAsync("!unknowncommand");

        Assert.NotNull(testMessage);

        await Task.Delay(1000);

        var messages = await channel.GetMessagesAsync(1);
        var response = messages.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Unknown command.", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnregisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Help_Message()
    {
        EnsureConfigured();
        _commandHandlerService.RegisterHandler(_discordClient);

        try
        {
            var channel = await _discordClient.GetChannelAsync(_testChannelId);
            var testMessage = await channel.SendMessageAsync("!help");

            await Task.Delay(1000);

            var messages = await channel.GetMessagesAsync(5);
            var response = messages
                .Where(message => message.Id > testMessage.Id)
                .FirstOrDefault(message => message.Content.Contains("Available commands:", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(response);
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
        EnsureConfigured();
        _commandHandlerService.RegisterHandler(_discordClient);

        try
        {
            var channel = await _discordClient.GetChannelAsync(_testChannelId);
            var marker = $"e2e-no-prefix-{Guid.NewGuid():N}";
            var markerMessage = await channel.SendMessageAsync(marker);

            await Task.Delay(1200);

            var recentMessages = await channel.GetMessagesAsync(10);
            Assert.DoesNotContain(recentMessages.Where(message => message.Id > markerMessage.Id),
                message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase) ||
                           message.Content.Contains("Unknown command", StringComparison.OrdinalIgnoreCase) ||
                           message.Content.Contains("Available commands", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _commandHandlerService.UnregisterHandler(_discordClient);
        }
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Log_No_Prefix_Provided()
    {
        EnsureConfigured();
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
        freshCommandHandlerService.Prefix = null;

        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token",
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        };
        var mockClient = new DiscordClient(discordConfig);
        await mockClient.ConnectAsync();

        freshCommandHandlerService.RegisterHandler(mockClient);
        freshLoggerMock.Invocations.Clear();
        var channel = await mockClient.GetChannelAsync(_testChannelId);
        await channel.SendMessageAsync("!noPrefix");

        await Task.Delay(3000);

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

        freshCommandHandlerService.UnregisterHandler(mockClient);
        await mockClient.DisconnectAsync();
        mockClient.Dispose();
    }

    [Fact]
    public void UnregisterCommandHandler_Should_Log_Warning()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

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

        mockClient.Dispose();
    }

    [Fact]
    public void RegisterCommandAsync_Twice_Should_Log_Already_Registered()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();

        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

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
        mockClient.Dispose();
    }

    [Fact]
    public async Task HandleCommandAsync_WhenAuthorIsBot_AndIsTestModeFalse_IgnoresCommand()
    {
        EnsureConfigured();
        EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);

        var botSettings = new BotSettings { Token = envToken, Prefix = BotPrefix };
        var loggerMock = new Mock<ILogger<CommandHandlerService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var userValidationService = new ValidationService(_validationLoggerMock.Object);
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton<ILocalizationService>(_localizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<ICommand, PingCommand>()
            .BuildServiceProvider();

        var nonTestHandler = new CommandHandlerService(
            services,
            loggerMock.Object,
            _localizationServiceMock.Object,
            botSettings);

        nonTestHandler.RegisterHandler(_discordClient);

        var channel = await _discordClient.GetChannelAsync(_testChannelId);
        var marker = $"e2e-ignore-bot-{Guid.NewGuid():N}";
        var markerMessage = await channel.SendMessageAsync($"!ping {marker}");

        await Task.Delay(1200);

        var recentMessages = await channel.GetMessagesAsync(10);
        Assert.DoesNotContain(recentMessages.Where(m => m.Id > markerMessage.Id),
            m => m.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase));

        nonTestHandler.UnregisterHandler(_discordClient);
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured)
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndChannelMessage());
    }
}
