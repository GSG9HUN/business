using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Service.Music;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service;

[Collection("Integration Tests")]
public class ReactionHandlerIntegrationTests : IAsyncLifetime
{
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock = new();

    private readonly DiscordClient _discordClient;
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<ILogger<LavaLinkService>> _loggerLavalinkServiceMock = new();
    private readonly Mock<ILogger<ReactionHandler>> _loggerReactionHandlerMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionMock = new();
    private readonly Mock<IProgressiveTimerService> _progressiveTimerServiceMock = new();
    private readonly Mock<IRepeatService> _repeatServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<ITrackNotificationService> _trackNotificationMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlayBackMock = new();
    private readonly Mock<ILogger<ValidationService>> _validationLoggerMock = new();

    public ReactionHandlerIntegrationTests()
    {
        // LoggerMessage-generated extensions check IsEnabled before logging.
        _loggerReactionHandlerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _loggerLavalinkServiceMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _validationLoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName ??
                            "";

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        var botSettings = new BotSettings
            { Token = string.IsNullOrWhiteSpace(envToken) ? "fake-test-token" : envToken, Prefix = "!" };

        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.SkipCommandResponse))
            .Returns("Now Playing: ");
        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.MusicControl))
            .Returns("Music Controls");

        var validationService = new ValidationService(_validationLoggerMock.Object, true);
        var lavalinkService = new LavaLinkService(_musicQueueServiceMock.Object, _loggerLavalinkServiceMock.Object,
            _audioServiceMock.Object, _responseBuilderMock.Object, _localizationServiceMock.Object,
            _repeatServiceMock.Object, _currentTrackServiceMock.Object, _trackNotificationMock.Object,
            _playerConnectionMock.Object, _playbackEventHandlerMock.Object, _progressiveTimerServiceMock.Object,
            _trackPlayBackMock.Object);

        var reactionHandlerService = new ReactionHandler(lavalinkService,
            _loggerReactionHandlerMock.Object, _progressiveTimerServiceMock.Object, _localizationServiceMock.Object);

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton<IValidationService>(validationService)
            .AddSingleton(lavalinkService)
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<DiscordClient>(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>(),
                provider.GetRequiredService<DiscordClientEventHandler>()))
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton(reactionHandlerService)
            .BuildServiceProvider();

        _discordClient = _serviceProvider.GetRequiredService<DiscordClient>();
    }

    public async Task InitializeAsync()
    {
        await _discordClient.ConnectAsync();
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        var reactionHandler = _serviceProvider.GetRequiredService<ReactionHandler>();
        reactionHandler.UnregisterHandler(_discordClient);
        await _discordClient.DisconnectAsync();
        await _serviceProvider.DisposeAsync();
        _discordClient.Dispose();
    }

    // Unit tests for ReactionHandler lifecycle
    [Fact]
    public void RegisterHandler_LogsRegistrationMessage()
    {
        var reactionHandler = _serviceProvider.GetRequiredService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered reaction handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnregisterHandler(_discordClient);
    }

    [Fact]
    public void RegisterHandler_CalledTwice_LogsAlreadyRegistered()
    {
        var reactionHandler = _serviceProvider.GetRequiredService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);
        reactionHandler.RegisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ReactionHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnregisterHandler(_discordClient);
    }

    [Fact]
    public void UnregisterHandler_WithoutRegister_LogsWarning()
    {
        var reactionHandler = _serviceProvider.GetRequiredService<ReactionHandler>();
        reactionHandler.UnregisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Tried to unregister handlers, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void UnregisterHandler_AfterRegister_LogsUnregistration()
    {
        var reactionHandler = _serviceProvider.GetRequiredService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);

        reactionHandler.UnregisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unregistered reaction handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void ReactionHandler_SubscribeEvents_Successfully()
    {
        var reactionHandler = _serviceProvider.GetRequiredService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1202),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered reaction handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnregisterHandler(_discordClient);
    }
}